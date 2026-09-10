using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Application.DTOs;

public sealed class NominaDto
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public Guid EmpleadoId { get; set; }

    public string EmpleadoNombre { get; set; } = string.Empty;

    public Guid PeriodoNominaId { get; set; }

    public string PeriodoNombre { get; set; } = string.Empty;

    public EstadoNomina Estado { get; set; } = EstadoNomina.Borrador;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaCalculo { get; set; }

    public string? Observaciones { get; set; }

    public decimal SubtotalDevengos { get; set; }

    public decimal SubtotalDeducciones { get; set; }

    public decimal TotalNeto { get; set; }

    public string Moneda { get; set; } = "COP";

    public List<DetalleNominaDto> Detalles { get; set; } = new();
}
