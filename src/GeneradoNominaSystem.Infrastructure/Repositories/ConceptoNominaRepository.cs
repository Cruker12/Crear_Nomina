using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneradoNominaSystem.Infrastructure.Repositories;

public sealed class ConceptoNominaRepository : Repository<ConceptoNomina>, IConceptoNominaRepository
{
    public ConceptoNominaRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<ConceptoNomina>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking()
            .Where(c => c.EmpresaId == null || c.EmpresaId == empresaId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ConceptoNomina>> ListarActivosAsync(Guid? empresaId = null, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().Where(c => c.Activo);

        if (empresaId.HasValue)
        {
            query = query.Where(c => c.EmpresaId == null || c.EmpresaId == empresaId.Value);
        }

        return await query.ToListAsync(ct);
    }
}
