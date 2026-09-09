namespace GeneradoNominaSystem.Application.DTOs;

public sealed class EmpresaDto
{
    public Guid Id { get; set; }

    public string RazonSocial { get; set; } = string.Empty;

    public string NombreComercial { get; set; } = string.Empty;

    public string Nit { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? LogoRuta { get; set; }

    public string DireccionCompleta { get; set; } = string.Empty;

    public string Ciudad { get; set; } = string.Empty;

    public string Departamento { get; set; } = string.Empty;

    public string Pais { get; set; } = "Colombia";

    public string? CodigoPostal { get; set; }

    public string? DigitoVerificacion { get; set; }

    public string? RegimenTributario { get; set; }

    public bool ResponsableIva { get; set; }

    public string? RepresentanteLegal { get; set; }

    public bool Activo { get; set; } = true;
}
