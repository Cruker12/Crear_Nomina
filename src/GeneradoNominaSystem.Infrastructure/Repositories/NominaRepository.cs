using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneradoNominaSystem.Infrastructure.Repositories;

public sealed class NominaRepository : Repository<Nomina>, INominaRepository
{
    public NominaRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Nomina>> ListarPorPeriodoAsync(Guid periodoId, CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().Where(n => n.PeriodoNominaId == periodoId).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Nomina>> ListarPorEmpleadoAsync(Guid empleadoId, CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().Where(n => n.EmpleadoId == empleadoId).ToListAsync(ct);
    }
}
