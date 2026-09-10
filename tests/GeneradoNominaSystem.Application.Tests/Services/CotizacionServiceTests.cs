using FluentAssertions;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Services;
using GeneradoNominaSystem.Application.Validators;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.Interfaces.Services;
using Moq;

namespace GeneradoNominaSystem.Application.Tests.Services;

public class CotizacionServiceTests
{
    private readonly Mock<ICotizacionRepository> _cotizaciones = new();
    private readonly Mock<IProductoServicioRepository> _productos = new();
    private readonly Mock<IPlantillaCotizacionRepository> _plantillas = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IServicioNumeracion> _numeracion = new();
    private readonly Guid _empresaId = Guid.NewGuid();

    public CotizacionServiceTests()
    {
        _numeracion.Setup(n => n.GenerarNumero("COT", It.IsAny<int>(), It.IsAny<int>()))
            .Returns("COT-2026-0001");
    }

    private CotizacionService CrearSut()
    {
        return new CotizacionService(
            _cotizaciones.Object,
            _productos.Object,
            _plantillas.Object,
            _uow.Object,
            _numeracion.Object,
            new CotizacionValidator(),
            new DetalleCotizacionValidator());
    }

    private static CotizacionDto CrearDtoValido(Guid empresaId)
    {
        return new CotizacionDto
        {
            EmpresaId = empresaId,
            ClienteNombre = "Cliente Test",
            FechaEmision = new DateTime(2026, 3, 1),
            FechaVigencia = new DateTime(2026, 3, 31),
            Moneda = "COP",
        };
    }

    [Fact]
    public async Task CrearAsync_DtoValido_DeberiaGenerarNumeroSecuencial()
    {
        _cotizaciones.Setup(r => r.ListarPorEmpresaAsync(_empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Cotizacion>());
        var sut = CrearSut();

        var resultado = await sut.CrearAsync(CrearDtoValido(_empresaId));

        resultado.NumeroCotizacion.Should().Be("COT-2026-0001");
        resultado.Estado.Should().Be(EstadoCotizacion.Borrador);
        _numeracion.Verify(n => n.GenerarNumero("COT", It.IsAny<int>(), 1), Times.Once);
    }

    [Fact]
    public async Task Flujo_CincoLineas_DeberiaCalcularTotalManual()
    {
        var cotizacion = new Cotizacion(
            _empresaId, "Cliente", "COT-2026-0001",
            new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        _cotizaciones.Setup(r => r.ObtenerConDetallesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cotizacion);
        var sut = CrearSut();

        await sut.AgregarDetalleAsync(cotizacion.Id, "Item 1", 2m, 100000m, "COP");
        await sut.AgregarDetalleAsync(cotizacion.Id, "Item 2", 1m, 250000m, "COP");
        await sut.AgregarDetalleAsync(cotizacion.Id, "Item 3", 3m, 50000m, "COP");
        await sut.AgregarDetalleAsync(cotizacion.Id, "Item 4", 1m, 1000000m, "COP", null, 20m);
        var resultado = await sut.AgregarDetalleAsync(cotizacion.Id, "Item 5", 5m, 20000m, "COP");

        resultado.Detalles.Should().HaveCount(5);
        resultado.Subtotal.Should().Be(200000m + 250000m + 150000m + 800000m + 100000m);
        resultado.TotalNeto.Should().Be(1500000m);
    }

    [Fact]
    public async Task QuitarDetalleAsync_Existente_DeberiaRecalcular()
    {
        var cotizacion = new Cotizacion(
            _empresaId, "Cliente", "COT-2026-0001",
            new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        _cotizaciones.Setup(r => r.ObtenerConDetallesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cotizacion);
        var sut = CrearSut();

        var conDetalle = await sut.AgregarDetalleAsync(cotizacion.Id, "Item", 1m, 500000m, "COP");
        var detalleId = conDetalle.Detalles[0].Id;
        await sut.QuitarDetalleAsync(cotizacion.Id, detalleId);

        cotizacion.Detalles.Should().BeEmpty();
        cotizacion.Subtotal.Monto.Should().Be(0m);
    }

    [Fact]
    public async Task CambiarEstadoAsync_Existente_DeberiaCambiar()
    {
        var cotizacion = new Cotizacion(
            _empresaId, "Cliente", "COT-2026-0001",
            new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        _cotizaciones.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cotizacion);
        var sut = CrearSut();

        await sut.CambiarEstadoAsync(cotizacion.Id, EstadoCotizacion.Enviada);

        cotizacion.Estado.Should().Be(EstadoCotizacion.Enviada);
    }

    [Fact]
    public async Task AsignarPlantillaAsync_PlantillaValida_DeberiaAsignarla()
    {
        var cotizacion = new Cotizacion(
            _empresaId, "Cliente", "COT-2026-0001",
            new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        var plantilla = new PlantillaCotizacion(_empresaId, "Formal");
        _cotizaciones.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cotizacion);
        _plantillas.Setup(r => r.ObtenerPorIdAsync(plantilla.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);
        var sut = CrearSut();

        await sut.AsignarPlantillaAsync(cotizacion.Id, plantilla.Id);

        cotizacion.PlantillaCotizacionId.Should().Be(plantilla.Id);
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AsignarPlantillaAsync_OtraEmpresa_DeberiaLanzarReglaNegocio()
    {
        var cotizacion = new Cotizacion(
            _empresaId, "Cliente", "COT-2026-0001",
            new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        var plantilla = new PlantillaCotizacion(Guid.NewGuid(), "Formal");
        _cotizaciones.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cotizacion);
        _plantillas.Setup(r => r.ObtenerPorIdAsync(plantilla.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);
        var sut = CrearSut();

        var accion = () => sut.AsignarPlantillaAsync(cotizacion.Id, plantilla.Id);

        await accion.Should().ThrowAsync<ReglaNegocioException>();
    }
}
