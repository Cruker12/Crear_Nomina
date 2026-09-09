using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneradoNominaSystem.Infrastructure.Repositories;

public sealed class PeriodoNominaRepository : Repository<PeriodoNomina>, IPeriodoNominaRepository
{
    public PeriodoNominaRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<PeriodoNomina>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().Where(p => p.EmpresaId == empresaId).ToListAsync(ct);
    }
}
