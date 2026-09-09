using FluentAssertions;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Services;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Tests.Services;

public class ServicioCalculoNominaTests
{
    private readonly ServicioCalculoNomina _sut = new();

    private static ConceptoNomina CrearConcepto(string nombre, TipoConcepto tipo, int orden)
    {
        return new ConceptoNomina(nombre, tipo, orden, valorFijo: new Dinero(0m, "COP"));
    }

    [Fact]
    public void Calcular_SalarioMasBonificacionMenosSalud_DeberiaRetornarNeto()
    {
        var items = new List<ItemCalculoNomina>
        {
            new(CrearConcepto("Salario", TipoConcepto.Devengo, 1), new Dinero(2000000m, "COP")),
            new(CrearConcepto("Bonificación", TipoConcepto.Devengo, 2), new Dinero(300000m, "COP")),
            new(CrearConcepto("Salud", TipoConcepto.Deduccion, 3), new Dinero(92000m, "COP")),
        };

        var resultado = _sut.Calcular("COP", items);

        resultado.SubtotalDevengos.Monto.Should().Be(2300000m);
        resultado.SubtotalDeducciones.Monto.Should().Be(92000m);
        resultado.TotalNeto.Monto.Should().Be(2208000m);
    }

    [Fact]
    public void Calcular_Comision_DeberiaSumarComoDevengo()
    {
        var items = new List<ItemCalculoNomina>
        {
            new(CrearConcepto("Salario", TipoConcepto.Devengo, 1), new Dinero(1000000m, "COP")),
            new(CrearConcepto("Comisión", TipoConcepto.Comision, 2), new Dinero(250000m, "COP")),
        };

        var resultado = _sut.Calcular("COP", items);

        resultado.SubtotalDevengos.Monto.Should().Be(1250000m);
        resultado.TotalNeto.Monto.Should().Be(1250000m);
    }

    [Fact]
    public void Calcular_MonedasMezcladas_DeberiaLanzarReglaNegocio()
    {
        var items = new List<ItemCalculoNomina>
        {
            new(CrearConcepto("Salario", TipoConcepto.Devengo, 1), new Dinero(1000000m, "COP")),
            new(CrearConcepto("Bono", TipoConcepto.Devengo, 2), new Dinero(100m, "USD")),
        };

        var accion = () => _sut.Calcular("COP", items);

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void Calcular_MismoInputDosVeces_DeberiaSerDeterminista()
    {
        var items = new List<ItemCalculoNomina>
        {
            new(CrearConcepto("Salario", TipoConcepto.Devengo, 1), new Dinero(1500000m, "COP")),
            new(CrearConcepto("Pensión", TipoConcepto.Deduccion, 2), new Dinero(60000m, "COP")),
        };

        var primero = _sut.Calcular("COP", items);
        var segundo = _sut.Calcular("COP", items);

        segundo.Should().Be(primero);
    }
}
