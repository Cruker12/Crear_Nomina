using GeneradoNominaSystem.Domain.Enums;

namespace GeneradoNominaSystem.Application.DTOs;

public sealed class PeriodoNominaDto
{
    public Guid Id { get; set; }

    public Guid EmpresaId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public TipoPeriodo Tipo { get; set; } = TipoPeriodo.Mensual;

    public DateTime FechaInicio { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

    public DateTime FechaFin { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1);

    public bool Activo { get; set; } = true;
}
