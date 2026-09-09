using GeneradoNominaSystem.Domain.Entities;

namespace GeneradoNominaSystem.Domain.Interfaces.Repositories;

public interface IPlantillaCotizacionRepository : IRepository<PlantillaCotizacion>
{
    Task<IReadOnlyList<PlantillaCotizacion>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);
}
