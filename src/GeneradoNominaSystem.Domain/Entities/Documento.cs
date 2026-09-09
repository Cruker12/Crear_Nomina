using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;

namespace GeneradoNominaSystem.Domain.Entities;

public class Documento : EntityBase
{
    public Guid EmpresaId { get; private set; }

    public TipoDocumentoSistema Tipo { get; private set; }

    public Guid ReferenciaId { get; private set; }

    public string NumeroDocumento { get; private set; }

    public FormatoExportacion Formato { get; private set; }

    public string RutaArchivo { get; private set; }

    public long TamanoBytes { get; private set; }

    public DateTime FechaGeneracion { get; private set; } = DateTime.UtcNow;

    public string? GeneradoPor { get; private set; }

    protected Documento()
    {
        NumeroDocumento = string.Empty;
        RutaArchivo = string.Empty;
    }

    public Documento(
        Guid empresaId,
        TipoDocumentoSistema tipo,
        Guid referenciaId,
        string numeroDocumento,
        FormatoExportacion formato,
        string rutaArchivo,
        long tamanoBytes,
        string? generadoPor = null)
    {
        if (empresaId == Guid.Empty)
        {
            throw new ReglaNegocioException("El documento debe pertenecer a una empresa.");
        }

        if (referenciaId == Guid.Empty)
        {
            throw new ReglaNegocioException("El documento debe referenciar una nómina o cotización.");
        }

        if (string.IsNullOrWhiteSpace(numeroDocumento))
        {
            throw new ReglaNegocioException("El número de documento es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(rutaArchivo))
        {
            throw new ReglaNegocioException("La ruta del archivo es obligatoria.");
        }

        if (tamanoBytes < 0)
        {
            throw new ReglaNegocioException("El tamaño no puede ser negativo.");
        }

        EmpresaId = empresaId;
        Tipo = tipo;
        ReferenciaId = referenciaId;
        NumeroDocumento = numeroDocumento.Trim();
        Formato = formato;
        RutaArchivo = rutaArchivo.Trim();
        TamanoBytes = tamanoBytes;
        GeneradoPor = generadoPor?.Trim();
    }
}
