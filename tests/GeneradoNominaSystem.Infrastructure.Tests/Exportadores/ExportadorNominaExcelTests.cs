using FluentAssertions;
using GeneradoNominaSystem.Domain.Documentos;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Infrastructure.Exportadores;

namespace GeneradoNominaSystem.Infrastructure.Tests.Exportadores;

public class ExportadorNominaExcelTests
{
    [Fact]
    public void ExportarNomina_ModeloValido_DeberiaGenerarExcel()
    {
        var sut = new ExportadorNominaExcel();
        var ruta = Path.Combine(Path.GetTempPath(), $"gns-nomina-{Guid.NewGuid():N}.xlsx");

        try
        {
            sut.ExportarNomina(ModeloDocumentoMuestra.CrearNomina(), ruta);

            File.Exists(ruta).Should().BeTrue();
            new FileInfo(ruta).Length.Should().BeGreaterThan(0);
            sut.Formato.Should().Be(FormatoExportacion.Excel);
        }
        finally
        {
            if (File.Exists(ruta))
            {
                File.Delete(ruta);
            }
        }
    }

    [Fact]
    public void ExportarCotizacion_DeberiaLanzarNotSupported()
    {
        var sut = new ExportadorNominaExcel();
        var modelo = new ModeloDocumentoCotizacion(
            "E", "N", null, null, "C", null, "COT-1",
            DateTime.Today, DateTime.Today, "Borrador",
            new List<LineaDocumentoCotizacion>(), 0m, 0m, "COP", null);

        var accion = () => sut.ExportarCotizacion(modelo, Path.GetTempPath());

        accion.Should().Throw<NotSupportedException>();
    }
}
