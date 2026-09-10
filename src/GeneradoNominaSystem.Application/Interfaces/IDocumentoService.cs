using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Application.Interfaces;

public interface IDocumentoService
{
    Task<DocumentoDto> ExportarNominaAsync(
        Guid nominaId,
        FormatoExportacion formato,
        string carpetaDestino,
        CancellationToken ct = default);

    Task<IReadOnlyList<DocumentoDto>> ListarPorReferenciaAsync(Guid referenciaId, CancellationToken ct = default);
}
