using GeneradoNominaSystem.Domain.Entities;

namespace GeneradoNominaSystem.Domain.Interfaces.Repositories;

public interface IPlantillaNominaRepository : IRepository<PlantillaNomina>
{
    Task<IReadOnlyList<PlantillaNomina>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);

    Task<PlantillaNomina?> ObtenerConConceptosAsync(Guid id, CancellationToken ct = default);

    Task AgregarConceptoAsync(PlantillaConcepto concepto, CancellationToken ct = default);
}
