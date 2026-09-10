namespace GeneradoNominaSystem.Application.DTOs;

public sealed class PlantillaNominaDto
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;

    public List<PlantillaConceptoDto> Conceptos { get; set; } = new();
}
