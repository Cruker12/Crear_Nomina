using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Application.DTOs;

public sealed class DocumentoDto
{
    public Guid Id { get; set; }

    public TipoDocumentoSistema Tipo { get; set; }

    public Guid ReferenciaId { get; set; }

    public string NumeroDocumento { get; set; } = string.Empty;

    public FormatoExportacion Formato { get; set; }

    public string RutaArchivo { get; set; } = string.Empty;

    public long TamanoBytes { get; set; }

    public DateTime FechaGeneracion { get; set; }
}
