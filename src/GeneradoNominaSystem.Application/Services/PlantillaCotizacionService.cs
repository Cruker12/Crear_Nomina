using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;

namespace GeneradoNominaSystem.Application.Services;

public sealed class PlantillaCotizacionService : IPlantillaCotizacionService
{
    private readonly IPlantillaCotizacionRepository _plantillas;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<PlantillaCotizacionDto> _validator;

    public PlantillaCotizacionService(
        IPlantillaCotizacionRepository plantillas,
        IUnitOfWork uow,
        IValidator<PlantillaCotizacionDto> validator)
    {
        _plantillas = plantillas;
        _uow = uow;
        _validator = validator;
    }

    public async Task<IReadOnlyList<PlantillaCotizacionDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        var entidades = await _plantillas.ListarPorEmpresaAsync(empresaId, ct);
        return entidades.Select(Mapear).ToList();
    }

    public async Task<PlantillaCotizacionDto> CrearAsync(PlantillaCotizacionDto dto, CancellationToken ct = default)
    {
        var resultado = await _validator.ValidateAsync(dto, ct);
        if (!resultado.IsValid)
        {
            throw new ValidationException($"Plantilla inválida: {string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage))}");
        }

        var entidad = new PlantillaCotizacion(dto.EmpresaId, dto.Nombre);
        await _plantillas.AgregarAsync(entidad, ct);
        await _uow.GuardarCambiosAsync(ct);

        return Mapear(entidad);
    }

    public async Task DesactivarAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _plantillas.ObtenerPorIdAsync(id, ct);
        if (entidad is null)
        {
            throw new ReglaNegocioException("La plantilla no existe.");
        }

        entidad.Desactivar();
        _plantillas.Actualizar(entidad);
        await _uow.GuardarCambiosAsync(ct);
    }

    private static PlantillaCotizacionDto Mapear(PlantillaCotizacion p)
    {
        return new PlantillaCotizacionDto
        {
            Id = p.Id,
            EmpresaId = p.EmpresaId,
            Nombre = p.Nombre,
            Activo = p.Activo,
        };
    }
}
