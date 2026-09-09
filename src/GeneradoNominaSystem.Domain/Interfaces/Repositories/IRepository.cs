using GeneradoNominaSystem.Domain.Entities;

namespace GeneradoNominaSystem.Domain.Interfaces.Repositories;

public interface IRepository<T> where T : EntityBase
{
    Task<T?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<T>> ListarAsync(CancellationToken ct = default);

    Task AgregarAsync(T entidad, CancellationToken ct = default);

    void Actualizar(T entidad);

    void Eliminar(T entidad);
}
