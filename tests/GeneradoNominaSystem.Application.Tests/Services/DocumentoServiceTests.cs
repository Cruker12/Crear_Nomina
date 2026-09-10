using FluentAssertions;
using GeneradoNominaSystem.Application.Services;
using GeneradoNominaSystem.Domain.Documentos;
using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces;
using GeneradoNominaSystem.Domain.Interfaces.Repositories;
using GeneradoNominaSystem.Domain.Interfaces.Services;
using GeneradoNominaSystem.Domain.ValueObjects;
using Moq;

namespace GeneradoNominaSystem.Application.Tests.Services;

public class DocumentoServiceTests : IDisposable
{
    private readonly Mock<INominaRepository> _nominas = new();
    private readonly Mock<ICotizacionRepository> _cotizaciones = new();
    private readonly Mock<IProductoServicioRepository> _productosServicios = new();
    private readonly Mock<IEmpresaRepository> _empresas = new();
    private readonly Mock<IEmpleadoRepository> _empleados = new();
    private readonly Mock<IPeriodoNominaRepository> _periodos = new();
    private readonly Mock<IConceptoNominaRepository> _conceptos = new();
    private readonly Mock<IDocumentoRepository> _documentos = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IServicioNumeracion> _numeracion = new();
    private readonly Mock<IExportadorDocumento> _exportador = new();

