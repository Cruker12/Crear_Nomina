using FluentAssertions;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Tests.ValueObjects;

public class PeriodoFechaTests
{
    [Fact]
    public void Constructor_RangoValido_DeberiaCrearPeriodo()
    {
        var periodo = new PeriodoFecha(
            new DateTime(2026, 3, 1),
            new DateTime(2026, 3, 31));

        periodo.DuracionDias.Should().Be(31);
    }

    [Fact]
    public void Constructor_FinAnteriorAInicio_DeberiaLanzarReglaNegocio()
    {
        var accion = () => new PeriodoFecha(
            new DateTime(2026, 3, 31),
            new DateTime(2026, 3, 1));

        accion.Should().Throw<ReglaNegocioException>();
    }
}
