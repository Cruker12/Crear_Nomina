using GeneradoNominaSystem.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace GeneradoNominaSystem.Infrastructure.Services;

public sealed class AppPaths : IAppPaths
{
    public AppPaths(IConfiguration configuration)
    {
        BaseDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GeneradoNominaSystem");

        DatabasePath = Resolver(configuration["Database:Path"], "data/app.db");
        LogsPath = Resolver(configuration["Logging:Path"], "logs/log-.txt");
    }

    public string BaseDirectory { get; }

    public string DatabasePath { get; }

    public string LogsPath { get; }

    private string Resolver(string? configurado, string porDefecto)
    {
        var ruta = string.IsNullOrWhiteSpace(configurado) ? porDefecto : configurado.Trim();

        if (Path.IsPathRooted(ruta))
        {
            return ruta;
        }

        ruta = ruta.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        return Path.Combine(BaseDirectory, ruta);
    }
}
