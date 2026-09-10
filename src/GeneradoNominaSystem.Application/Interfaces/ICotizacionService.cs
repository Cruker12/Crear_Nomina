using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Application.Interfaces;

public interface ICotizacionService
{
    Task<CotizacionDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<CotizacionDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);

    Task<CotizacionDto> CrearAsync(CotizacionDto dto, CancellationToken ct = default);

    Task<CotizacionDto> AgregarDetalleAsync(
        Guid cotizacionId,
        string descripcion,
        decimal cantidad,
        decimal precioUnitarioMonto,
        string moneda,
        Guid? productoServicioId = null,
        decimal? descuentoPorcentaje = null,
        CancellationToken ct = default);

    Task QuitarDetalleAsync(Guid cotizacionId, Guid detalleId, CancellationToken ct = default);

    Task CambiarEstadoAsync(Guid cotizacionId, EstadoCotizacion nuevoEstado, CancellationToken ct = default);
}
