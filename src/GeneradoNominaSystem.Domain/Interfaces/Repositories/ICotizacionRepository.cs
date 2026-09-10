using GeneradoNominaSystem.Domain.Entities;

namespace GeneradoNominaSystem.Domain.Interfaces.Repositories;

public interface ICotizacionRepository : IRepository<Cotizacion>
{
    Task<Cotizacion?> ObtenerPorNumeroAsync(Guid empresaId, string numeroCotizacion, CancellationToken ct = default);

    Task<IReadOnlyList<Cotizacion>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);

    Task<Cotizacion?> ObtenerConDetallesAsync(Guid id, CancellationToken ct = default);

    Task AgregarDetalleAsync(DetalleCotizacion detalle, CancellationToken ct = default);
}
