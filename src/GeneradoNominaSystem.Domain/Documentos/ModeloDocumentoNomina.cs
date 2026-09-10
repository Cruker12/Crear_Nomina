using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Domain.Documentos;

public sealed record LineaDocumentoNomina(
    string ConceptoNombre,
    TipoConcepto Tipo,
    decimal? Cantidad,
    decimal ValorMonto,
    string ValorMoneda,
    int Orden);

public sealed record ModeloDocumentoNomina(
    string EmpresaNombre,
    string EmpresaNit,
    string? EmpresaDireccion,
    string? EmpresaTelefono,
    string EmpleadoNombre,
    string EmpleadoDocumento,
    string PeriodoNombre,
    string NumeroDocumento,
    DateTime FechaGeneracion,
    string Estado,
    IReadOnlyList<LineaDocumentoNomina> Lineas,
    decimal SubtotalDevengos,
    decimal SubtotalDeducciones,
    decimal TotalNeto,
    string Moneda);
