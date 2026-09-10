using FluentValidation;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Application.Services;

public sealed class PlantillaNominaService : IPlantillaNominaService
{
    private readonly IPlantillaNominaRepository _plantillas;
    private readonly IConceptoNominaRepository _conceptos;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<PlantillaNominaDto> _validator;

    public PlantillaNominaService(
        IPlantillaNominaRepository plantillas,
        IConceptoNominaRepository conceptos,
        IUnitOfWork uow,
        IValidator<PlantillaNominaDto> validator)
    {
        _plantillas = plantillas;
        _conceptos = conceptos;
        _uow = uow;
        _validator = validator;
    }

    public async Task<IReadOnlyList<PlantillaNominaDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        var entidades = await _plantillas.ListarPorEmpresaAsync(empresaId, ct);
        var lista = new List<PlantillaNominaDto>();
        foreach (var p in entidades)
        {
            var completa = await _plantillas.ObtenerConConceptosAsync(p.Id, ct);
            if (completa is not null)
            {
                lista.Add(await MapearAsync(completa, ct));
            }
        }

        return lista;
    }

    public async Task<PlantillaNominaDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var entidad = await _plantillas.ObtenerConConceptosAsync(id, ct);
        return entidad is null ? null : await MapearAsync(entidad, ct);
    }

    public async Task<PlantillaNominaDto> CrearAsync(PlantillaNominaDto dto, CancellationToken ct = default)
    {
        var resultado = await _validator.ValidateAsync(dto, ct);
        if (!resultado.IsValid)
        {
            throw new ValidationException($"Plantilla inválida: {string.Join("; ", resultado.Errors.Select(e => e.ErrorMessage))}");
        }

        var entidad = new PlantillaNomina(dto.EmpresaId, dto.Nombre, null, dto.Descripcion);
        await _plantillas.AgregarAsync(entidad, ct);
        await _uow.GuardarCambiosAsync(ct);

        return await MapearAsync(entidad, ct);
    }

    public async Task<PlantillaNominaDto> AgregarConceptoAsync(
        Guid plantillaId,
        Guid conceptoId,
        int orden,
        bool obligatorio = false,
        decimal? valorPorDefectoMonto = null,
        string valorPorDefectoMoneda = "COP",
        CancellationToken ct = default)
    {
        var plantilla = await _plantillas.ObtenerConConceptosAsync(plantillaId, ct);
        if (plantilla is null)
        {
            throw new ReglaNegocioException("La plantilla no existe.");
        }

        var concepto = await _conceptos.ObtenerPorIdAsync(conceptoId, ct);
        if (concepto is null || !concepto.Activo)
        {
            throw new ReglaNegocioException("El concepto no existe o está inactivo.");
        }

        if (orden < 0)
        {
            throw new ReglaNegocioException("El orden no puede ser negativo.");
        }

        Dinero? valorPorDefecto = null;
        if (valorPorDefectoMonto.HasValue)
        {
            if (valorPorDefectoMonto.Value < 0m)
            {
                throw new ReglaNegocioException("El valor por defecto no puede ser negativo.");
            }

            valorPorDefecto = new Dinero(valorPorDefectoMonto.Value, valorPorDefectoMoneda);
        }

        var nuevo = new PlantillaConcepto(plantilla.Id, conceptoId, orden, obligatorio, valorPorDefecto);
        plantilla.AgregarConcepto(nuevo);
        await _plantillas.AgregarConceptoAsync(nuevo, ct);
        await _uow.GuardarCambiosAsync(ct);

        var recargada = await _plantillas.ObtenerConConceptosAsync(plantilla.Id, ct);
        return await MapearAsync(recargada!, ct);
    }

    public async Task QuitarConceptoAsync(Guid plantillaId, Guid conceptoId, CancellationToken ct = default)
    {
        var plantilla = await _plantillas.ObtenerConConceptosAsync(plantillaId, ct);
        if (plantilla is null)
        {
            throw new ReglaNegocioException("La plantilla no existe.");
        }

        plantilla.RemoverConcepto(conceptoId);
        // Sin Actualizar(): la plantilla está tracked; EF detecta el hijo eliminado.
        await _uow.GuardarCambiosAsync(ct);
    }

    public async Task DesactivarAsync(Guid id, CancellationToken ct = default)
    {
        var plantilla = await _plantillas.ObtenerPorIdAsync(id, ct);
        if (plantilla is null)
        {
            throw new ReglaNegocioException("La plantilla no existe.");
        }

        plantilla.Desactivar();
        _plantillas.Actualizar(plantilla);
        await _uow.GuardarCambiosAsync(ct);
    }

    private async Task<PlantillaNominaDto> MapearAsync(PlantillaNomina p, CancellationToken ct)
    {
        var dto = new PlantillaNominaDto
        {
            Id = p.Id,
            EmpresaId = p.EmpresaId,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Activo = p.Activo,
        };

        foreach (var pc in p.Conceptos.OrderBy(c => c.Orden))
        {
            var concepto = await _conceptos.ObtenerPorIdAsync(pc.ConceptoNominaId, ct);
            dto.Conceptos.Add(new PlantillaConceptoDto
            {
                ConceptoNominaId = pc.ConceptoNominaId,
                ConceptoNombre = concepto?.Nombre ?? pc.ConceptoNominaId.ToString(),
                Orden = pc.Orden,
                Obligatorio = pc.Obligatorio,
                ValorPorDefectoMonto = pc.ValorPorDefecto?.Monto,
                ValorPorDefectoMoneda = pc.ValorPorDefecto?.Moneda ?? "COP",
            });
        }

        return dto;
    }
}
