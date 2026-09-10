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

public class ProductoServicioServiceTests
{
    private readonly Mock<IProductoServicioRepository> _productos = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private ProductoServicioService CrearSut()
    {
        return new ProductoServicioService(_productos.Object, _uow.Object, new ProductoServicioValidator());
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
}
