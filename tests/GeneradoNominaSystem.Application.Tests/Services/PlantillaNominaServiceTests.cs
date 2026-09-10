using FluentAssertions;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Services;
using GeneradoNominaSystem.Application.Validators;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.ValueObjects;
using Moq;

namespace GeneradoNominaSystem.Application.Tests.Services;

public class PlantillaNominaServiceTests
{
    private readonly Mock<IPlantillaNominaRepository> _plantillas = new();
    private readonly Mock<IConceptoNominaRepository> _conceptos = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Guid _empresaId = Guid.NewGuid();

    private PlantillaNominaService CrearSut()
    {
        return new PlantillaNominaService(_plantillas.Object, _conceptos.Object, _uow.Object, new PlantillaNominaValidator());
    }

    [Fact]
    public async Task CrearAsync_DatosValidos_DeberiaPersistir()
    {
        var sut = CrearSut();
        var dto = new PlantillaNominaDto { EmpresaId = _empresaId, Nombre = "Quincenal base" };

        var resultado = await sut.CrearAsync(dto);

        resultado.Nombre.Should().Be("Quincenal base");
        _plantillas.Verify(r => r.AgregarAsync(It.IsAny<PlantillaNomina>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AgregarConceptoAsync_ConceptoValido_DeberiaAgregarlo()
    {
        var plantilla = new PlantillaNomina(_empresaId, "Base");
        var concepto = new ConceptoNomina("Salario", TipoConcepto.Devengo, 1, _empresaId, valorFijo: new Dinero(0m, "COP"));
        _plantillas.Setup(r => r.ObtenerConConceptosAsync(plantilla.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);
        _conceptos.Setup(r => r.ObtenerPorIdAsync(concepto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepto);
        var sut = CrearSut();

        var resultado = await sut.AgregarConceptoAsync(plantilla.Id, concepto.Id, 1, true, 1500000m);

        resultado.Conceptos.Should().HaveCount(1);
        resultado.Conceptos[0].ValorPorDefectoMonto.Should().Be(1500000m);
    }

    [Fact]
    public async Task AgregarConceptoAsync_Duplicado_DeberiaLanzarReglaNegocio()
    {
        var plantilla = new PlantillaNomina(_empresaId, "Base");
        var concepto = new ConceptoNomina("Salario", TipoConcepto.Devengo, 1, _empresaId, valorFijo: new Dinero(0m, "COP"));
        plantilla.AgregarConcepto(new PlantillaConcepto(plantilla.Id, concepto.Id, 1));
        _plantillas.Setup(r => r.ObtenerConConceptosAsync(plantilla.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);
        _conceptos.Setup(r => r.ObtenerPorIdAsync(concepto.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concepto);
        var sut = CrearSut();

        var accion = () => sut.AgregarConceptoAsync(plantilla.Id, concepto.Id, 2);

        await accion.Should().ThrowAsync<ReglaNegocioException>();
    }

    [Fact]
    public async Task QuitarConceptoAsync_Existente_DeberiaQuitarlo()
    {
        var plantilla = new PlantillaNomina(_empresaId, "Base");
        var conceptoId = Guid.NewGuid();
        plantilla.AgregarConcepto(new PlantillaConcepto(plantilla.Id, conceptoId, 1));
        _plantillas.Setup(r => r.ObtenerConConceptosAsync(plantilla.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);
        var sut = CrearSut();

        await sut.QuitarConceptoAsync(plantilla.Id, conceptoId);

        plantilla.Conceptos.Should().BeEmpty();
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
