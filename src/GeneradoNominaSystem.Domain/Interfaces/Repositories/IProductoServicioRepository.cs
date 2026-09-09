using GeneradoNominaSystem.Domain.Entities;

namespace GeneradoNominaSystem.Domain.Interfaces.Repositories;

public interface IProductoServicioRepository : IRepository<ProductoServicio>
{
    Task<IReadOnlyList<ProductoServicio>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);
}
