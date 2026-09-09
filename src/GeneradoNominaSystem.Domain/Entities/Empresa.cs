using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Entities;

public class Empresa : EntityBase
{
    public string RazonSocial { get; private set; }

    public string NombreComercial { get; private set; }

    public string Nit { get; private set; }

    public Direccion Direccion { get; private set; }

    public string? Telefono { get; private set; }

    public string? Email { get; private set; }

    public string? LogoRuta { get; private set; }

    public DatosFiscales DatosFiscales { get; private set; }

    public bool Activo { get; private set; } = true;

    protected Empresa()
    {
        RazonSocial = string.Empty;
        NombreComercial = string.Empty;
        Nit = string.Empty;
        Direccion = null!;
        DatosFiscales = null!;
    }

    public Empresa(
        string razonSocial,
        string nombreComercial,
        string nit,
        Direccion direccion,
        DatosFiscales datosFiscales,
        string? telefono = null,
        string? email = null,
        string? logoRuta = null)
    {
        if (string.IsNullOrWhiteSpace(razonSocial))
        {
            throw new ReglaNegocioException("La razón social es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(nombreComercial))
        {
            throw new ReglaNegocioException("El nombre comercial es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(nit))
        {
            throw new ReglaNegocioException("El NIT es obligatorio.");
        }

        RazonSocial = razonSocial.Trim();
        NombreComercial = nombreComercial.Trim();
        Nit = nit.Trim();
        Direccion = direccion ?? throw new ReglaNegocioException("La dirección es obligatoria.");
        DatosFiscales = datosFiscales ?? throw new ReglaNegocioException("Los datos fiscales son obligatorios.");
        Telefono = telefono?.Trim();
        Email = email?.Trim();
        LogoRuta = logoRuta?.Trim();
    }

    public void ActualizarDatos(string razonSocial, string nombreComercial, string? telefono, string? email)
    {
        if (string.IsNullOrWhiteSpace(razonSocial))
        {
            throw new ReglaNegocioException("La razón social es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(nombreComercial))
        {
            throw new ReglaNegocioException("El nombre comercial es obligatorio.");
        }

        RazonSocial = razonSocial.Trim();
        NombreComercial = nombreComercial.Trim();
        Telefono = telefono?.Trim();
        Email = email?.Trim();
        MarcarModificacion();
    }

    public void ActualizarLogo(string? logoRuta)
    {
        LogoRuta = logoRuta?.Trim();
        MarcarModificacion();
    }

    public void Desactivar()
    {
        Activo = false;
        MarcarModificacion();
    }

    public void Activar()
    {
        Activo = true;
        MarcarModificacion();
    }
}
