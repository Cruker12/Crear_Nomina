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

public class ConceptoNominaServiceTests
{
    private readonly Mock<IConceptoNominaRepository> _conceptos = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private ConceptoNominaService CrearSut()
    {
        return new ConceptoNominaService(_conceptos.Object, _uow.Object, new ConceptoNominaValidator());
    }

    [Fact]
    public async Task CrearAsync_ConceptoFijoValido_DeberiaPersistir()
    {
        var sut = CrearSut();
        var dto = new ConceptoNominaDto
        {
            EmpresaId = Guid.NewGuid(),
            Nombre = "Salario Base",
            Tipo = TipoConcepto.Devengo,
            Orden = 1,
            ValorFijoMonto = 1500000m,
        };

        var resultado = await sut.CrearAsync(dto);

        resultado.Nombre.Should().Be("Salario Base");
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_PorcentajeSinValor_DeberiaFallarValidacion()
    {
        var sut = CrearSut();
        var dto = new ConceptoNominaDto
        {
            Nombre = "Salud",
            Tipo = TipoConcepto.Deduccion,
            Orden = 2,
            EsPorcentaje = true,
            PorcentajeBase = null,
        };

        var accion = () => sut.CrearAsync(dto);

        await accion.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task DesactivarAsync_Inexistente_DeberiaLanzarReglaNegocio()
    {
        _conceptos.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConceptoNomina?)null);
        var sut = CrearSut();

        var accion = () => sut.DesactivarAsync(Guid.NewGuid());

        await accion.Should().ThrowAsync<ReglaNegocioException>();
    }
}
