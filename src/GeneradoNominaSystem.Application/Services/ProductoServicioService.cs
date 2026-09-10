using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Application.Services;

public sealed class ProductoServicioService : IProductoServicioService
{
    private readonly IProductoServicioRepository _productos;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<ProductoServicioDto> _validator;

    public ProductoServicioService(
        IProductoServicioRepository productos,
        IUnitOfWork uow,
        IValidator<ProductoServicioDto> validator)
    {
        _productos = productos;
        _uow = uow;
        _validator = validator;
    }

    public async Task<IReadOnlyList<ProductoServicioDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        var entidades = await _productos.ListarPorEmpresaAsync(empresaId, ct);
        return entidades.Select(Mapear).ToList();
    }

    public async Task<ProductoServicioDto> CrearAsync(ProductoServicioDto dto, CancellationToken ct = default)
    {
        var resultado = await _validator.ValidateAsync(dto, ct);
        if (!resultado.IsValid)
        {
            throw new ValidationException($"Producto/servicio inválido: {string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage))}");
        }

        var entidad = new ProductoServicio(
            dto.EmpresaId,
            dto.Nombre,
            new Dinero(dto.PrecioUnitarioMonto, dto.PrecioUnitarioMoneda),
            string.IsNullOrWhiteSpace(dto.Unidad) ? "Unidad" : dto.Unidad,
            dto.Descripcion);

        await _productos.AgregarAsync(entidad, ct);
        await _uow.GuardarCambiosAsync(ct);

        return Mapear(entidad);
    }

    public async Task DesactivarAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _productos.ObtenerPorIdAsync(id, ct);
        if (entidad is null)
        {
            throw new ReglaNegocioException("El producto/servicio no existe.");
        }

        entidad.Desactivar();
        _productos.Actualizar(entidad);
        await _uow.GuardarCambiosAsync(ct);
    }

    private static ProductoServicioDto Mapear(ProductoServicio p)
    {
        return new ProductoServicioDto
        {
            Id = p.Id,
            EmpresaId = p.EmpresaId,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Unidad = p.Unidad,
            PrecioUnitarioMonto = p.PrecioUnitario.Monto,
            PrecioUnitarioMoneda = p.PrecioUnitario.Moneda,
            Activo = p.Activo,
        };
    }
}
