using FluentAssertions;
using GeneradoNominaSystem.Domain.Interfaces.Services;
using GeneradoNominaSystem.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GeneradoNominaSystem.Infrastructure.Tests;

public class AppPathsTests
{
    private static IConfiguration CrearConfiguracion(string? baseDatos, string? logs)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Path"] = baseDatos,
                ["Logging:Path"] = logs,
            })
            .Build();
    }

    [Fact]
    public void Constructor_RutasRelativas_DeberiaResolverEnLocalAppData()
    {
        var rutas = new AppPaths(CrearConfiguracion("data/app.db", "logs/log-.txt"));

        rutas.BaseDirectory.Should().Contain("GeneradoNominaSystem");
        rutas.DatabasePath.Should().Be(Path.Combine(rutas.BaseDirectory, "data", "app.db"));
        rutas.LogsPath.Should().Be(Path.Combine(rutas.BaseDirectory, "logs", "log-.txt"));
    }

    [Fact]
    public void Constructor_RutasAbsolutas_DeberiaRespetarlas()
    {
        var db = Path.Combine(Path.GetTempPath(), "x.db");
        var log = Path.Combine(Path.GetTempPath(), "y.txt");
        var rutas = new AppPaths(CrearConfiguracion(db, log));

        rutas.DatabasePath.Should().Be(db);
        rutas.LogsPath.Should().Be(log);
    }

    [Fact]
    public void Constructor_SinConfiguracion_DeberiaUsarDefectos()
    {
        var rutas = new AppPaths(CrearConfiguracion(null, null));

        rutas.DatabasePath.Should().EndWith(Path.Combine("data", "app.db"));
        rutas.LogsPath.Should().EndWith(Path.Combine("logs", "log-.txt"));
    }

    [Fact]
    public void AddInfrastructure_DeberiaRegistrarAppPaths()
    {
        var configuration = CrearConfiguracion(
            Path.Combine(Path.GetTempPath(), $"gns-p-{Guid.NewGuid():N}.db"),
            Path.Combine(Path.GetTempPath(), $"gns-p-{Guid.NewGuid():N}.txt"));
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider(validateScopes: true);

        var rutas = provider.GetRequiredService<IAppPaths>();

        rutas.Should().NotBeNull();
        rutas.DatabasePath.Should().EndWith(".db");
    }
}
