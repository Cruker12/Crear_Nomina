using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Interfaces;

public interface IPlantillaNominaService
{
    Task<IReadOnlyList<PlantillaNominaDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);

    Task<PlantillaNominaDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<PlantillaNominaDto> CrearAsync(PlantillaNominaDto dto, CancellationToken ct = default);

    Task<PlantillaNominaDto> AgregarConceptoAsync(
        Guid plantillaId,
        Guid conceptoId,
        int orden,
        bool obligatorio = false,
        decimal? valorPorDefectoMonto = null,
        string valorPorDefectoMoneda = "COP",
        CancellationToken ct = default);

    Task QuitarConceptoAsync(Guid plantillaId, Guid conceptoId, CancellationToken ct = default);

    Task DesactivarAsync(Guid id, CancellationToken ct = default);

    Task ActivarAsync(Guid id, CancellationToken ct = default);

    Task EliminarAsync(Guid id, CancellationToken ct = default);
}
