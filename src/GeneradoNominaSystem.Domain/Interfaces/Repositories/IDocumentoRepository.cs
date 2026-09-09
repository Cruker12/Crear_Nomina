using GeneradoNominaSystem.Domain.Entities;

namespace GeneradoNominaSystem.Domain.Interfaces.Repositories;

public interface IDocumentoRepository : IRepository<Documento>
{
    Task<IReadOnlyList<Documento>> ListarPorReferenciaAsync(Guid referenciaId, CancellationToken ct = default);

    Task<IReadOnlyList<Documento>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);
}
