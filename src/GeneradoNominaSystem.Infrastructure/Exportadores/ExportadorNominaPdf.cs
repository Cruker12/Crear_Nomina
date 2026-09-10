using GeneradoNominaSystem.Domain.Documentos;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.Interfaces.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GeneradoNominaSystem.Infrastructure.Exportadores;

public sealed class ExportadorNominaPdf : IExportadorDocumento
{
    static ExportadorNominaPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public FormatoExportacion Formato => FormatoExportacion.Pdf;

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

        Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(2, Unit.Centimetre);
                pagina.DefaultTextStyle(x => x.FontSize(10));

                pagina.Header().Column(columna =>
                {
                    columna.Item().Text(modelo.EmpresaNombre).Bold().FontSize(16);
                    columna.Item().Text($"NIT {modelo.EmpresaNit}").FontSize(10);
                    if (!string.IsNullOrWhiteSpace(modelo.EmpresaDireccion))
                    {
                        columna.Item().Text(modelo.EmpresaDireccion).FontSize(9);
                    }

                    columna.Item().PaddingTop(8).Text($"Comprobante de nómina {modelo.NumeroDocumento}").Bold().FontSize(13);
                    columna.Item().Text($"Periodo: {modelo.PeriodoNombre}  |  Fecha: {modelo.FechaGeneracion:dd/MM/yyyy}  |  Estado: {modelo.Estado}").FontSize(9);
                });

                pagina.Content().PaddingVertical(10).Column(columna =>
                {
                    columna.Item().Text($"Empleado: {modelo.EmpleadoNombre} ({modelo.EmpleadoDocumento})").Bold();

                    columna.Item().PaddingTop(8).Table(tabla =>
                    {
                        tabla.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(4);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        tabla.Header(h =>
                        {
                            h.Cell().Text("Concepto").Bold();
                            h.Cell().AlignRight().Text("Tipo").Bold();
                            h.Cell().AlignRight().Text($"Valor ({modelo.Moneda})").Bold();
                        });

                        foreach (var linea in modelo.Lineas.OrderBy(l => l.Orden))
                        {
                            tabla.Cell().Text(linea.ConceptoNombre);
                            tabla.Cell().AlignRight().Text(linea.Tipo.ToString());
                            tabla.Cell().AlignRight().Text(linea.ValorMonto.ToString("N2"));
                        }
                    });

                    columna.Item().PaddingTop(10).AlignRight().Column(totales =>
                    {
                        totales.Item().Text($"Devengos: {modelo.SubtotalDevengos:N2} {modelo.Moneda}");
                        totales.Item().Text($"Deducciones: {modelo.SubtotalDeducciones:N2} {modelo.Moneda}");
                        totales.Item().Text($"Neto: {modelo.TotalNeto:N2} {modelo.Moneda}").Bold().FontSize(12);
                    });
                });

                pagina.Footer().AlignCenter().Text(texto =>
                {
                    texto.Span("Generado Nomina System — ");
                    texto.Span("Página ");
                    texto.CurrentPageNumber();
                    texto.Span(" de ");
                    texto.TotalPages();
                });
            });
        }).GeneratePdf(rutaDestino);
    }

    public void ExportarCotizacion(ModeloDocumentoCotizacion modelo, string rutaDestino)
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

        Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(2, Unit.Centimetre);
                pagina.DefaultTextStyle(x => x.FontSize(10));

                pagina.Header().Column(columna =>
                {
                    columna.Item().Text(modelo.EmpresaNombre).Bold().FontSize(16);
                    columna.Item().Text($"NIT {modelo.EmpresaNit}").FontSize(10);
                    if (!string.IsNullOrWhiteSpace(modelo.EmpresaDireccion))
                    {
                        columna.Item().Text(modelo.EmpresaDireccion).FontSize(9);
                    }

                    columna.Item().PaddingTop(8).Text($"Cotización {modelo.NumeroCotizacion}").Bold().FontSize(13);
                    columna.Item().Text($"Emisión: {modelo.FechaEmision:dd/MM/yyyy}  |  Vigencia: {modelo.FechaVigencia:dd/MM/yyyy}  |  Estado: {modelo.Estado}").FontSize(9);
                });

                pagina.Content().PaddingVertical(10).Column(columna =>
                {
                    columna.Item().Text($"Cliente: {modelo.ClienteNombre}").Bold();
                    if (!string.IsNullOrWhiteSpace(modelo.ClienteDocumento))
                    {
                        columna.Item().Text($"Documento: {modelo.ClienteDocumento}").FontSize(9);
                    }

                    columna.Item().PaddingTop(8).Table(tabla =>
                    {
                        tabla.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(4);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                            c.RelativeColumn(2);
                        });

                        tabla.Header(h =>
                        {
                            h.Cell().Text("Descripción").Bold();
                            h.Cell().AlignRight().Text("Cant.").Bold();
                            h.Cell().AlignRight().Text("Precio").Bold();
                            h.Cell().AlignRight().Text($"Subtotal ({modelo.Moneda})").Bold();
                        });

                        foreach (var linea in modelo.Lineas.OrderBy(l => l.Orden))
                        {
                            tabla.Cell().Text(linea.Descripcion);
                            tabla.Cell().AlignRight().Text(linea.Cantidad.ToString("N2"));
                            tabla.Cell().AlignRight().Text(linea.PrecioUnitario.ToString("N2"));
                            tabla.Cell().AlignRight().Text(linea.Subtotal.ToString("N2"));
                        }
                    });

                    columna.Item().PaddingTop(10).AlignRight().Column(totales =>
                    {
                        totales.Item().Text($"Subtotal: {modelo.Subtotal:N2} {modelo.Moneda}");
                        totales.Item().Text($"Total: {modelo.TotalNeto:N2} {modelo.Moneda}").Bold().FontSize(12);
                    });

                    if (!string.IsNullOrWhiteSpace(modelo.Observaciones))
                    {
                        columna.Item().PaddingTop(8).Text($"Observaciones: {modelo.Observaciones}").FontSize(9);
                    }
                });

                pagina.Footer().AlignCenter().Text(texto =>
                {
                    texto.Span("Generado Nomina System — ");
                    texto.Span("Página ");
                    texto.CurrentPageNumber();
                    texto.Span(" de ");
                    texto.TotalPages();
                });
            });
        }).GeneratePdf(rutaDestino);
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
