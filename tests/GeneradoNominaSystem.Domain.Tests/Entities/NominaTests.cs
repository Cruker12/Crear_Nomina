using FluentAssertions;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Tests.Entities;

public class NominaTests
{
    private static Nomina CrearNomina()
    {
        return new Nomina(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
    }

    [Fact]
    public void AgregarDetalle_ConceptoDuplicado_DeberiaLanzarReglaNegocio()
    {
        var nomina = CrearNomina();
        var conceptoId = Guid.NewGuid();
        nomina.AgregarDetalle(new DetalleNomina(nomina.Id, conceptoId, new Dinero(1000m, "COP"), 1));

        var accion = () => nomina.AgregarDetalle(
            new DetalleNomina(nomina.Id, conceptoId, new Dinero(500m, "COP"), 2));

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void MaquinaEstados_BorradorCalculadaAprobadaPagada_DeberiaTransicionar()
    {
        var nomina = CrearNomina();

        nomina.MarcarCalculada();
        nomina.Estado.Should().Be(EstadoNomina.Calculada);

        nomina.Aprobar();
        nomina.Estado.Should().Be(EstadoNomina.Aprobada);

        nomina.MarcarPagada();
        nomina.Estado.Should().Be(EstadoNomina.Pagada);
    }

    [Fact]
    public void Aprobar_SinCalcular_DeberiaLanzarReglaNegocio()
    {
        var nomina = CrearNomina();

        var accion = () => nomina.Aprobar();

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void Anular_SinMotivo_DeberiaLanzarReglaNegocio()
    {
        var nomina = CrearNomina();
        nomina.MarcarCalculada();

        var accion = () => nomina.Anular("  ");

        accion.Should().Throw<ReglaNegocioException>();
    }
}
