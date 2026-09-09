using FluentAssertions;
using GeneradoNominaSystem.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GeneradoNominaSystem.Infrastructure.Tests;

public class LoggingConfigurationTests
{
    private static IConfiguration CrearConfiguracion(string rutaLog)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:Path"] = rutaLog,
                ["App:Name"] = "Generado Nomina System",
            })
            .Build();
    }

    [Fact]
    public void CrearLogger_DeberiaEscribirArchivoDeLog()
    {
        var nombreBase = $"gns-test-{Guid.NewGuid():N}";
        var rutaLog = Path.Combine(Path.GetTempPath(), $"{nombreBase}.txt");
        var configuration = CrearConfiguracion(rutaLog);

        var logger = ConfiguracionSerilog.CrearLogger(configuration);
        try
        {
            logger.Information("Mensaje de prueba Fase 2");
        }
        finally
        {
            (logger as IDisposable)?.Dispose();
        }

        var archivos = Directory.GetFiles(Path.GetTempPath(), $"{nombreBase}*.txt");
        archivos.Should().NotBeEmpty();

        var contenido = File.ReadAllText(archivos[0]);
        contenido.Should().Contain("Mensaje de prueba Fase 2");

        foreach (var archivo in archivos)
        {
            File.Delete(archivo);
        }
    }

    [Fact]
    public void AddInfrastructure_DeberiaRegistrarLoggerYConfiguracion()
    {
        var rutaLog = Path.Combine(Path.GetTempPath(), $"gns-di-{Guid.NewGuid():N}.txt");
        var configuration = CrearConfiguracion(rutaLog);
        var services = new ServiceCollection();

        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider(validateScopes: true);

        var logger = provider.GetRequiredService<Serilog.ILogger>();
        var configResuelta = provider.GetRequiredService<IConfiguration>();

        logger.Should().NotBeNull();
        configResuelta.Should().BeSameAs(configuration);
        configResuelta["App:Name"].Should().Be("Generado Nomina System");

        (provider as IDisposable)?.Dispose();
        (logger as IDisposable)?.Dispose();
    }
}
