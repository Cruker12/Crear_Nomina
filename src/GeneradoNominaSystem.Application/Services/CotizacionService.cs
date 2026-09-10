using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Comun;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.Interfaces.Services;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Application.Services;

public sealed class CotizacionService : ICotizacionService
{
    private readonly ICotizacionRepository _cotizaciones;
    private readonly IProductoServicioRepository _productos;
    private readonly IPlantillaCotizacionRepository _plantillas;
    private readonly IUnitOfWork _uow;
    private readonly IServicioNumeracion _numeracion;
    private readonly IValidator<CotizacionDto> _validator;
    private readonly IValidator<DetalleCotizacionDto> _detalleValidator;

    public CotizacionService(
        ICotizacionRepository cotizaciones,
        IProductoServicioRepository productos,
        IPlantillaCotizacionRepository plantillas,
        IUnitOfWork uow,
        IServicioNumeracion numeracion,
        IValidator<CotizacionDto> validator,
        IValidator<DetalleCotizacionDto> detalleValidator)
    {
        _cotizaciones = cotizaciones;
        _productos = productos;
        _plantillas = plantillas;
        _uow = uow;
        _numeracion = numeracion;
        _validator = validator;
        _detalleValidator = detalleValidator;
    }

    public async Task<CotizacionDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _cotizaciones.ObtenerConDetallesAsync(id, ct);
        return entidad is null ? null : await MapearAsync(entidad, ct);
    }

    public async Task<IReadOnlyList<CotizacionDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        var entidades = await _cotizaciones.ListarPorEmpresaAsync(empresaId, ct);
        var lista = new List<CotizacionDto>();
        foreach (var c in entidades)
        {
            var completa = await _cotizaciones.ObtenerConDetallesAsync(c.Id, ct);
            if (completa is not null)
            {
                lista.Add(await MapearAsync(completa, ct));
            }
        }

        return lista;
    }

    public async Task<CotizacionDto> CrearAsync(CotizacionDto dto, CancellationToken ct = default)
    {
        dto.ClienteNombre = DatoDesconocido.Normalizar(dto.ClienteNombre) ?? string.Empty;
        dto.ClienteDocumento = DatoDesconocido.Normalizar(dto.ClienteDocumento);
        dto.ClienteEmail = DatoDesconocido.Normalizar(dto.ClienteEmail);
        dto.ClienteTelefono = DatoDesconocido.Normalizar(dto.ClienteTelefono);
        dto.Observaciones = DatoDesconocido.Normalizar(dto.Observaciones);

        var resultado = await _validator.ValidateAsync(dto, ct);
        if (!resultado.IsValid)
        {
            throw new ValidationException($"Cotización inválida: {string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage))}");
        }

        var existentes = await _cotizaciones.ListarPorEmpresaAsync(dto.EmpresaId, ct);
        var numero = _numeracion.GenerarNumero("COT", DateTime.UtcNow.Year, existentes.Count + 1);

        Guid? plantillaId = null;
        if (dto.PlantillaCotizacionId.HasValue)
        {
            var plantilla = await _plantillas.ObtenerPorIdAsync(dto.PlantillaCotizacionId.Value, ct);
            if (plantilla is null || plantilla.EmpresaId != dto.EmpresaId || !plantilla.Activo)
            {
                throw new ReglaNegocioException("La plantilla no existe, no está activa o no pertenece a esta empresa.");
            }

            plantillaId = plantilla.Id;
        }

        var entidad = new Cotizacion(
            dto.EmpresaId,
            dto.ClienteNombre,
            numero,
            dto.FechaEmision,
            dto.FechaVigencia,
            dto.Moneda,
            plantillaId);

        entidad.ActualizarDatosCliente(dto.ClienteDocumento, dto.ClienteEmail, dto.ClienteTelefono, dto.Observaciones);

        await _cotizaciones.AgregarAsync(entidad, ct);
        await _uow.GuardarCambiosAsync(ct);

        return await MapearAsync(entidad, ct);
    }

    public async Task<CotizacionDto> AgregarDetalleAsync(
        Guid cotizacionId,
        string descripcion,
        decimal cantidad,
        decimal precioUnitarioMonto,
        string moneda,
        Guid? productoServicioId = null,
        decimal? descuentoPorcentaje = null,
        CancellationToken ct = default)
    {
        var cotizacion = await _cotizaciones.ObtenerConDetallesAsync(cotizacionId, ct);
        if (cotizacion is null)
        {
            throw new ReglaNegocioException("La cotización no existe.");
        }

        if (productoServicioId.HasValue)
        {
            var producto = await _productos.ObtenerPorIdAsync(productoServicioId.Value, ct);
            if (producto is null || !producto.Activo)
            {
                throw new ReglaNegocioException("El producto/servicio no existe o está inactivo.");
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                descripcion = producto.Nombre;
            }
        }

        var dto = new DetalleCotizacionDto
        {
            Descripcion = descripcion,
            Cantidad = cantidad,
            PrecioUnitarioMonto = precioUnitarioMonto,
            PrecioUnitarioMoneda = moneda,
            DescuentoPorcentaje = descuentoPorcentaje,
            Orden = cotizacion.Detalles.Count,
        };

        var validacion = await _detalleValidator.ValidateAsync(dto, ct);
        if (!validacion.IsValid)
        {
            throw new ValidationException($"Detalle inválido: {string.Join("; ", validacion.Errors.Select(e => e.ErrorMessage))}");
        }

        var detalle = new DetalleCotizacion(
            cotizacion.Id,
            descripcion,
            cantidad,
            new Dinero(precioUnitarioMonto, moneda),
            cotizacion.Detalles.Count,
            productoServicioId,
            descuentoPorcentaje);

        cotizacion.AgregarDetalle(detalle);
        await _cotizaciones.AgregarDetalleAsync(detalle, ct);
        await _uow.GuardarCambiosAsync(ct);

        var recargada = await _cotizaciones.ObtenerConDetallesAsync(cotizacion.Id, ct);
        return await MapearAsync(recargada!, ct);
    }

    public async Task QuitarDetalleAsync(Guid cotizacionId, Guid detalleId, CancellationToken ct = default)
    {
        var cotizacion = await _cotizaciones.ObtenerConDetallesAsync(cotizacionId, ct);
        if (cotizacion is null)
        {
            throw new ReglaNegocioException("La cotización no existe.");
        }

        cotizacion.RemoverDetalle(detalleId);
        // Sin Actualizar(): la cotización está tracked; EF detecta el hijo eliminado.
        await _uow.GuardarCambiosAsync(ct);
    }

    public async Task CambiarEstadoAsync(Guid cotizacionId, EstadoCotizacion nuevoEstado, CancellationToken ct = default)
    {
        var cotizacion = await _cotizaciones.ObtenerPorIdAsync(cotizacionId, ct);
        if (cotizacion is null)
        {
            throw new ReglaNegocioException("La cotización no existe.");
        }

        cotizacion.CambiarEstado(nuevoEstado);
        _cotizaciones.Actualizar(cotizacion);
        await _uow.GuardarCambiosAsync(ct);
    }

    public async Task AsignarPlantillaAsync(Guid cotizacionId, Guid? plantillaId, CancellationToken ct = default)
    {
        var cotizacion = await _cotizaciones.ObtenerPorIdAsync(cotizacionId, ct);
        if (cotizacion is null)
        {
            throw new ReglaNegocioException("La cotización no existe.");
        }

        if (plantillaId.HasValue)
        {
            var plantilla = await _plantillas.ObtenerPorIdAsync(plantillaId.Value, ct);
            if (plantilla is null || plantilla.EmpresaId != cotizacion.EmpresaId || !plantilla.Activo)
            {
                throw new ReglaNegocioException("La plantilla no existe, no está activa o no pertenece a esta empresa.");
            }
        }

        cotizacion.AsignarPlantilla(plantillaId);
        _cotizaciones.Actualizar(cotizacion);
        await _uow.GuardarCambiosAsync(ct);
    }

    private async Task<CotizacionDto> MapearAsync(Cotizacion c, CancellationToken ct)
    {
        var dto = new CotizacionDto
        {
            Id = c.Id,
            EmpresaId = c.EmpresaId,
            ClienteNombre = c.ClienteNombre,
            ClienteDocumento = c.ClienteDocumento,
            ClienteEmail = c.ClienteEmail,
            ClienteTelefono = c.ClienteTelefono,
            NumeroCotizacion = c.NumeroCotizacion,
            FechaEmision = c.FechaEmision,
            FechaVigencia = c.FechaVigencia,
            Estado = c.Estado,
            Observaciones = c.Observaciones,
            Subtotal = c.Subtotal.Monto,
            TotalNeto = c.TotalNeto.Monto,
            Moneda = c.Moneda,
            PlantillaCotizacionId = c.PlantillaCotizacionId,
        };

        foreach (var d in c.Detalles.OrderBy(x => x.Orden))
        {
            string nombreProducto = string.Empty;
            if (d.ProductoServicioId.HasValue)
            {
                var producto = await _productos.ObtenerPorIdAsync(d.ProductoServicioId.Value, ct);
                nombreProducto = producto?.Nombre ?? string.Empty;
            }

            dto.Detalles.Add(new DetalleCotizacionDto
            {
                Id = d.Id,
                ProductoServicioId = d.ProductoServicioId,
                ProductoNombre = nombreProducto,
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitarioMonto = d.PrecioUnitario.Monto,
                PrecioUnitarioMoneda = d.PrecioUnitario.Moneda,
                DescuentoPorcentaje = d.DescuentoPorcentaje,
                Subtotal = d.Subtotal.Monto,
                Orden = d.Orden,
            });
        }

        return dto;
    }
}
