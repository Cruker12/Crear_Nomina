using GeneradoNominaSystem.Application.DTOs;

namespace GeneradoNominaSystem.Application.Interfaces;

public interface IEmpresaService
{
    Task<IReadOnlyList<EmpresaDto>> ListarAsync(CancellationToken ct = default);

    Task<EmpresaDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<EmpresaDto> CrearAsync(EmpresaDto dto, CancellationToken ct = default);

    Task<EmpresaDto> ActualizarAsync(EmpresaDto dto, CancellationToken ct = default);

    Task DesactivarAsync(Guid id, CancellationToken ct = default);

    Task ActivarAsync(Guid id, CancellationToken ct = default);

    Task EliminarAsync(Guid id, CancellationToken ct = default);
}
