using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace GeneradoNominaSystem.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_DeberiaConstruirProveedorSinErrores()
    {
        var services = new ServiceCollection();

        services.AddApplication();
        using var provider = services.BuildServiceProvider(validateScopes: true);

        provider.Should().NotBeNull();
    }
}
