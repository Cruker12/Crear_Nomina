namespace GeneradoNominaSystem.Domain.Documentos;

public sealed record LineaDocumentoCotizacion(
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal? DescuentoPorcentaje,
    decimal Subtotal,
    int Orden);

public sealed record ModeloDocumentoCotizacion(
    string EmpresaNombre,
    string EmpresaNit,
    string? EmpresaDireccion,
    string? EmpresaTelefono,
    string ClienteNombre,
    string? ClienteDocumento,
    string NumeroCotizacion,
    DateTime FechaEmision,
    DateTime FechaVigencia,
    string Estado,
    IReadOnlyList<LineaDocumentoCotizacion> Lineas,
    decimal Subtotal,
    decimal TotalNeto,
    string Moneda,
    string? Observaciones);
