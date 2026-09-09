using System.Reflection;
using FluentAssertions;

namespace GeneradoNominaSystem.IntegrationTests;

public class SolutionStructureTests
{
    [Fact]
    public void AllLayers_ShouldLoadTogether()
    {
        var domain = Assembly.Load("GeneradoNominaSystem.Domain");
        var application = Assembly.Load("GeneradoNominaSystem.Application");
        var infrastructure = Assembly.Load("GeneradoNominaSystem.Infrastructure");

        domain.Should().NotBeNull();
        application.Should().NotBeNull();
        infrastructure.Should().NotBeNull();
    }
}
