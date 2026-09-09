using GeneradoNominaSystem.Domain.Entities;

namespace GeneradoNominaSystem.Domain.Interfaces.Repositories;

public interface IEmpleadoRepository : IRepository<Empleado>
{
    Task<IReadOnlyList<Empleado>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);

    Task<Empleado?> ObtenerPorDocumentoAsync(Guid empresaId, string numeroDocumento, CancellationToken ct = default);
}
