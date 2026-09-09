using GeneradoNominaSystem.Domain.Exceptions;

namespace GeneradoNominaSystem.Domain.ValueObjects;

public sealed class DatosFiscales : ValueObjectBase
{
    public string Nit { get; }

    public string RazonSocial { get; }

    public string? DigitoVerificacion { get; }

    public string? RegimenTributario { get; }

    public bool ResponsableIva { get; }

    public string? RepresentanteLegal { get; }

    public DatosFiscales(
        string nit,
        string razonSocial,
        string? digitoVerificacion = null,
        string? regimenTributario = null,
        bool responsableIva = false,
        string? representanteLegal = null)
    {
        if (string.IsNullOrWhiteSpace(nit))
        {
            throw new ReglaNegocioException("El NIT es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(razonSocial))
        {
            throw new ReglaNegocioException("La razón social es obligatoria.");
        }

        Nit = nit.Trim();
        RazonSocial = razonSocial.Trim();
        DigitoVerificacion = digitoVerificacion?.Trim();
        RegimenTributario = regimenTributario?.Trim();
        ResponsableIva = responsableIva;
        RepresentanteLegal = representanteLegal?.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Nit;
        yield return RazonSocial;
        yield return DigitoVerificacion;
        yield return RegimenTributario;
        yield return ResponsableIva;
        yield return RepresentanteLegal;
    }
}
