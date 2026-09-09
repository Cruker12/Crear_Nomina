using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneradoNominaSystem.Infrastructure.Repositories;

public sealed class EmpresaRepository : Repository<Empresa>, IEmpresaRepository
{
    public EmpresaRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<Empresa?> ObtenerPorNitAsync(string nit, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(e => e.Nit == nit, ct);
    }
}
