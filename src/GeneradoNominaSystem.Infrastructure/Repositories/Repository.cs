using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneradoNominaSystem.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : EntityBase
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default)
    {
        return await DbSet.FindAsync(new object[] { id }, ct);
    }

    public virtual async Task<IReadOnlyList<T>> ListarAsync(CancellationToken ct = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(ct);
    }

    public virtual async Task AgregarAsync(T entidad, CancellationToken ct = default)
    {
        await DbSet.AddAsync(entidad, ct);
    }

    public virtual void Actualizar(T entidad)
    {
        DbSet.Update(entidad);
    }

    public virtual void Eliminar(T entidad)
    {
        DbSet.Remove(entidad);
    }
}
