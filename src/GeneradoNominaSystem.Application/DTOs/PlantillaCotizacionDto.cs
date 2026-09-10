namespace GeneradoNominaSystem.Application.DTOs;

public sealed class PlantillaCotizacionDto
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
