using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Infrastructure.Data;
using GeneradoNominaSystem.Infrastructure.Logging;
using GeneradoNominaSystem.Infrastructure.Repositories;
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
        var logger = ConfiguracionSerilog.CrearLogger(configuration);
        services.AddSingleton(logger);
        services.AddSingleton(configuration);

        var rutaBd = configuration["Database:Path"];
        if (string.IsNullOrWhiteSpace(rutaBd))
        {
            rutaBd = "data/app.db";
        }

        if (!Path.IsPathRooted(rutaBd))
        {
            rutaBd = Path.Combine(AppContext.BaseDirectory, rutaBd);
        }

        var directorio = Path.GetDirectoryName(rutaBd);
        if (!string.IsNullOrWhiteSpace(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

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

        return services;
    }
}
