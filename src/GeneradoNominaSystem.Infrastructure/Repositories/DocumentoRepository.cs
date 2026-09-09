using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneradoNominaSystem.Infrastructure.Repositories;

public sealed class DocumentoRepository : Repository<Documento>, IDocumentoRepository
{
    public DocumentoRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Documento>> ListarPorReferenciaAsync(Guid referenciaId, CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().Where(d => d.ReferenciaId == referenciaId).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Documento>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().Where(d => d.EmpresaId == empresaId).ToListAsync(ct);
    }
}
