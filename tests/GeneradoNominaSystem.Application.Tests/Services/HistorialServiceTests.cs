using FluentAssertions;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Application.Services;
using GeneradoNominaSystem.Domain.Enums;
using Moq;

namespace GeneradoNominaSystem.Application.Tests.Services;

public class HistorialServiceTests
{
    private readonly Mock<INominaService> _nominas = new();
    private readonly Mock<ICotizacionService> _cotizaciones = new();
    private readonly Mock<IDocumentoService> _documentos = new();
    private readonly Guid _empresaId = Guid.NewGuid();

    private HistorialService CrearSut()
    {
        return new HistorialService(_nominas.Object, _cotizaciones.Object, _documentos.Object);
    }

    private static NominaDto CrearNomina(string empleado, EstadoNomina estado, DateTime creada)
    {
        return new NominaDto
        {
            Id = Guid.NewGuid(),
            EmpresaId = Guid.NewGuid(),
            EmpleadoNombre = empleado,
            PeriodoNombre = "Marzo 2026",
            Estado = estado,
            FechaCreacion = creada,
        };
    }

    private static CotizacionDto CrearCotizacion(string cliente, string numero, EstadoCotizacion estado, DateTime emision)
    {
        return new CotizacionDto
        {
            Id = Guid.NewGuid(),
            ClienteNombre = cliente,
            NumeroCotizacion = numero,
            Estado = estado,
            FechaEmision = emision,
        };
    }

    private static DocumentoDto CrearDocumento(TipoDocumentoSistema tipo, FormatoExportacion formato, DateTime generado)
    {
        return new DocumentoDto
        {
            Id = Guid.NewGuid(),
            Tipo = tipo,
            Formato = formato,
            NumeroDocumento = "DOC-1",
            RutaArchivo = "ruta",
            FechaGeneracion = generado,
        };
    }

    [Fact]
    public async Task BuscarNominas_PorTextoYEstado_DeberiaFiltrar()
    {
        var nominas = new List<NominaDto>
        {
            CrearNomina("Juan Pérez", EstadoNomina.Aprobada, new DateTime(2026, 3, 10)),
            CrearNomina("Ana Gómez", EstadoNomina.Calculada, new DateTime(2026, 3, 11)),
            CrearNomina("Juan Torres", EstadoNomina.Aprobada, new DateTime(2026, 2, 5)),
        };
        _nominas.Setup(s => s.ListarPorEmpresaAsync(_empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nominas);
        var sut = CrearSut();

        var resultado = await sut.BuscarNominasAsync(_empresaId, "juan", EstadoNomina.Aprobada);

        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task BuscarNominas_PorFechas_DeberiaFiltrar()
    {
        var nominas = new List<NominaDto>
        {
            CrearNomina("A", EstadoNomina.Pagada, new DateTime(2026, 3, 10)),
            CrearNomina("B", EstadoNomina.Pagada, new DateTime(2026, 1, 5)),
        };
        _nominas.Setup(s => s.ListarPorEmpresaAsync(_empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nominas);
        var sut = CrearSut();

        var resultado = await sut.BuscarNominasAsync(_empresaId, null, null, new DateTime(2026, 3, 1), new DateTime(2026, 3, 31));

        resultado.Should().ContainSingle().Which.EmpleadoNombre.Should().Be("A");
    }

    [Fact]
    public async Task BuscarCotizaciones_PorTexto_DeberiaFiltrar()
    {
        var cotizaciones = new List<CotizacionDto>
        {
            CrearCotizacion("Acme", "COT-2026-0001", EstadoCotizacion.Enviada, new DateTime(2026, 3, 2)),
            CrearCotizacion("Globex", "COT-2026-0002", EstadoCotizacion.Borrador, new DateTime(2026, 3, 3)),
        };
        _cotizaciones.Setup(s => s.ListarPorEmpresaAsync(_empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cotizaciones);
        var sut = CrearSut();

        var porCliente = await sut.BuscarCotizacionesAsync(_empresaId, "acme");
        var porNumero = await sut.BuscarCotizacionesAsync(_empresaId, "0002");

        porCliente.Should().ContainSingle().Which.ClienteNombre.Should().Be("Acme");
        porNumero.Should().ContainSingle().Which.NumeroCotizacion.Should().Be("COT-2026-0002");
    }

    [Fact]
    public async Task ListarDocumentos_PorTipoYFormato_DeberiaFiltrar()
    {
        var documentos = new List<DocumentoDto>
        {
            CrearDocumento(TipoDocumentoSistema.Nomina, FormatoExportacion.Pdf, new DateTime(2026, 3, 10)),
            CrearDocumento(TipoDocumentoSistema.Nomina, FormatoExportacion.Excel, new DateTime(2026, 3, 11)),
            CrearDocumento(TipoDocumentoSistema.Cotizacion, FormatoExportacion.Pdf, new DateTime(2026, 3, 12)),
        };
        _documentos.Setup(s => s.ListarPorEmpresaAsync(_empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documentos);
        var sut = CrearSut();

        var nominasPdf = await sut.ListarDocumentosAsync(_empresaId, TipoDocumentoSistema.Nomina, FormatoExportacion.Pdf);

        nominasPdf.Should().ContainSingle();
    }
}
