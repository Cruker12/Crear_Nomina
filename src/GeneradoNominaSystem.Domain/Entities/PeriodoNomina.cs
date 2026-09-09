using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;

namespace GeneradoNominaSystem.Domain.Entities;

public class PeriodoNomina : EntityBase
{
    public Guid EmpresaId { get; private set; }

    public string Nombre { get; private set; }

    public TipoPeriodo Tipo { get; private set; }

    public DateTime FechaInicio { get; private set; }

    public DateTime FechaFin { get; private set; }

    public int Anio { get; private set; }

    public int? Mes { get; private set; }

    public bool Activo { get; private set; } = true;

    protected PeriodoNomina()
    {
        Nombre = string.Empty;
    }

    public PeriodoNomina(
        Guid empresaId,
        string nombre,
        TipoPeriodo tipo,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        if (empresaId == Guid.Empty)
        {
            throw new ReglaNegocioException("El periodo debe pertenecer a una empresa.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ReglaNegocioException("El nombre del periodo es obligatorio.");
        }

        if (fechaFin <= fechaInicio)
        {
            throw new ReglaNegocioException("La fecha fin debe ser posterior a la fecha inicio.");
        }

        EmpresaId = empresaId;
        Nombre = nombre.Trim();
        Tipo = tipo;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        Anio = fechaInicio.Year;
        Mes = tipo == TipoPeriodo.Mensual ? fechaInicio.Month : null;
    }

    public void Desactivar()
    {
        Activo = false;
        MarcarModificacion();
    }
}
