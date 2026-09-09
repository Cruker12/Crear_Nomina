using GeneradoNominaSystem.Domain.Entities;

namespace GeneradoNominaSystem.Domain.Interfaces.Repositories;

public interface IConceptoNominaRepository : IRepository<ConceptoNomina>
{
    Task<IReadOnlyList<ConceptoNomina>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);

    Task<IReadOnlyList<ConceptoNomina>> ListarActivosAsync(Guid? empresaId = null, CancellationToken ct = default);
}
