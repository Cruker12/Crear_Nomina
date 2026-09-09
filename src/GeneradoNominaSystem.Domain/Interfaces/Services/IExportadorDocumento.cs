using GeneradoNominaSystem.Domain.Entities;
using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Domain.Interfaces.Services;

public interface IExportadorDocumento
{
    FormatoExportacion Formato { get; }

    void ExportarNomina(
        Nomina nomina,
        IReadOnlyList<(ConceptoNomina Concepto, DetalleNomina Detalle)> lineas,
        string rutaDestino);

    void ExportarCotizacion(Cotizacion cotizacion, string rutaDestino);
}
