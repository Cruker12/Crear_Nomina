namespace GeneradoNominaSystem.Domain.Interfaces.Services;

public interface IAppPaths
{
    string BaseDirectory { get; }

    string DatabasePath { get; }

    string LogsPath { get; }
}
