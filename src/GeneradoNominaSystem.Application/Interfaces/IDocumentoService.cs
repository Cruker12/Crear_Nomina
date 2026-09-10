using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Application.Interfaces;

public interface IDocumentoService
{
    Task<DocumentoDto> ExportarNominaAsync(
        Guid nominaId,
        FormatoExportacion formato,
        string rutaDestino,
        CancellationToken ct = default);

    Task<DocumentoDto> ExportarCotizacionAsync(
        Guid cotizacionId,
        FormatoExportacion formato,
        string rutaDestino,
        CancellationToken ct = default);

    Task<IReadOnlyList<DocumentoDto>> ListarPorReferenciaAsync(Guid referenciaId, CancellationToken ct = default);

    Task<IReadOnlyList<DocumentoDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);
}
