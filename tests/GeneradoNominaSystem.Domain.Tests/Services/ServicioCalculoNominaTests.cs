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

    private static ConceptoNomina CrearConceptoPorcentaje(string nombre, TipoConcepto tipo, int orden, decimal porcentaje)
    {
        return new ConceptoNomina(nombre, tipo, orden, esPorcentaje: true, porcentajeBase: porcentaje);
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

    [Fact]
    public void Calcular_SaludYPensionPorcentaje_DeberiaCalcularSobreBase()
    {
        var @base = new Dinero(2000000m, "COP");
        var items = new List<ItemCalculoNomina>
        {
            new(CrearConcepto("Salario", TipoConcepto.Devengo, 1), new Dinero(2000000m, "COP")),
            new(CrearConceptoPorcentaje("Salud", TipoConcepto.Deduccion, 2, 4m), Dinero.Cero()),
            new(CrearConceptoPorcentaje("Pensión", TipoConcepto.Deduccion, 3, 4m), Dinero.Cero()),
        };

        var resultado = _sut.Calcular("COP", items, @base);

        resultado.SubtotalDevengos.Monto.Should().Be(2000000m);
        resultado.SubtotalDeducciones.Monto.Should().Be(160000m);
        resultado.TotalNeto.Monto.Should().Be(1840000m);
    }

    [Fact]
    public void Calcular_PorcentajeSinBase_DeberiaLanzarReglaNegocio()
    {
        var items = new List<ItemCalculoNomina>
        {
            new(CrearConceptoPorcentaje("Salud", TipoConcepto.Deduccion, 1, 4m), Dinero.Cero()),
        };

        var accion = () => _sut.Calcular("COP", items);

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void Calcular_BaseCeroConPorcentaje_DeberiaRetornarCero()
    {
        var items = new List<ItemCalculoNomina>
        {
            new(CrearConceptoPorcentaje("Salud", TipoConcepto.Deduccion, 1, 4m), Dinero.Cero()),
        };

        var resultado = _sut.Calcular("COP", items, Dinero.Cero());

        resultado.SubtotalDeducciones.Monto.Should().Be(0m);
        resultado.TotalNeto.Monto.Should().Be(0m);
    }

    [Fact]
    public void Calcular_ValorNegativo_DeberiaLanzarReglaNegocio()
    {
        var items = new List<ItemCalculoNomina>
        {
            new(CrearConcepto("Salario", TipoConcepto.Devengo, 1), new Dinero(-100m, "COP")),
        };

        var accion = () => _sut.Calcular("COP", items);

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void Calcular_Redondeo_DeberiaUsarDosDecimales()
    {
        var @base = new Dinero(1000000m, "COP");
        var items = new List<ItemCalculoNomina>
        {
            new(CrearConceptoPorcentaje("Aporte", TipoConcepto.Deduccion, 1, 3.333m), Dinero.Cero()),
        };

        var resultado = _sut.Calcular("COP", items, @base);

        resultado.SubtotalDeducciones.Monto.Should().Be(33330m);
    }

    [Fact]
    public void Calcular_ConceptoInactivo_DeberiaLanzarReglaNegocio()
    {
        var concepto = CrearConcepto("Bono viejo", TipoConcepto.Devengo, 1);
        concepto.Desactivar();
        var items = new List<ItemCalculoNomina>
        {
            new(concepto, new Dinero(100m, "COP")),
        };

        var accion = () => _sut.Calcular("COP", items);

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void Calcular_CincuentaVeces_DeberiaSerDeterminista()
    {
        var @base = new Dinero(2500000m, "COP");
        var items = new List<ItemCalculoNomina>
        {
            new(CrearConcepto("Salario", TipoConcepto.Devengo, 1), new Dinero(2500000m, "COP")),
            new(CrearConcepto("Horas extra", TipoConcepto.Devengo, 2), new Dinero(180000m, "COP")),
            new(CrearConceptoPorcentaje("Salud", TipoConcepto.Deduccion, 3, 4m), Dinero.Cero()),
            new(CrearConceptoPorcentaje("Pensión", TipoConcepto.Deduccion, 4, 4m), Dinero.Cero()),
        };

        var esperado = _sut.Calcular("COP", items, @base);
        for (var i = 0; i < 50; i++)
        {
            _sut.Calcular("COP", items, @base).Should().Be(esperado);
        }
    }
}
