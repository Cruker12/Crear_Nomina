using System.IO;
using System.Windows;
using GeneradoNominaSystem.Application;
using GeneradoNominaSystem.Infrastructure;
using GeneradoNominaSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GeneradoNominaSystem.Presentation;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;
    private Serilog.ILogger? _logger;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddPresentation();

        _serviceProvider = services.BuildServiceProvider();
        _logger = _serviceProvider.GetRequiredService<Serilog.ILogger>();

        try
        {
            using var scope = _serviceProvider.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
            _logger.Information("Base de datos verificada.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "No se pudo preparar la base de datos");
            MessageBox.Show(
                "No se pudo preparar la base de datos local. La aplicación se cerrará.",
                "Error crítico",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        _logger.Information("Iniciando {App} v{Version}",
            configuration["App:Name"], configuration["App:Version"]);

        DispatcherUnhandledException += (_, args) =>
        {
            _logger.Error(args.Exception, "Excepción no controlada en UI");
            MessageBox.Show(
                "Ocurrió un error inesperado. Revisa el archivo de log para más detalles.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _logger?.Information("Cerrando aplicación");
        (_logger as IDisposable)?.Dispose();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
