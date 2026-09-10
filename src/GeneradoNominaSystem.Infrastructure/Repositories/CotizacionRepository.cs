using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneradoNominaSystem.Infrastructure.Repositories;

public sealed class CotizacionRepository : Repository<Cotizacion>, ICotizacionRepository
{
    public CotizacionRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<Cotizacion?> ObtenerPorNumeroAsync(Guid empresaId, string numeroCotizacion, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            c => c.EmpresaId == empresaId && c.NumeroCotizacion == numeroCotizacion, ct);
    }

    public async Task<IReadOnlyList<Cotizacion>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().Where(c => c.EmpresaId == empresaId).ToListAsync(ct);
    }

    public async Task<Cotizacion?> ObtenerConDetallesAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.Detalles)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task AgregarDetalleAsync(DetalleCotizacion detalle, CancellationToken ct = default)
    {
        await Context.Set<DetalleCotizacion>().AddAsync(detalle, ct);
    }
}
