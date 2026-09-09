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

        return services;
    }
}
