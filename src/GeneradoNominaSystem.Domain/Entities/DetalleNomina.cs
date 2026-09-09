using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Entities;

public class DetalleNomina : EntityBase
{
    public Guid NominaId { get; private set; }

    public Guid ConceptoNominaId { get; private set; }

    public Dinero Valor { get; private set; }

    public decimal? Cantidad { get; private set; }

    public string? Descripcion { get; private set; }

    public int Orden { get; private set; }

    protected DetalleNomina()
    {
        Valor = Dinero.Cero();
    }

    public DetalleNomina(
        Guid nominaId,
        Guid conceptoNominaId,
        Dinero valor,
        int orden,
        decimal? cantidad = null,
        string? descripcion = null)
    {
        if (nominaId == Guid.Empty)
        {
            throw new ReglaNegocioException("El detalle debe pertenecer a una nómina.");
        }

        if (conceptoNominaId == Guid.Empty)
        {
            throw new ReglaNegocioException("El detalle debe referenciar un concepto.");
        }

        if (valor is null)
        {
            throw new ReglaNegocioException("El valor del detalle es obligatorio.");
        }

        if (valor.Monto < 0m)
        {
            throw new ReglaNegocioException("El valor del detalle no puede ser negativo. El signo lo define el tipo de concepto.");
        }

        if (cantidad is < 0m)
        {
            throw new ReglaNegocioException("La cantidad no puede ser negativa.");
        }

        NominaId = nominaId;
        ConceptoNominaId = conceptoNominaId;
        Valor = valor;
        Orden = orden;
        Cantidad = cantidad;
        Descripcion = descripcion?.Trim();
    }
}
