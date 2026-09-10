using FluentValidation;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Application.Services;
using GeneradoNominaSystem.Domain.Interfaces.Services;
using GeneradoNominaSystem.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GeneradoNominaSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddSingleton<IServicioNumeracion, ServicioNumeracion>();
        services.AddScoped<IEmpresaService, EmpresaService>();
        services.AddScoped<IEmpleadoService, EmpleadoService>();
        services.AddScoped<IConceptoNominaService, ConceptoNominaService>();
        services.AddScoped<IPeriodoNominaService, PeriodoNominaService>();
        services.AddScoped<IPlantillaNominaService, PlantillaNominaService>();
        services.AddScoped<IProductoServicioService, ProductoServicioService>();
        services.AddScoped<IPlantillaCotizacionService, PlantillaCotizacionService>();
        services.AddScoped<IHistorialService, HistorialService>();
        services.AddScoped<ICotizacionService, CotizacionService>();
        services.AddScoped<IDocumentoService, DocumentoService>();
        services.AddScoped<INominaService, NominaService>();

        return services;
    }
}
