using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Infrastructure.Data;

namespace GeneradoNominaSystem.Infrastructure.Data;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public EfUnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> GuardarCambiosAsync(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }
}
