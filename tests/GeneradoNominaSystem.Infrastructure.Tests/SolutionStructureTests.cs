using System.Reflection;
using FluentAssertions;

namespace GeneradoNominaSystem.Infrastructure.Tests;

public class SolutionStructureTests
{
    [Fact]
    public void InfrastructureLayer_ShouldBeReferenceable()
    {
        var assembly = Assembly.Load("GeneradoNominaSystem.Infrastructure");

        assembly.Should().NotBeNull();
    }
}