    private readonly Guid _empresaId = Guid.NewGuid();
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), $"gns-doc-{Guid.NewGuid():N}");

    private readonly Empleado _empleado;
    private readonly PeriodoNomina _periodo;
    private readonly ConceptoNomina _salario;
    private readonly Empresa _empresa;

    public DocumentoServiceTests()
    {
        Directory.CreateDirectory(_carpeta);

        _empresa = new Empresa(
            "Empresa Test S.A.S.",
            "Empresa Test",
            "900123456",
            new Direccion("Calle 1", "Bogotá", "Cundinamarca"),
            new DatosFiscales("900123456", "Empresa Test S.A.S."));

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

        _exportador.SetupGet(e => e.Formato).Returns(FormatoExportacion.Pdf);
        _exportador.Setup(e => e.ExportarNomina(It.IsAny<ModeloDocumentoNomina>(), It.IsAny<string>()))
            .Callback<ModeloDocumentoNomina, string>((_, ruta) => File.WriteAllText(ruta, "dummy-pdf"));
        _numeracion.Setup(n => n.GenerarNumero("NOM", It.IsAny<int>(), It.IsAny<int>()))
            .Returns("NOM-2026-0001");
    }

    private static Nomina CrearNominaCalculada(Guid empresaId, Guid empleadoId, Guid periodoId, Guid conceptoId)
    {
        var nomina = new Nomina(empresaId, empleadoId, periodoId);
        nomina.AgregarDetalle(new DetalleNomina(nomina.Id, conceptoId, new Dinero(2000000m, "COP"), 1));
        nomina.AplicarTotales(new Dinero(2000000m, "COP"), Dinero.Cero());
        nomina.MarcarCalculada();
        return nomina;
    }

    private DocumentoService CrearSut()
    {
        return new DocumentoService(
            _nominas.Object,
            _cotizaciones.Object,
            _productosServicios.Object,
            _empresas.Object,
            _empleados.Object,
            _periodos.Object,
            _conceptos.Object,
            _documentos.Object,
            _uow.Object,
            _numeracion.Object,
            new List<IExportadorDocumento> { _exportador.Object });
    }

    private void ConfigurarComunes(Nomina nomina)
    {
        _nominas.Setup(r => r.ObtenerConDetallesAsync(nomina.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nomina);
        _empresas.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_empresa);
        _empleados.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_empleado);
        _periodos.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_periodo);
        _conceptos.Setup(r => r.ListarActivosAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ConceptoNomina> { _salario });
        _documentos.Setup(r => r.ListarPorEmpresaAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Documento>());
    }

    [Fact]
    public async Task ExportarNominaAsync_NominaCalculada_DeberiaGenerarYRegistrar()
    {
        var nomina = CrearNominaCalculada(_empresaId, _empleado.Id, _periodo.Id, _salario.Id);
        ConfigurarComunes(nomina);
        var sut = CrearSut();

        var resultado = await sut.ExportarNominaAsync(nomina.Id, FormatoExportacion.Pdf, _carpeta);

        resultado.NumeroDocumento.Should().Be("NOM-2026-0001");
        resultado.TamanoBytes.Should().BeGreaterThan(0);
        File.Exists(resultado.RutaArchivo).Should().BeTrue();
        _documentos.Verify(r => r.AgregarAsync(It.IsAny<Documento>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExportarNominaAsync_NominaBorrador_DeberiaLanzarReglaNegocio()
    {
        var nomina = new Nomina(_empresaId, _empleado.Id, _periodo.Id);
        _nominas.Setup(r => r.ObtenerConDetallesAsync(nomina.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nomina);
        var sut = CrearSut();

        var accion = () => sut.ExportarNominaAsync(nomina.Id, FormatoExportacion.Pdf, _carpeta);

        await accion.Should().ThrowAsync<ReglaNegocioException>();
        _exportador.Verify(e => e.ExportarNomina(It.IsAny<ModeloDocumentoNomina>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExportarNominaAsync_DosVeces_DeberiaCrearDosRegistrosConRutasDistintas()
    {
        var nomina = CrearNominaCalculada(_empresaId, _empleado.Id, _periodo.Id, _salario.Id);
        ConfigurarComunes(nomina);
        var sut = CrearSut();

        var primero = await sut.ExportarNominaAsync(nomina.Id, FormatoExportacion.Pdf, _carpeta);
        var segundo = await sut.ExportarNominaAsync(nomina.Id, FormatoExportacion.Pdf, _carpeta);

        _documentos.Verify(r => r.AgregarAsync(It.IsAny<Documento>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        primero.RutaArchivo.Should().NotBe(segundo.RutaArchivo);
        File.Exists(primero.RutaArchivo).Should().BeTrue();
        File.Exists(segundo.RutaArchivo).Should().BeTrue();
    }

    [Fact]
    public async Task ExportarCotizacionAsync_ConDetalles_DeberiaGenerarYRegistrar()
    {
        var cotizacion = new Cotizacion(
            _empresaId, "Cliente Test", "COT-2026-0001",
            new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        cotizacion.AgregarDetalle(new DetalleCotizacion(
            cotizacion.Id, "Consultoría", 2m, new Dinero(150000m, "COP"), 1));
        _cotizaciones.Setup(r => r.ObtenerConDetallesAsync(cotizacion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cotizacion);
        _empresas.Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_empresa);
        _exportador.Setup(e => e.ExportarCotizacion(It.IsAny<ModeloDocumentoCotizacion>(), It.IsAny<string>()))
            .Callback<ModeloDocumentoCotizacion, string>((_, ruta) => File.WriteAllText(ruta, "dummy-xlsx"));
        var sut = CrearSut();

        var resultado = await sut.ExportarCotizacionAsync(cotizacion.Id, FormatoExportacion.Pdf, _carpeta);

        resultado.NumeroDocumento.Should().Be("COT-2026-0001");
        resultado.TamanoBytes.Should().BeGreaterThan(0);
        File.Exists(resultado.RutaArchivo).Should().BeTrue();
        _documentos.Verify(r => r.AgregarAsync(It.IsAny<Documento>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExportarCotizacionAsync_SinDetalles_DeberiaLanzarReglaNegocio()
    {
        var cotizacion = new Cotizacion(
            _empresaId, "Cliente Test", "COT-2026-0001",
            new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));
        _cotizaciones.Setup(r => r.ObtenerConDetallesAsync(cotizacion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cotizacion);
        var sut = CrearSut();

        var accion = () => sut.ExportarCotizacionAsync(cotizacion.Id, FormatoExportacion.Pdf, _carpeta);

        await accion.Should().ThrowAsync<ReglaNegocioException>();
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_carpeta))
            {
                Directory.Delete(_carpeta, recursive: true);
            }
        }
        catch
        {
        }
    }
}
