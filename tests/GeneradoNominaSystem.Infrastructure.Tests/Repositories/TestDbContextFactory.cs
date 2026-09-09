using GeneradoNominaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GeneradoNominaSystem.Infrastructure.Tests.Repositories;

public sealed class TestDbContextFactory : IDisposable
{
    public string RutaBd { get; } = Path.Combine(Path.GetTempPath(), $"gns-test-{Guid.NewGuid():N}.db");

    public AppDbContext Crear()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={RutaBd}")
            .Options;

        var context = new AppDbContext(options);
        context.Database.Migrate();
        return context;
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(RutaBd))
            {
                File.Delete(RutaBd);
            }
        }
        catch
        {
        }
    }
}
