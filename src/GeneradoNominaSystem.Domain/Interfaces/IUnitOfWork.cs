namespace GeneradoNominaSystem.Domain.Interfaces;

public interface IUnitOfWork
{
    Task<int> GuardarCambiosAsync(CancellationToken ct = default);
}
