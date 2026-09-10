using FluentAssertions;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Tests.Entities;

public class CotizacionTests
{
    private static Cotizacion CrearCotizacion()
    {
        return new Cotizacion(
            Guid.NewGuid(),
            "Cliente Test",
            "COT-2026-0001",
            new DateTime(2026, 3, 1),
            new DateTime(2026, 3, 31));
    }

    private static DetalleCotizacion CrearDetalle(Guid cotizacionId, string descripcion, decimal cantidad, decimal precio, decimal? descuento = null)
    {
        return new DetalleCotizacion(
            cotizacionId,
            descripcion,
            cantidad,
            new Dinero(precio, "COP"),
            1,
            null,
            descuento);
    }

    [Fact]
    public void AgregarDetalle_ConDescuento_DeberiaRecalcularTotales()
    {
        var cotizacion = CrearCotizacion();
        cotizacion.AgregarDetalle(CrearDetalle(cotizacion.Id, "Servicio A", 2m, 500000m));
        cotizacion.AgregarDetalle(CrearDetalle(cotizacion.Id, "Servicio B", 1m, 300000m, 10m));

        cotizacion.Subtotal.Monto.Should().Be(1270000m);
        cotizacion.TotalNeto.Monto.Should().Be(1270000m);
        cotizacion.Detalles.Should().HaveCount(2);
    }

    [Fact]
    public void RemoverDetalle_EnBorrador_DeberiaQuitarYRecalcular()
    {
        var cotizacion = CrearCotizacion();
        var detalle = CrearDetalle(cotizacion.Id, "Servicio A", 1m, 500000m);
        cotizacion.AgregarDetalle(detalle);

        cotizacion.RemoverDetalle(detalle.Id);

        cotizacion.Detalles.Should().BeEmpty();
        cotizacion.Subtotal.Monto.Should().Be(0m);
    }

    [Fact]
    public void RemoverDetalle_Inexistente_DeberiaLanzarReglaNegocio()
    {
        var cotizacion = CrearCotizacion();

        var accion = () => cotizacion.RemoverDetalle(Guid.NewGuid());

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void Constructor_VigenciaAnteriorAEmision_DeberiaLanzarReglaNegocio()
    {
        var accion = () => new Cotizacion(
            Guid.NewGuid(),
            "Cliente",
            "COT-1",
            new DateTime(2026, 3, 31),
            new DateTime(2026, 3, 1));

        accion.Should().Throw<ReglaNegocioException>();
    }

    [Fact]
    public void AsignarPlantilla_ConId_DeberiaFijarla()
    {
        var cotizacion = CrearCotizacion();
        var plantillaId = Guid.NewGuid();

        cotizacion.AsignarPlantilla(plantillaId);

        cotizacion.PlantillaCotizacionId.Should().Be(plantillaId);
    }
}
