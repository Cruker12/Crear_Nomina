using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Application.Services;

public sealed class ConceptoNominaService : IConceptoNominaService
{
    private readonly IConceptoNominaRepository _conceptos;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<ConceptoNominaDto> _validator;

    public ConceptoNominaService(
        IConceptoNominaRepository conceptos,
        IUnitOfWork uow,
        IValidator<ConceptoNominaDto> validator)
    {
        _conceptos = conceptos;
        _uow = uow;
        _validator = validator;
    }

    public async Task<IReadOnlyList<ConceptoNominaDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        var entidades = await _conceptos.ListarPorEmpresaAsync(empresaId, ct);
        return entidades.Select(Mapear).ToList();
    }

    public async Task<ConceptoNominaDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _conceptos.ObtenerPorIdAsync(id, ct);
        return entidad is null ? null : Mapear(entidad);
    }

    public async Task<ConceptoNominaDto> CrearAsync(ConceptoNominaDto dto, CancellationToken ct = default)
    {
        var resultado = await _validator.ValidateAsync(dto, ct);
        if (!resultado.IsValid)
        {
            throw new ValidationException($"Concepto inválido: {string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage))}");
        }

        Dinero? valorFijo = null;
        if (!dto.EsPorcentaje && dto.ValorFijoMonto.HasValue)
        {
            valorFijo = new Dinero(dto.ValorFijoMonto.Value, dto.ValorFijoMoneda);
        }

        var entidad = new ConceptoNomina(
            dto.Nombre,
            dto.Tipo,
            dto.Orden,
            dto.EmpresaId,
            dto.Subtipo,
            dto.EsPorcentaje,
            dto.PorcentajeBase,
            valorFijo);

        await _conceptos.AgregarAsync(entidad, ct);
        await _uow.GuardarCambiosAsync(ct);

        return Mapear(entidad);
    }

    public async Task DesactivarAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _conceptos.ObtenerPorIdAsync(id, ct);
        if (entidad is null)
        {
            throw new ReglaNegocioException("El concepto no existe.");
        }

        entidad.Desactivar();
        _conceptos.Actualizar(entidad);
        await _uow.GuardarCambiosAsync(ct);
    }

    public async Task ActivarAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _conceptos.ObtenerPorIdAsync(id, ct);
        if (entidad is null)
        {
            throw new ReglaNegocioException("El concepto no existe.");
        }

        entidad.Activar();
        _conceptos.Actualizar(entidad);
        await _uow.GuardarCambiosAsync(ct);
    }

    private static ConceptoNominaDto Mapear(ConceptoNomina c)
    {
        return new ConceptoNominaDto
        {
            Id = c.Id,
            EmpresaId = c.EmpresaId,
            Nombre = c.Nombre,
            Tipo = c.Tipo,
            Subtipo = c.Subtipo,
            EsPorcentaje = c.EsPorcentaje,
            PorcentajeBase = c.PorcentajeBase,
            ValorFijoMonto = c.ValorFijo?.Monto,
            ValorFijoMoneda = c.ValorFijo?.Moneda ?? "COP",
            Orden = c.Orden,
            Activo = c.Activo,
        };
    }
}
