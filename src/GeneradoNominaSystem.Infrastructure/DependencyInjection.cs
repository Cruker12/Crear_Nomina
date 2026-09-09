using GeneradoNominaSystem.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GeneradoNominaSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var logger = ConfiguracionSerilog.CrearLogger(configuration);
        services.AddSingleton(logger);
        services.AddSingleton(configuration);

        return services;
    }
}
