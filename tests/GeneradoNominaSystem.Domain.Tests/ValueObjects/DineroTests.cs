using FluentAssertions;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Tests.ValueObjects;

public class DineroTests
{
    [Fact]
    public void Constructor_ConMonedaMinuscula_DeberiaNormalizarAMayusculas()
    {
        var dinero = new Dinero(1000m, "cop");

        dinero.Moneda.Should().Be("COP");
        dinero.Monto.Should().Be(1000m);
    }

    [Fact]
    public void Sumar_MismaMoneda_DeberiaRetornarTotal()
    {
        var a = new Dinero(1000m, "COP");
        var b = new Dinero(500m, "COP");

        var resultado = a.Sumar(b);

        resultado.Monto.Should().Be(1500m);
        resultado.Moneda.Should().Be("COP");
    }

    [Fact]
    public void Sumar_MonedaDistinta_DeberiaLanzarReglaNegocio()
    {
        var a = new Dinero(1000m, "COP");
        var b = new Dinero(500m, "USD");

        var accion = () => a.Sumar(b);

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void Constructor_MonedaInvalida_DeberiaLanzarReglaNegocio()
    {
        var accion = () => new Dinero(100m, "CO");

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void Igualdad_MismoMontoYMoneda_DeberiaSerIgual()
    {
        var a = new Dinero(1000m, "COP");
        var b = new Dinero(1000m, "COP");

        a.Should().Be(b);
    }
}
