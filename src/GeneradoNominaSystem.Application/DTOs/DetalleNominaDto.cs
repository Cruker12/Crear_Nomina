using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Application.DTOs;

public sealed class DetalleNominaDto
{
    public Guid Id { get; set; }

    public Guid ConceptoNominaId { get; set; }

    public string ConceptoNombre { get; set; } = string.Empty;

    public TipoConcepto Tipo { get; set; }

    public decimal ValorMonto { get; set; }

    public string ValorMoneda { get; set; } = "COP";

    public decimal? Cantidad { get; set; }

    public string? Descripcion { get; set; }

    public int Orden { get; set; }
}
