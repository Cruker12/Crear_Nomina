using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Application.Interfaces;

public interface IHistorialService
{
    Task<IReadOnlyList<NominaDto>> BuscarNominasAsync(
        Guid empresaId,
        string? texto = null,
        EstadoNomina? estado = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<CotizacionDto>> BuscarCotizacionesAsync(
        Guid empresaId,
        string? texto = null,
        EstadoCotizacion? estado = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<DocumentoDto>> ListarDocumentosAsync(
        Guid empresaId,
        TipoDocumentoSistema? tipo = null,
        FormatoExportacion? formato = null,
        DateTime? desde = null,
        DateTime? hasta = null,
        CancellationToken ct = default);
}
