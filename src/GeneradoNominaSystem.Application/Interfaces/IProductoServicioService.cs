using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Interfaces;

public interface IProductoServicioService
{
    Task<IReadOnlyList<ProductoServicioDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);

    Task<ProductoServicioDto> CrearAsync(ProductoServicioDto dto, CancellationToken ct = default);

    Task DesactivarAsync(Guid id, CancellationToken ct = default);

    Task ActivarAsync(Guid id, CancellationToken ct = default);

    Task EliminarAsync(Guid id, CancellationToken ct = default);
}
