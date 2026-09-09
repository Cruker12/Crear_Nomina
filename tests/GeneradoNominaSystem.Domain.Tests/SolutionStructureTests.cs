using System.Reflection;
using FluentAssertions;

namespace GeneradoNominaSystem.Domain.Tests;

public class SolutionStructureTests
{
    [Fact]
    public void DomainLayer_ShouldBeReferenceable()
    {
        var assembly = Assembly.Load("GeneradoNominaSystem.Domain");

        assembly.Should().NotBeNull();
    }
}
