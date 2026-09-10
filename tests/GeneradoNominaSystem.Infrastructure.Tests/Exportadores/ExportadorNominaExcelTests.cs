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
    public void ExportarCotizacion_ModeloValido_DeberiaGenerarExcel()
    {
        var sut = new ExportadorNominaExcel();
        var ruta = Path.Combine(Path.GetTempPath(), $"gns-cot-{Guid.NewGuid():N}.xlsx");

        try
        {
            sut.ExportarCotizacion(ModeloDocumentoMuestra.CrearCotizacion(), ruta);

            File.Exists(ruta).Should().BeTrue();
            new FileInfo(ruta).Length.Should().BeGreaterThan(0);
        }
        finally
        {
            if (File.Exists(ruta))
            {
                File.Delete(ruta);
            }
        }
    }
}
