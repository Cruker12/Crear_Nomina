using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Application.DTOs;

public sealed class EmpleadoDto
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string EmpresaNombre { get; set; } = string.Empty;

    public TipoDocumentoIdentidad TipoDocumento { get; set; } = TipoDocumentoIdentidad.Cedula;

    public string NumeroDocumento { get; set; } = string.Empty;

    public string Nombres { get; set; } = string.Empty;

    public string Apellidos { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Telefono { get; set; }

    public string? DireccionCompleta { get; set; }

    public string? Ciudad { get; set; }

    public string? Departamento { get; set; }

    public string Pais { get; set; } = "Colombia";

    public string? CodigoPostal { get; set; }

    public DateTime FechaIngreso { get; set; } = DateTime.Today;

    public string Cargo { get; set; } = string.Empty;

    public string? DepartamentoArea { get; set; }

    public TipoContrato TipoContrato { get; set; } = TipoContrato.Indefinido;

    public decimal SalarioBaseMonto { get; set; }

    public string SalarioBaseMoneda { get; set; } = "COP";

    public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;
}
