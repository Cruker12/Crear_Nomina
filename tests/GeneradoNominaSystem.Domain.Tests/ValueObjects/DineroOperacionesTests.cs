using FluentAssertions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Tests.ValueObjects;

public class DineroOperacionesTests
{
    [Fact]
    public void MultiplicarPor_Factor_DeberiaRedondearADosDecimales()
    {
        var dinero = new Dinero(1000000m, "COP");

        var resultado = dinero.MultiplicarPor(0.03333m);

        resultado.Monto.Should().Be(33330m);
    }

    [Fact]
    public void Operadores_SumaYResta_DeberianFuncionar()
    {
        var a = new Dinero(1000m, "COP");
        var b = new Dinero(400m, "COP");

        (a + b).Monto.Should().Be(1400m);
        (a - b).Monto.Should().Be(600m);
        (a * 2m).Monto.Should().Be(2000m);
    }

    [Fact]
    public void Formatear_ConSimbolo_DeberiaIncluirSimboloYMoneda()
    {
        var dinero = new Dinero(1500000m, "COP");

        var texto = dinero.Formatear("$", 2);

        texto.Should().StartWith("$").And.EndWith("COP");
    }

    [Fact]
    public void Cero_DeberiaSerCero()
    {
        var cero = Dinero.Cero();

        cero.EsCero.Should().BeTrue();
        cero.EsPositivo.Should().BeFalse();
    }
}
