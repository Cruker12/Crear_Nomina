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
    private readonly Mock<INominaRepository> _nominas = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private PeriodoNominaService CrearSut()
    {
        return new PeriodoNominaService(_periodos.Object, _nominas.Object, _uow.Object, new PeriodoNominaValidator());
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

    [Fact]
    public async Task DesactivarYActivar_PeriodoExistente_DeberiaCambiarEstado()
    {
        var empresaId = Guid.NewGuid();
        var periodo = new PeriodoNomina(empresaId, "Marzo 2026", TipoPeriodo.Mensual, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        _periodos.Setup(r => r.ObtenerPorIdAsync(periodo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(periodo);
        var sut = CrearSut();

        await sut.DesactivarAsync(periodo.Id);
        periodo.Activo.Should().BeFalse();

        await sut.ActivarAsync(periodo.Id);
        periodo.Activo.Should().BeTrue();
    }

    [Fact]
    public async Task EliminarAsync_SinNominas_DeberiaEliminar()
    {
        var empresaId = Guid.NewGuid();
        var periodo = new PeriodoNomina(empresaId, "Marzo 2026", TipoPeriodo.Mensual, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        _periodos.Setup(r => r.ObtenerPorIdAsync(periodo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(periodo);
        _nominas.Setup(r => r.ListarPorPeriodoAsync(periodo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Nomina>());
        var sut = CrearSut();

        await sut.EliminarAsync(periodo.Id);

        _periodos.Verify(r => r.Eliminar(periodo), Times.Once);
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EliminarAsync_ConNominas_DeberiaLanzarReglaNegocio()
    {
        var empresaId = Guid.NewGuid();
        var periodo = new PeriodoNomina(empresaId, "Marzo 2026", TipoPeriodo.Mensual, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        var nomina = new Nomina(empresaId, Guid.NewGuid(), periodo.Id);
        _periodos.Setup(r => r.ObtenerPorIdAsync(periodo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(periodo);
        _nominas.Setup(r => r.ListarPorPeriodoAsync(periodo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Nomina> { nomina });
        var sut = CrearSut();

        var accion = () => sut.EliminarAsync(periodo.Id);

        await accion.Should().ThrowAsync<ReglaNegocioException>();
        _periodos.Verify(r => r.Eliminar(It.IsAny<PeriodoNomina>()), Times.Never);
    }
}
