using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.Services;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Application.Services;

public sealed class NominaService : INominaService
{
    private readonly INominaRepository _nominas;
    private readonly IEmpleadoRepository _empleados;
    private readonly IPeriodoNominaRepository _periodos;
    private readonly IConceptoNominaRepository _conceptos;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<DetalleNominaDto> _detalleValidator;
    private readonly ServicioCalculoNomina _motor = new();

    public NominaService(
        INominaRepository nominas,
        IEmpleadoRepository empleados,
        IPeriodoNominaRepository periodos,
        IConceptoNominaRepository conceptos,
        IUnitOfWork uow,
        IValidator<DetalleNominaDto> detalleValidator)
    {
        _nominas = nominas;
        _empleados = empleados;
        _periodos = periodos;
        _conceptos = conceptos;
        _uow = uow;
        _detalleValidator = detalleValidator;
    }

    public async Task<NominaDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _nominas.ObtenerConDetallesAsync(id, ct);
        return entidad is null ? null : await MapearAsync(entidad, ct);
    }

    public async Task<IReadOnlyList<NominaDto>> ListarPorPeriodoAsync(Guid periodoId, CancellationToken ct = default)
    {
        var entidades = await _nominas.ListarPorPeriodoAsync(periodoId, ct);
        var lista = new List<NominaDto>();
        foreach (var n in entidades)
        {
            var completa = await _nominas.ObtenerConDetallesAsync(n.Id, ct);
            if (completa is not null)
            {
                lista.Add(await MapearAsync(completa, ct));
            }
        }

        return lista;
    }

    public async Task<IReadOnlyList<NominaDto>> ListarPorEmpleadoAsync(Guid empleadoId, CancellationToken ct = default)
    {
        var entidades = await _nominas.ListarPorEmpleadoAsync(empleadoId, ct);
        var lista = new List<NominaDto>();
        foreach (var n in entidades)
        {
            var completa = await _nominas.ObtenerConDetallesAsync(n.Id, ct);
            if (completa is not null)
            {
                lista.Add(await MapearAsync(completa, ct));
            }
        }

        return lista;
    }

    public async Task<NominaDto> CrearAsync(Guid empresaId, Guid empleadoId, Guid periodoId, CancellationToken ct = default)
    {
        var empleado = await _empleados.ObtenerPorIdAsync(empleadoId, ct);
        if (empleado is null || empleado.EmpresaId != empresaId)
        {
            throw new ReglaNegocioException("El empleado no existe en esta empresa.");
        }

        var periodo = await _periodos.ObtenerPorIdAsync(periodoId, ct);
        if (periodo is null || periodo.EmpresaId != empresaId)
        {
            throw new ReglaNegocioException("El periodo no existe en esta empresa.");
        }

        var entidad = new Nomina(empresaId, empleadoId, periodoId);
        await _nominas.AgregarAsync(entidad, ct);
        await _uow.GuardarCambiosAsync(ct);

        return await MapearAsync(entidad, ct);
    }

    public async Task<NominaDto> AgregarDetalleAsync(
        Guid nominaId,
        Guid conceptoId,
        decimal valorMonto,
        string moneda,
        decimal? cantidad = null,
        string? descripcion = null,
        CancellationToken ct = default)
    {
        var nomina = await _nominas.ObtenerConDetallesAsync(nominaId, ct);
        if (nomina is null)
        {
            throw new ReglaNegocioException("La nómina no existe.");
        }

        var concepto = await _conceptos.ObtenerPorIdAsync(conceptoId, ct);
        if (concepto is null || !concepto.Activo)
        {
            throw new ReglaNegocioException("El concepto no existe o está inactivo.");
        }

        var dto = new DetalleNominaDto
        {
            ConceptoNominaId = conceptoId,
            ValorMonto = valorMonto,
            ValorMoneda = moneda,
            Cantidad = cantidad,
            Descripcion = descripcion,
            Orden = concepto.Orden,
        };

        var validacion = await _detalleValidator.ValidateAsync(dto, ct);
        if (!validacion.IsValid)
        {
            throw new ValidationException($"Detalle inválido: {string.Join("; ", validacion.Errors.Select(e => e.ErrorMessage))}");
        }

        var detalle = new DetalleNomina(
            nomina.Id,
            conceptoId,
            new Dinero(valorMonto, moneda),
            concepto.Orden,
            cantidad,
            descripcion);

        nomina.AgregarDetalle(detalle);
        _nominas.Actualizar(nomina);
        await _uow.GuardarCambiosAsync(ct);

        var recargada = await _nominas.ObtenerConDetallesAsync(nomina.Id, ct);
        return await MapearAsync(recargada!, ct);
    }

    public async Task<NominaDto> CalcularAsync(
        Guid nominaId,
        decimal? baseMonto = null,
        string moneda = "COP",
        CancellationToken ct = default)
    {
        var nomina = await _nominas.ObtenerConDetallesAsync(nominaId, ct);
        if (nomina is null)
        {
            throw new ReglaNegocioException("La nómina no existe.");
        }

        if (nomina.Detalles.Count == 0)
        {
            throw new ReglaNegocioException("La nómina no tiene detalles para calcular.");
        }

        var conceptos = await _conceptos.ListarActivosAsync(nomina.EmpresaId, ct);
        var porId = conceptos.ToDictionary(c => c.Id);

        var items = new List<ItemCalculoNomina>();
        foreach (var detalle in nomina.Detalles.OrderBy(d => d.Orden))
        {
            if (!porId.TryGetValue(detalle.ConceptoNominaId, out var concepto))
            {
                throw new ReglaNegocioException("Un concepto del detalle ya no existe o está inactivo.");
            }

            items.Add(new ItemCalculoNomina(concepto, detalle.Valor));
        }

        Dinero? @base = baseMonto.HasValue ? new Dinero(baseMonto.Value, moneda) : null;
        var resultado = _motor.Calcular(moneda, items, @base);

        nomina.AplicarTotales(resultado.SubtotalDevengos, resultado.SubtotalDeducciones);
        nomina.MarcarCalculada();
        _nominas.Actualizar(nomina);
        await _uow.GuardarCambiosAsync(ct);

        var recargada = await _nominas.ObtenerConDetallesAsync(nomina.Id, ct);
        return await MapearAsync(recargada!, ct);
    }

    public async Task AprobarAsync(Guid nominaId, CancellationToken ct = default)
    {
        var nomina = await _nominas.ObtenerPorIdAsync(nominaId, ct);
        if (nomina is null)
        {
            throw new ReglaNegocioException("La nómina no existe.");
        }

        nomina.Aprobar();
        _nominas.Actualizar(nomina);
        await _uow.GuardarCambiosAsync(ct);
    }

    public async Task MarcarPagadaAsync(Guid nominaId, CancellationToken ct = default)
    {
        var nomina = await _nominas.ObtenerPorIdAsync(nominaId, ct);
        if (nomina is null)
        {
            throw new ReglaNegocioException("La nómina no existe.");
        }

        nomina.MarcarPagada();
        _nominas.Actualizar(nomina);
        await _uow.GuardarCambiosAsync(ct);
    }

    public async Task AnularAsync(Guid nominaId, string motivo, CancellationToken ct = default)
    {
        var nomina = await _nominas.ObtenerPorIdAsync(nominaId, ct);
        if (nomina is null)
        {
            throw new ReglaNegocioException("La nómina no existe.");
        }

        nomina.Anular(motivo);
        _nominas.Actualizar(nomina);
        await _uow.GuardarCambiosAsync(ct);
    }

    private async Task<NominaDto> MapearAsync(Nomina n, CancellationToken ct)
    {
        var empleado = await _empleados.ObtenerPorIdAsync(n.EmpleadoId, ct);
        var periodo = await _periodos.ObtenerPorIdAsync(n.PeriodoNominaId, ct);
        var conceptos = await _conceptos.ListarActivosAsync(n.EmpresaId, ct);
        var porId = conceptos.ToDictionary(c => c.Id);

        var dto = new NominaDto
        {
            Id = n.Id,
            EmpresaId = n.EmpresaId,
            EmpleadoId = n.EmpleadoId,
            EmpleadoNombre = empleado is null ? string.Empty : $"{empleado.Nombres} {empleado.Apellidos}",
            PeriodoNominaId = n.PeriodoNominaId,
            PeriodoNombre = periodo?.Nombre ?? string.Empty,
            Estado = n.Estado,
            Observaciones = n.Observaciones,
            SubtotalDevengos = n.SubtotalDevengos.Monto,
            SubtotalDeducciones = n.SubtotalDeducciones.Monto,
            TotalNeto = n.TotalNeto.Monto,
            Moneda = n.TotalNeto.Moneda,
        };

        foreach (var d in n.Detalles.OrderBy(x => x.Orden))
        {
            porId.TryGetValue(d.ConceptoNominaId, out var concepto);
            dto.Detalles.Add(new DetalleNominaDto
            {
                Id = d.Id,
                ConceptoNominaId = d.ConceptoNominaId,
                ConceptoNombre = concepto?.Nombre ?? d.ConceptoNominaId.ToString(),
                Tipo = concepto?.Tipo ?? Domain.Enums.TipoConcepto.Devengo,
                ValorMonto = d.Valor.Monto,
                ValorMoneda = d.Valor.Moneda,
                Cantidad = d.Cantidad,
                Descripcion = d.Descripcion,
                Orden = d.Orden,
            });
        }

        return dto;
    }
}
