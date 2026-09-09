using GeneradoNominaSystem.Domain.Entities;

namespace GeneradoNominaSystem.Domain.Interfaces.Repositories;

public interface IPeriodoNominaRepository : IRepository<PeriodoNomina>
{
    Task<IReadOnlyList<PeriodoNomina>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);
}
