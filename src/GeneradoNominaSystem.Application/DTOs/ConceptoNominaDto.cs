using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Application.DTOs;

public sealed class ConceptoNominaDto
{
    public Guid Id { get; set; }

    public Guid? EmpresaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public TipoConcepto Tipo { get; set; } = TipoConcepto.Devengo;

    public string? Subtipo { get; set; }

    public bool EsPorcentaje { get; set; }

    public decimal? PorcentajeBase { get; set; }

    public decimal? ValorFijoMonto { get; set; }

    public string ValorFijoMoneda { get; set; } = "COP";

    public int Orden { get; set; }

    public bool Activo { get; set; } = true;
}
