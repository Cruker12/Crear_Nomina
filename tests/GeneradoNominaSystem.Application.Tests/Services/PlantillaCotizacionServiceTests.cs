using FluentAssertions;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Services;
using GeneradoNominaSystem.Application.Validators;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using Moq;

namespace GeneradoNominaSystem.Application.Tests.Services;

public class PlantillaCotizacionServiceTests
{
    private readonly Mock<IPlantillaCotizacionRepository> _plantillas = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private PlantillaCotizacionService CrearSut()
    {
        return new PlantillaCotizacionService(_plantillas.Object, _uow.Object, new PlantillaCotizacionValidator());
    }

    [Fact]
    public async Task CrearAsync_DatosValidos_DeberiaPersistir()
    {
        var sut = CrearSut();
        var dto = new PlantillaCotizacionDto { EmpresaId = Guid.NewGuid(), Nombre = "Formal" };

        var resultado = await sut.CrearAsync(dto);

        resultado.Nombre.Should().Be("Formal");
        _plantillas.Verify(r => r.AgregarAsync(It.IsAny<PlantillaCotizacion>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_NombreVacio_DeberiaFallarValidacion()
    {
        var sut = CrearSut();
        var dto = new PlantillaCotizacionDto { EmpresaId = Guid.NewGuid(), Nombre = string.Empty };

        var accion = () => sut.CrearAsync(dto);

        await accion.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task DesactivarAsync_Inexistente_DeberiaLanzarReglaNegocio()
    {
        _plantillas.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlantillaCotizacion?)null);
        var sut = CrearSut();

        var accion = () => sut.DesactivarAsync(Guid.NewGuid());

        await accion.Should().ThrowAsync<ReglaNegocioException>();
    }
}
