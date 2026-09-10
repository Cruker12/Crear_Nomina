using FluentAssertions;
using GeneradoNominaSystem.Domain.Documentos;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Infrastructure.Exportadores;

namespace GeneradoNominaSystem.Infrastructure.Tests.Exportadores;

public class ExportadorNominaPdfTests
{
    [Fact]
    public void ExportarNomina_ModeloValido_DeberiaGenerarPdf()
    {
        var sut = new ExportadorNominaPdf();
        var ruta = Path.Combine(Path.GetTempPath(), $"gns-nomina-{Guid.NewGuid():N}.pdf");

        try
        {
            sut.ExportarNomina(ModeloDocumentoMuestra.CrearNomina(), ruta);

            File.Exists(ruta).Should().BeTrue();
            new FileInfo(ruta).Length.Should().BeGreaterThan(0);
            sut.Formato.Should().Be(FormatoExportacion.Pdf);
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
    public void ExportarCotizacion_ModeloValido_DeberiaGenerarPdf()
    {
        var sut = new ExportadorNominaPdf();
        var ruta = Path.Combine(Path.GetTempPath(), $"gns-cot-{Guid.NewGuid():N}.pdf");

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
