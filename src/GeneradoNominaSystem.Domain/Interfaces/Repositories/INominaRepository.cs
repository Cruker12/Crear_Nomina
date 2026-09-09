using GeneradoNominaSystem.Domain.Entities;

namespace GeneradoNominaSystem.Domain.Interfaces.Repositories;

public interface INominaRepository : IRepository<Nomina>
{
    Task<IReadOnlyList<Nomina>> ListarPorPeriodoAsync(Guid periodoId, CancellationToken ct = default);

    Task<IReadOnlyList<Nomina>> ListarPorEmpleadoAsync(Guid empleadoId, CancellationToken ct = default);
}
