using GeneradoNominaSystem.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace GeneradoNominaSystem.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddTransient<EmpresaViewModel>();
        services.AddTransient<EmpleadoViewModel>();
        services.AddTransient<ConceptoViewModel>();
        services.AddTransient<PeriodoViewModel>();
        services.AddTransient<NominaViewModel>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}
