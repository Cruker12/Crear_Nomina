using FluentAssertions;
using GeneradoNominaSystem.Application;
using GeneradoNominaSystem.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GeneradoNominaSystem.IntegrationTests;

public class CompositionRootTests
{
    [Fact]
    public void CompositionRoot_DeberiaResolverCapasConConfiguracion()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["App:Name"] = "Generado Nomina System",
                ["App:Version"] = "0.2.0",
                ["Company:DefaultCurrency"] = "COP",
                ["Logging:Path"] = Path.Combine(Path.GetTempPath(), $"gns-comp-{Guid.NewGuid():N}.txt"),
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddApplication();
        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider(validateScopes: true);

        var logger = provider.GetRequiredService<Serilog.ILogger>();
        logger.Should().NotBeNull();
        configuration["Company:DefaultCurrency"].Should().Be("COP");

        (logger as IDisposable)?.Dispose();
    }
}
