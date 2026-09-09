using GeneradoNominaSystem.Domain.Entities;

namespace GeneradoNominaSystem.Domain.Interfaces.Repositories;

public interface IEmpresaRepository : IRepository<Empresa>
{
    Task<Empresa?> ObtenerPorNitAsync(string nit, CancellationToken ct = default);
}
