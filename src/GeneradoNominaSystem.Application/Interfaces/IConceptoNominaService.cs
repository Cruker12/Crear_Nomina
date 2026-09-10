using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Interfaces;

public interface IConceptoNominaService
{
    Task<IReadOnlyList<ConceptoNominaDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);

    Task<ConceptoNominaDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<ConceptoNominaDto> CrearAsync(ConceptoNominaDto dto, CancellationToken ct = default);

    Task DesactivarAsync(Guid id, CancellationToken ct = default);

    Task ActivarAsync(Guid id, CancellationToken ct = default);
}
