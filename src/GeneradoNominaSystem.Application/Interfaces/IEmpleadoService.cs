using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Application.Interfaces;

public interface IEmpleadoService
{
    Task<IReadOnlyList<EmpleadoDto>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);

    Task<EmpleadoDto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<EmpleadoDto> CrearAsync(EmpleadoDto dto, CancellationToken ct = default);

    Task<EmpleadoDto> ActualizarAsync(EmpleadoDto dto, CancellationToken ct = default);

    Task CambiarEstadoAsync(Guid id, EstadoEmpleado nuevoEstado, CancellationToken ct = default);

    Task EliminarAsync(Guid id, CancellationToken ct = default);
}
