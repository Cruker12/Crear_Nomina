using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneradoNominaSystem.Infrastructure.Repositories;

public sealed class EmpleadoRepository : Repository<Empleado>, IEmpleadoRepository
{
    public EmpleadoRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<Empleado>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().Where(e => e.EmpresaId == empresaId).ToListAsync(ct);
    }

    public async Task<Empleado?> ObtenerPorDocumentoAsync(Guid empresaId, string numeroDocumento, CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            e => e.EmpresaId == empresaId && e.NumeroDocumento == numeroDocumento, ct);
    }
}
