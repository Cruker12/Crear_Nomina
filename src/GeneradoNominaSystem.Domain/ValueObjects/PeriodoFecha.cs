using GeneradoNominaSystem.Domain.Exceptions;

namespace GeneradoNominaSystem.Domain.ValueObjects;

public sealed class PeriodoFecha : ValueObjectBase
{
    public DateTime FechaInicio { get; }

    public DateTime FechaFin { get; }

    public PeriodoFecha(DateTime fechaInicio, DateTime fechaFin)
    {
        if (fechaFin <= fechaInicio)
        {
            throw new ReglaNegocioException("La fecha fin debe ser posterior a la fecha inicio.");
        }

        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
    }

    public int DuracionDias => (FechaFin.Date - FechaInicio.Date).Days + 1;

    public bool Contiene(DateTime fecha) => fecha >= FechaInicio && fecha <= FechaFin;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FechaInicio;
        yield return FechaFin;
    }
}
