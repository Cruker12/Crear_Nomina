using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Interfaces;

public interface IPlantillaCotizacionService
{
    Task<IReadOnlyList<PlantillaCotizacionDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);

    Task<PlantillaCotizacionDto> CrearAsync(PlantillaCotizacionDto dto, CancellationToken ct = default);

    Task DesactivarAsync(Guid id, CancellationToken ct = default);
}
