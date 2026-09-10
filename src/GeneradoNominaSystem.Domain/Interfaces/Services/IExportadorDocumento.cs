using GeneradoNominaSystem.Domain.Documentos;
using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Domain.Interfaces.Services;

public interface IExportadorDocumento
{
    FormatoExportacion Formato { get; }

    void ExportarNomina(ModeloDocumentoNomina modelo, string rutaDestino);

    void ExportarCotizacion(ModeloDocumentoCotizacion modelo, string rutaDestino);
}
