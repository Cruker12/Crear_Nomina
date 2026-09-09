using GeneradoNominaSystem.Domain.Exceptions;

namespace GeneradoNominaSystem.Domain.ValueObjects;

public sealed class Direccion : ValueObjectBase
{
    public string DireccionCompleta { get; }

    public string Ciudad { get; }

    public string Departamento { get; }

    public string Pais { get; }

    public string? CodigoPostal { get; }

    public Direccion(
        string direccionCompleta,
        string ciudad,
        string departamento,
        string pais = "Colombia",
        string? codigoPostal = null)
    {
        if (string.IsNullOrWhiteSpace(direccionCompleta))
        {
            throw new ReglaNegocioException("La dirección es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(ciudad))
        {
            throw new ReglaNegocioException("La ciudad es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(departamento))
        {
            throw new ReglaNegocioException("El departamento es obligatorio.");
        }

        DireccionCompleta = direccionCompleta.Trim();
        Ciudad = ciudad.Trim();
        Departamento = departamento.Trim();
        Pais = string.IsNullOrWhiteSpace(pais) ? "Colombia" : pais.Trim();
        CodigoPostal = codigoPostal?.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DireccionCompleta;
        yield return Ciudad;
        yield return Departamento;
        yield return Pais;
        yield return CodigoPostal;
    }
}
