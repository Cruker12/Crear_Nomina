using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Entities;

public class Nomina : EntityBase
{
    private readonly List<DetalleNomina> _detalles = new();

    public Guid EmpresaId { get; private set; }

    public Guid EmpleadoId { get; private set; }

    public Guid PeriodoNominaId { get; private set; }

    public Guid? PlantillaNominaId { get; private set; }

    public EstadoNomina Estado { get; private set; } = EstadoNomina.Borrador;

    public DateTime? FechaCalculo { get; private set; }

    public DateTime? FechaAprobacion { get; private set; }

    public string? Observaciones { get; private set; }

    public Dinero SubtotalDevengos { get; private set; }

    public Dinero SubtotalDeducciones { get; private set; }

    public Dinero TotalNeto { get; private set; }

    public IReadOnlyList<DetalleNomina> Detalles => _detalles.AsReadOnly();

    protected Nomina()
    {
        SubtotalDevengos = Dinero.Cero();
        SubtotalDeducciones = Dinero.Cero();
        TotalNeto = Dinero.Cero();
    }

    public Nomina(Guid empresaId, Guid empleadoId, Guid periodoNominaId, Guid? plantillaNominaId = null)
    {
        if (empresaId == Guid.Empty)
        {
            throw new ReglaNegocioException("La nómina debe pertenecer a una empresa.");
        }

        if (empleadoId == Guid.Empty)
        {
            throw new ReglaNegocioException("La nómina debe pertenecer a un empleado.");
        }

        if (periodoNominaId == Guid.Empty)
        {
            throw new ReglaNegocioException("La nómina debe pertenecer a un periodo.");
        }

        EmpresaId = empresaId;
        EmpleadoId = empleadoId;
        PeriodoNominaId = periodoNominaId;
        PlantillaNominaId = plantillaNominaId;
        SubtotalDevengos = Dinero.Cero();
        SubtotalDeducciones = Dinero.Cero();
        TotalNeto = Dinero.Cero();
    }

    public void AgregarDetalle(DetalleNomina detalle)
    {
        if (Estado != EstadoNomina.Borrador)
        {
            throw new ReglaNegocioException("Solo se pueden editar nóminas en estado Borrador.");
        }

        if (detalle is null)
        {
            throw new ReglaNegocioException("El detalle es obligatorio.");
        }

        if (_detalles.Any(d => d.ConceptoNominaId == detalle.ConceptoNominaId))
        {
            throw new ReglaNegocioException("El concepto ya está incluido en la nómina.");
        }

        _detalles.Add(detalle);
        MarcarModificacion();
    }

    public void AplicarTotales(Dinero subtotalDevengos, Dinero subtotalDeducciones)
    {
        if (subtotalDevengos is null || subtotalDeducciones is null)
        {
            throw new ReglaNegocioException("Los subtotales son obligatorios.");
        }

        SubtotalDevengos = subtotalDevengos;
        SubtotalDeducciones = subtotalDeducciones;
        TotalNeto = subtotalDevengos.Restar(subtotalDeducciones);
        MarcarModificacion();
    }

    public void MarcarCalculada()
    {
        if (Estado != EstadoNomina.Borrador)
        {
            throw new ReglaNegocioException("Solo una nómina en Borrador puede marcarse como Calculada.");
        }

        Estado = EstadoNomina.Calculada;
        FechaCalculo = DateTime.UtcNow;
        MarcarModificacion();
    }

    public void Aprobar()
    {
        if (Estado != EstadoNomina.Calculada)
        {
            throw new ReglaNegocioException("Solo una nómina Calculada puede aprobarse.");
        }

        Estado = EstadoNomina.Aprobada;
        FechaAprobacion = DateTime.UtcNow;
        MarcarModificacion();
    }

    public void MarcarPagada()
    {
        if (Estado != EstadoNomina.Aprobada)
        {
            throw new ReglaNegocioException("Solo una nómina Aprobada puede marcarse como Pagada.");
        }

        Estado = EstadoNomina.Pagada;
        MarcarModificacion();
    }

    public void Anular(string motivo)
    {
        if (Estado == EstadoNomina.Pagada)
        {
            throw new ReglaNegocioException("No se puede anular una nómina pagada.");
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ReglaNegocioException("Anular requiere un motivo en Observaciones.");
        }

        Estado = EstadoNomina.Anulada;
        Observaciones = motivo.Trim();
        MarcarModificacion();
    }
}
