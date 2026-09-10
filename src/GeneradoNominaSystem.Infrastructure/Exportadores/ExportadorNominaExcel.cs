using ClosedXML.Excel;
using GeneradoNominaSystem.Domain.Documentos;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces.Services;

namespace GeneradoNominaSystem.Infrastructure.Exportadores;

public sealed class ExportadorNominaExcel : IExportadorDocumento
{
    public FormatoExportacion Formato => FormatoExportacion.Excel;

    public void ExportarNomina(ModeloDocumentoNomina modelo, string rutaDestino)
    {
        if (modelo is null)
        {
            throw new ReglaNegocioException("El modelo del documento es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(rutaDestino))
        {
            throw new ReglaNegocioException("La ruta de destino es obligatoria.");
        }

        AsegurarDirectorio(rutaDestino);

        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Nómina");

        hoja.Cell(1, 1).Value = modelo.EmpresaNombre;
        hoja.Cell(1, 1).Style.Font.SetBold().Font.FontSize = 14;
        hoja.Cell(2, 1).Value = $"NIT {modelo.EmpresaNit}";
        hoja.Cell(3, 1).Value = $"Comprobante de nómina {modelo.NumeroDocumento}";
        hoja.Cell(3, 1).Style.Font.SetBold();
        hoja.Cell(4, 1).Value = $"Periodo: {modelo.PeriodoNombre}";
        hoja.Cell(5, 1).Value = $"Empleado: {modelo.EmpleadoNombre} ({modelo.EmpleadoDocumento})";
        hoja.Cell(6, 1).Value = $"Fecha: {modelo.FechaGeneracion:dd/MM/yyyy}  |  Estado: {modelo.Estado}";

        hoja.Cell(8, 1).Value = "Concepto";
        hoja.Cell(8, 2).Value = "Tipo";
        hoja.Cell(8, 3).Value = "Cantidad";
        hoja.Cell(8, 4).Value = $"Valor ({modelo.Moneda})";
        hoja.Range(8, 1, 8, 4).Style.Font.SetBold();

        var fila = 9;
        foreach (var linea in modelo.Lineas.OrderBy(l => l.Orden))
        {
            hoja.Cell(fila, 1).Value = linea.ConceptoNombre;
            hoja.Cell(fila, 2).Value = linea.Tipo.ToString();
            hoja.Cell(fila, 3).Value = linea.Cantidad;
            hoja.Cell(fila, 4).Value = linea.ValorMonto;
            hoja.Cell(fila, 4).Style.NumberFormat.Format = "#,##0.00";
            fila++;
        }

        fila++;
        hoja.Cell(fila, 3).Value = "Devengos:";
        hoja.Cell(fila, 3).Style.Font.SetBold();
        hoja.Cell(fila, 4).Value = modelo.SubtotalDevengos;
        hoja.Cell(fila, 4).Style.NumberFormat.Format = "#,##0.00";
        fila++;
        hoja.Cell(fila, 3).Value = "Deducciones:";
        hoja.Cell(fila, 3).Style.Font.SetBold();
        hoja.Cell(fila, 4).Value = modelo.SubtotalDeducciones;
        hoja.Cell(fila, 4).Style.NumberFormat.Format = "#,##0.00";
        fila++;
        hoja.Cell(fila, 3).Value = "Neto:";
        hoja.Cell(fila, 3).Style.Font.SetBold().Font.FontSize = 12;
        hoja.Cell(fila, 4).Value = modelo.TotalNeto;
        hoja.Cell(fila, 4).Style.Font.SetBold().Font.FontSize = 12;
        hoja.Cell(fila, 4).Style.NumberFormat.Format = "#,##0.00";

        hoja.Columns().AdjustToContents();

        libro.SaveAs(rutaDestino);
    }

    public void ExportarCotizacion(ModeloDocumentoCotizacion modelo, string rutaDestino)
    {
        throw new NotSupportedException("La exportación de cotizaciones se implementa en Fase 11.");
    }

    private static void AsegurarDirectorio(string ruta)
    {
        var directorio = Path.GetDirectoryName(Path.GetFullPath(ruta));
        if (!string.IsNullOrWhiteSpace(directorio))
        {
            Directory.CreateDirectory(directorio);
        }
    }
}
