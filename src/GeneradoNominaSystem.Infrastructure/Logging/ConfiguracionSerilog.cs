using Microsoft.Extensions.Configuration;
using Serilog;

namespace GeneradoNominaSystem.Infrastructure.Logging;

public static class ConfiguracionSerilog
{
    public static Serilog.ILogger CrearLogger(IConfiguration configuration)
    {
        var rutaLog = configuration["Logging:Path"];
        if (string.IsNullOrWhiteSpace(rutaLog))
        {
            rutaLog = "logs/log-.txt";
        }

        return CrearLoggerEn(rutaLog);
    }

    public static Serilog.ILogger CrearLoggerEn(string rutaLog)
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .WriteTo.File(
                rutaLog,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        return logger;
    }
}
