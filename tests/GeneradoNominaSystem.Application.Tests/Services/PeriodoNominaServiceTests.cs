using FluentAssertions;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Services;
using GeneradoNominaSystem.Application.Validators;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using Moq;

namespace GeneradoNominaSystem.Application.Tests.Services;

public class PeriodoNominaServiceTests
{
    private readonly Mock<IPeriodoNominaRepository> _periodos = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private PeriodoNominaService CrearSut()
    {
        return new PeriodoNominaService(_periodos.Object, _uow.Object, new PeriodoNominaValidator());
    }

    private static PeriodoNominaDto CrearDto(Guid empresaId)
    {
        return new PeriodoNominaDto
        {
            EmpresaId = empresaId,
            Nombre = "Marzo 2026",
            Tipo = TipoPeriodo.Mensual,
            FechaInicio = new DateTime(2026, 3, 1),
            FechaFin = new DateTime(2026, 3, 31),
        };
    }

    [Fact]
    public async Task CrearAsync_SinSolapamiento_DeberiaPersistir()
    {
        var empresaId = Guid.NewGuid();
        _periodos.Setup(r => r.ListarPorEmpresaAsync(empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PeriodoNomina>());
        var sut = CrearSut();

        var resultado = await sut.CrearAsync(CrearDto(empresaId));

        resultado.Nombre.Should().Be("Marzo 2026");
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrearAsync_SolapadoMismoTipo_DeberiaLanzarReglaNegocio()
    {
        var empresaId = Guid.NewGuid();
        var existente = new PeriodoNomina(empresaId, "Marzo 2026", TipoPeriodo.Mensual, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        _periodos.Setup(r => r.ListarPorEmpresaAsync(empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PeriodoNomina> { existente });
        var sut = CrearSut();

        var accion = () => sut.CrearAsync(CrearDto(empresaId));

        await accion.Should().ThrowAsync<ReglaNegocioException>();
    }

    [Fact]
    public async Task CrearAsync_SolapadoOtroTipo_DeberiaPermitir()
    {
        var empresaId = Guid.NewGuid();
        var existente = new PeriodoNomina(empresaId, "Especial", TipoPeriodo.Especial, new DateTime(2026, 3, 10), new DateTime(2026, 3, 20));
        _periodos.Setup(r => r.ListarPorEmpresaAsync(empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PeriodoNomina> { existente });
        var sut = CrearSut();

        var resultado = await sut.CrearAsync(CrearDto(empresaId));

        resultado.Should().NotBeNull();
    }
}
