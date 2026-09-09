using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Entities;

public class PlantillaConcepto : EntityBase
{
    public Guid PlantillaNominaId { get; private set; }

    public Guid ConceptoNominaId { get; private set; }

    public int Orden { get; private set; }

    public bool Obligatorio { get; private set; }

    public Dinero? ValorPorDefecto { get; private set; }

    protected PlantillaConcepto()
    {
    }

    public PlantillaConcepto(
        Guid plantillaNominaId,
        Guid conceptoNominaId,
        int orden,
        bool obligatorio = false,
        Dinero? valorPorDefecto = null)
    {
        if (plantillaNominaId == Guid.Empty)
        {
            throw new ReglaNegocioException("Debe pertenecer a una plantilla.");
        }

        if (conceptoNominaId == Guid.Empty)
        {
            throw new ReglaNegocioException("Debe referenciar un concepto.");
        }

        if (orden < 0)
        {
            throw new ReglaNegocioException("El orden no puede ser negativo.");
        }

        PlantillaNominaId = plantillaNominaId;
        ConceptoNominaId = conceptoNominaId;
        Orden = orden;
        Obligatorio = obligatorio;
        ValorPorDefecto = valorPorDefecto;
    }
}
