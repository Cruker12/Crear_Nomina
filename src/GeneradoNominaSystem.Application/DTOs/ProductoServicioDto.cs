namespace GeneradoNominaSystem.Application.DTOs;

public sealed class ProductoServicioDto
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public string Unidad { get; set; } = "Unidad";

    public decimal PrecioUnitarioMonto { get; set; }

    public string PrecioUnitarioMoneda { get; set; } = "COP";

    public bool Activo { get; set; } = true;
}
