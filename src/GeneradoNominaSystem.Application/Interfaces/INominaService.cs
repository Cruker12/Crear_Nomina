using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Interfaces;

public interface INominaService
{
    Task<NominaDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<NominaDto>> ListarPorPeriodoAsync(Guid periodoId, CancellationToken ct = default);

    Task<IReadOnlyList<NominaDto>> ListarPorEmpleadoAsync(Guid empleadoId, CancellationToken ct = default);

    Task<NominaDto> CrearAsync(Guid empresaId, Guid empleadoId, Guid periodoId, CancellationToken ct = default);

    Task<NominaDto> CrearDesdePlantillaAsync(
        Guid empresaId,
        Guid empleadoId,
        Guid periodoId,
        Guid plantillaId,
        string moneda = "COP",
        CancellationToken ct = default);

    Task<NominaDto> AgregarDetalleAsync(
        Guid nominaId,
        Guid conceptoId,
        decimal valorMonto,
        string moneda,
        decimal? cantidad = null,
        string? descripcion = null,
        CancellationToken ct = default);

    Task<NominaDto> CalcularAsync(
        Guid nominaId,
        decimal? baseMonto = null,
        string moneda = "COP",
        CancellationToken ct = default);

    Task AprobarAsync(Guid nominaId, CancellationToken ct = default);

    Task MarcarPagadaAsync(Guid nominaId, CancellationToken ct = default);

    Task AnularAsync(Guid nominaId, string motivo, CancellationToken ct = default);
}
