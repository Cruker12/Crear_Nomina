using System.Reflection;
using FluentAssertions;

namespace GeneradoNominaSystem.Application.Tests;

public class SolutionStructureTests
{
    [Fact]
    public void ApplicationLayer_ShouldReferenceDomain()
    {
        var appAssembly = Assembly.Load("GeneradoNominaSystem.Application");
        var domainAssembly = Assembly.Load("GeneradoNominaSystem.Domain");

        appAssembly.Should().NotBeNull();
        domainAssembly.Should().NotBeNull();
    }
}
