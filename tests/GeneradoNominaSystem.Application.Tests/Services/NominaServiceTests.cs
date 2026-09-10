using FluentAssertions;
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

public class NominaServiceTests
{
    private readonly Mock<INominaRepository> _nominas = new();
    private readonly Mock<IEmpleadoRepository> _empleados = new();
    private readonly Mock<IPeriodoNominaRepository> _periodos = new();
    private readonly Mock<IConceptoNominaRepository> _conceptos = new();
    private readonly Mock<IPlantillaNominaRepository> _plantillas = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private readonly Guid _empresaId = Guid.NewGuid();
    private readonly Empleado _empleado;
    private readonly PeriodoNomina _periodo;
    private readonly ConceptoNomina _salario;
    private readonly ConceptoNomina _salud;

    public NominaServiceTests()
    {
        _empleado = new Empleado(
            _empresaId,
            TipoDocumentoIdentidad.Cedula,
            "12345678",
            "Juan",
            "Pérez",
            new DateTime(2024, 1, 15),
            "Auxiliar",
            new Dinero(2000000m, "COP"));

        _periodo = new PeriodoNomina(_empresaId, "Marzo 2026", TipoPeriodo.Mensual, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));

        _salario = new ConceptoNomina("Salario", TipoConcepto.Devengo, 1, _empresaId, valorFijo: new Dinero(2000000m, "COP"));
        _salud = new ConceptoNomina("Salud", TipoConcepto.Deduccion, 2, _empresaId, esPorcentaje: true, porcentajeBase: 4m);
    }

    private NominaService CrearSut()
    {
        return new NominaService(
            _nominas.Object,
            _empleados.Object,
            _periodos.Object,
            _conceptos.Object,
            _plantillas.Object,
            _uow.Object,
            new DetalleNominaValidator());
    }

    private void ConfigurarComunes(Nomina nomina)
    {
        _empleados.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_empleado);
        _periodos.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_periodo);
        _nominas.Setup(r => r.ObtenerConDetallesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(nomina);
        _nominas.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(nomina);
        _conceptos.Setup(r => r.ObtenerPorIdAsync(_salario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_salario);
        _conceptos.Setup(r => r.ObtenerPorIdAsync(_salud.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_salud);
        _conceptos.Setup(r => r.ListarActivosAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConceptoNomina> { _salario, _salud });
    }

    [Fact]
    public async Task CrearAsync_DatosValidos_DeberiaRetornarBorrador()
    {
        var nomina = new Nomina(_empresaId, _empleado.Id, _periodo.Id);
        ConfigurarComunes(nomina);
        var sut = CrearSut();

        var resultado = await sut.CrearAsync(_empresaId, _empleado.Id, _periodo.Id);

        resultado.Estado.Should().Be(EstadoNomina.Borrador);
        _nominas.Verify(r => r.AgregarAsync(It.IsAny<Nomina>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Flujo_AgregarCalcularAprobar_DeberiaCompletar()
    {
        var nomina = new Nomina(_empresaId, _empleado.Id, _periodo.Id);
        ConfigurarComunes(nomina);
        var sut = CrearSut();

        await sut.AgregarDetalleAsync(nomina.Id, _salario.Id, 2000000m, "COP");
        await sut.AgregarDetalleAsync(nomina.Id, _salud.Id, 0m, "COP");
        var calculada = await sut.CalcularAsync(nomina.Id, 2000000m);

        calculada.Estado.Should().Be(EstadoNomina.Calculada);
        calculada.SubtotalDevengos.Should().Be(2000000m);
        calculada.SubtotalDeducciones.Should().Be(80000m);
        calculada.TotalNeto.Should().Be(1920000m);

        await sut.AprobarAsync(nomina.Id);

        nomina.Estado.Should().Be(EstadoNomina.Aprobada);
    }

    [Fact]
    public async Task AprobarAsync_SinCalcular_DeberiaLanzarReglaNegocio()
    {
        var nomina = new Nomina(_empresaId, _empleado.Id, _periodo.Id);
        _nominas.Setup(r => r.ObtenerPorIdAsync(nomina.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nomina);
        var sut = CrearSut();

        var accion = () => sut.AprobarAsync(nomina.Id);

        await accion.Should().ThrowAsync<ReglaNegocioException>();
    }

    [Fact]
    public async Task CalcularAsync_SinDetalles_DeberiaLanzarReglaNegocio()
    {
        var nomina = new Nomina(_empresaId, _empleado.Id, _periodo.Id);
        _nominas.Setup(r => r.ObtenerConDetallesAsync(nomina.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nomina);
        var sut = CrearSut();

        var accion = () => sut.CalcularAsync(nomina.Id);

        await accion.Should().ThrowAsync<ReglaNegocioException>();
    }

    [Fact]
    public async Task CrearDesdePlantillaAsync_PlantillaConDosConceptos_DeberiaPrecargarDetalles()
    {
        var plantilla = new PlantillaNomina(_empresaId, "Quincenal base");
        plantilla.AgregarConcepto(new PlantillaConcepto(plantilla.Id, _salario.Id, 1, true, new Dinero(2000000m, "COP")));
        plantilla.AgregarConcepto(new PlantillaConcepto(plantilla.Id, _salud.Id, 2));

        Nomina? capturada = null;
        _nominas.Setup(r => r.AgregarAsync(It.IsAny<Nomina>(), It.IsAny<CancellationToken>()))
            .Callback<Nomina, CancellationToken>((n, _) => capturada = n);
        _nominas.Setup(r => r.ObtenerConDetallesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => capturada);
        _empleados.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_empleado);
        _periodos.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_periodo);
        _plantillas.Setup(r => r.ObtenerConConceptosAsync(plantilla.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);
        _conceptos.Setup(r => r.ObtenerPorIdAsync(_salario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_salario);
        _conceptos.Setup(r => r.ObtenerPorIdAsync(_salud.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_salud);
        _conceptos.Setup(r => r.ListarActivosAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConceptoNomina> { _salario, _salud });
        var sut = CrearSut();

        var resultado = await sut.CrearDesdePlantillaAsync(_empresaId, _empleado.Id, _periodo.Id, plantilla.Id);

        resultado.Detalles.Should().HaveCount(2);
        resultado.Detalles.Should().Contain(d => d.ConceptoNominaId == _salario.Id && d.ValorMonto == 2000000m);
    }

    [Fact]
    public async Task CrearDesdePlantillaAsync_PlantillaVacia_DeberiaLanzarReglaNegocio()
    {
        var plantilla = new PlantillaNomina(_empresaId, "Vacía");
        _plantillas.Setup(r => r.ObtenerConConceptosAsync(plantilla.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(plantilla);
        var sut = CrearSut();

        var accion = () => sut.CrearDesdePlantillaAsync(_empresaId, _empleado.Id, _periodo.Id, plantilla.Id);

        await accion.Should().ThrowAsync<ReglaNegocioException>();
    }
}
