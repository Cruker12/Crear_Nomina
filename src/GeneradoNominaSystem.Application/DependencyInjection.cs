using FluentValidation;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GeneradoNominaSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IEmpresaService, EmpresaService>();
        services.AddScoped<IEmpleadoService, EmpleadoService>();
        services.AddScoped<IConceptoNominaService, ConceptoNominaService>();
        services.AddScoped<IPeriodoNominaService, PeriodoNominaService>();
        services.AddScoped<IPlantillaNominaService, PlantillaNominaService>();
        services.AddScoped<INominaService, NominaService>();

        return services;
    }
}
