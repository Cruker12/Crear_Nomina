namespace GeneradoNominaSystem.Application.DTOs;

public sealed class PlantillaConceptoDto
{
    public Guid ConceptoNominaId { get; set; }

    public string ConceptoNombre { get; set; } = string.Empty;

    public int Orden { get; set; }

    public bool Obligatorio { get; set; }

    public decimal? ValorPorDefectoMonto { get; set; }

    public string ValorPorDefectoMoneda { get; set; } = "COP";
}
