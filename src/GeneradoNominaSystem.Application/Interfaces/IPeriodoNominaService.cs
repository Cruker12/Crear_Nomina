using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Interfaces;

public interface IPeriodoNominaService
{
    Task<IReadOnlyList<PeriodoNominaDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);

    Task<PeriodoNominaDto> CrearAsync(PeriodoNominaDto dto, CancellationToken ct = default);

    Task DesactivarAsync(Guid id, CancellationToken ct = default);

    Task ActivarAsync(Guid id, CancellationToken ct = default);

    Task EliminarAsync(Guid id, CancellationToken ct = default);
}
