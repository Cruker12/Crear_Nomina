using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Application.DTOs;

public sealed class CotizacionDto
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    public string? ClienteDocumento { get; set; }

    public string? ClienteEmail { get; set; }

    public string? ClienteTelefono { get; set; }

    public string NumeroCotizacion { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; } = DateTime.Today;

    public DateTime FechaVigencia { get; set; } = DateTime.Today.AddDays(15);

    public EstadoCotizacion Estado { get; set; } = EstadoCotizacion.Borrador;

    public string? Observaciones { get; set; }

    public decimal Subtotal { get; set; }

    public decimal TotalNeto { get; set; }

    public string Moneda { get; set; } = "COP";

    public List<DetalleCotizacionDto> Detalles { get; set; } = new();
}
