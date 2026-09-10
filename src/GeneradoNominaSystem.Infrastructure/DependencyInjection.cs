using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.Interfaces.Services;
using GeneradoNominaSystem.Infrastructure.Data;
using GeneradoNominaSystem.Infrastructure.Exportadores;
using GeneradoNominaSystem.Infrastructure.Logging;
using GeneradoNominaSystem.Infrastructure.Repositories;
using GeneradoNominaSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GeneradoNominaSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rutas = new AppPaths(configuration);
        services.AddSingleton<IAppPaths>(rutas);
        services.AddSingleton(configuration);

        var logger = ConfiguracionSerilog.CrearLoggerEn(rutas.LogsPath);
        services.AddSingleton(logger);

        var directorio = Path.GetDirectoryName(rutas.DatabasePath);
        if (!string.IsNullOrWhiteSpace(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        var rutaBd = rutas.DatabasePath;
        services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={rutaBd}"));

        services.AddScoped<IEmpresaRepository, EmpresaRepository>();
        services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
        services.AddScoped<INominaRepository, NominaRepository>();
        services.AddScoped<IConceptoNominaRepository, ConceptoNominaRepository>();
        services.AddScoped<IPeriodoNominaRepository, PeriodoNominaRepository>();
        services.AddScoped<IPlantillaNominaRepository, PlantillaNominaRepository>();
        services.AddScoped<ICotizacionRepository, CotizacionRepository>();
        services.AddScoped<IProductoServicioRepository, ProductoServicioRepository>();
        services.AddScoped<IPlantillaCotizacionRepository, PlantillaCotizacionRepository>();
        services.AddScoped<IDocumentoRepository, DocumentoRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddSingleton<IExportadorDocumento, ExportadorNominaPdf>();
        services.AddSingleton<IExportadorDocumento, ExportadorNominaExcel>();

        return services;
    }
}
