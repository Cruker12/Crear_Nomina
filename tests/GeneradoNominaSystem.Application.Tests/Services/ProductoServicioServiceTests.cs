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
using GeneradoNominaSystem.Domain.ValueObjects;
using Moq;

namespace GeneradoNominaSystem.Application.Tests.Services;

public class ProductoServicioServiceTests
{
    private readonly Mock<IProductoServicioRepository> _productos = new();
    private readonly Mock<ICotizacionRepository> _cotizaciones = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private ProductoServicioService CrearSut()
    {
        return new ProductoServicioService(_productos.Object, _cotizaciones.Object, _uow.Object, new ProductoServicioValidator());
    }

    [Fact]
    public async Task CrearAsync_DtoValido_DeberiaPersistir()
    {
        var sut = CrearSut();
        var dto = new ProductoServicioDto
        {
            EmpresaId = Guid.NewGuid(),
            Nombre = "Consultoría",
            Unidad = "Hora",
            PrecioUnitarioMonto = 150000m,
        };

        var resultado = await sut.CrearAsync(dto);

        resultado.Nombre.Should().Be("Consultoría");
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DesactivarAsync_Inexistente_DeberiaLanzarReglaNegocio()
    {
        _productos.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductoServicio?)null);
        var sut = CrearSut();

        var accion = () => sut.DesactivarAsync(Guid.NewGuid());

        await accion.Should().ThrowAsync<ReglaNegocioException>();
    }

    [Fact]
    public async Task DesactivarYActivar_ProductoExistente_DeberiaCambiarEstado()
    {
        var empresaId = Guid.NewGuid();
        var producto = new ProductoServicio(empresaId, "Consultoría", new Dinero(100000m, "COP"));
        _productos.Setup(r => r.ObtenerPorIdAsync(producto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        var sut = CrearSut();

        await sut.DesactivarAsync(producto.Id);
        producto.Activo.Should().BeFalse();

        await sut.ActivarAsync(producto.Id);
        producto.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task EliminarAsync_SinUso_DeberiaEliminar()
    {
        var empresaId = Guid.NewGuid();
        var producto = new ProductoServicio(empresaId, "Consultoría", new Dinero(100000m, "COP"));
        _productos.Setup(r => r.ObtenerPorIdAsync(producto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _cotizaciones.Setup(r => r.ListarPorEmpresaAsync(empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Cotizacion>());
        var sut = CrearSut();

        await sut.EliminarAsync(producto.Id);

        _productos.Verify(r => r.Eliminar(producto), Times.Once);
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarAsync_EnUso_DeberiaLanzarReglaNegocio()
    {
        var empresaId = Guid.NewGuid();
        var producto = new ProductoServicio(empresaId, "Consultoría", new Dinero(100000m, "COP"));
        var cotizacion = new Cotizacion(empresaId, "Cliente", "COT-2026-1", new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        cotizacion.AgregarDetalle(new DetalleCotizacion(cotizacion.Id, "Servicio", 1m, new Dinero(100000m, "COP"), 1, producto.Id));
        _productos.Setup(r => r.ObtenerPorIdAsync(producto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(producto);
        _cotizaciones.Setup(r => r.ListarPorEmpresaAsync(empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Cotizacion> { cotizacion });
        _cotizaciones.Setup(r => r.ObtenerConDetallesAsync(cotizacion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cotizacion);
        var sut = CrearSut();

        var accion = () => sut.EliminarAsync(producto.Id);

        await accion.Should().ThrowAsync<ReglaNegocioException>();
        _productos.Verify(r => r.Eliminar(It.IsAny<ProductoServicio>()), Times.Never);
    }
}
