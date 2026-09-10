using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneradoNominaSystem.Infrastructure.Repositories;

public sealed class PlantillaNominaRepository : Repository<PlantillaNomina>, IPlantillaNominaRepository
{
    public PlantillaNominaRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<PlantillaNomina>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().Where(p => p.EmpresaId == empresaId).ToListAsync(ct);
    }

    public async Task<PlantillaNomina?> ObtenerConConceptosAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(p => p.Conceptos)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task AgregarConceptoAsync(PlantillaConcepto concepto, CancellationToken ct = default)
    {
        await Context.Set<PlantillaConcepto>().AddAsync(concepto, ct);
    }
}
