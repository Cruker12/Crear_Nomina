using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;

namespace GeneradoNominaSystem.Application.Services;

public sealed class PeriodoNominaService : IPeriodoNominaService
{
    private readonly IPeriodoNominaRepository _periodos;
    private readonly INominaRepository _nominas;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<PeriodoNominaDto> _validator;

    public PeriodoNominaService(
        IPeriodoNominaRepository periodos,
        INominaRepository nominas,
        IUnitOfWork uow,
        IValidator<PeriodoNominaDto> validator)
    {
        _periodos = periodos;
        _nominas = nominas;
        _uow = uow;
        _validator = validator;
    }

    public async Task<IReadOnlyList<PeriodoNominaDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        var entidades = await _periodos.ListarPorEmpresaAsync(empresaId, ct);
        return entidades.Select(Mapear).ToList();
    }

    public async Task<PeriodoNominaDto> CrearAsync(PeriodoNominaDto dto, CancellationToken ct = default)
    {
        var resultado = await _validator.ValidateAsync(dto, ct);
        if (!resultado.IsValid)
        {
            throw new ValidationException($"Periodo inválido: {string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage))}");
        }

        var existentes = await _periodos.ListarPorEmpresaAsync(dto.EmpresaId, ct);
        var solapa = existentes.Any(e =>
            e.Activo &&
            e.Tipo == dto.Tipo &&
            dto.FechaInicio <= e.FechaFin &&
            dto.FechaFin >= e.FechaInicio);

        if (solapa)
        {
            throw new ReglaNegocioException("Existe un periodo activo del mismo tipo que se solapa con estas fechas.");
        }

        var entidad = new PeriodoNomina(dto.EmpresaId, dto.Nombre, dto.Tipo, dto.FechaInicio, dto.FechaFin);
        await _periodos.AgregarAsync(entidad, ct);
        await _uow.GuardarCambiosAsync(ct);

        return Mapear(entidad);
    }

    public async Task DesactivarAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _periodos.ObtenerPorIdAsync(id, ct);
        if (entidad is null)
        {
            throw new ReglaNegocioException("El periodo no existe.");
        }

        entidad.Desactivar();
        _periodos.Actualizar(entidad);
        await _uow.GuardarCambiosAsync(ct);
    }

    public async Task ActivarAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _periodos.ObtenerPorIdAsync(id, ct);
        if (entidad is null)
        {
            throw new ReglaNegocioException("El periodo no existe.");
        }

        entidad.Activar();
        _periodos.Actualizar(entidad);
        await _uow.GuardarCambiosAsync(ct);
    }

    public async Task EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _periodos.ObtenerPorIdAsync(id, ct);
        if (entidad is null)
        {
            throw new ReglaNegocioException("El periodo no existe.");
        }

        var nominas = await _nominas.ListarPorPeriodoAsync(id, ct);
        if (nominas.Count > 0)
        {
            throw new ReglaNegocioException(
                "No se puede eliminar el periodo porque tiene nóminas registradas. " +
                "Desactívalo en su lugar.");
        }

        _periodos.Eliminar(entidad);
        await _uow.GuardarCambiosAsync(ct);
    }

    private static PeriodoNominaDto Mapear(PeriodoNomina p)
    {
        return new PeriodoNominaDto
        {
            Id = p.Id,
            EmpresaId = p.EmpresaId,
            Nombre = p.Nombre,
            Tipo = p.Tipo,
            FechaInicio = p.FechaInicio,
            FechaFin = p.FechaFin,
            Activo = p.Activo,
        };
    }
}
