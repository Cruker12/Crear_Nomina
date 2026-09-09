using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneradoNominaSystem.Infrastructure.Repositories;

public sealed class PlantillaCotizacionRepository : Repository<PlantillaCotizacion>, IPlantillaCotizacionRepository
{
    public PlantillaCotizacionRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<PlantillaCotizacion>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().Where(p => p.EmpresaId == empresaId).ToListAsync(ct);
    }
}
