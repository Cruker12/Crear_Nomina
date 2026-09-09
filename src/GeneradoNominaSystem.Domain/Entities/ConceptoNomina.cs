using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Entities;

public class ConceptoNomina : EntityBase
{
    public Guid? EmpresaId { get; private set; }

    public string Nombre { get; private set; }

    public TipoConcepto Tipo { get; private set; }

    public string? Subtipo { get; private set; }

    public bool EsPorcentaje { get; private set; }

    public decimal? PorcentajeBase { get; private set; }

    public Dinero? ValorFijo { get; private set; }

    public string? FormulaCalculo { get; private set; }

    public int Orden { get; private set; }

    public bool RequiereBase { get; private set; }

    public bool AfectaBase { get; private set; }

    public bool Activo { get; private set; } = true;

    protected ConceptoNomina()
    {
        Nombre = string.Empty;
    }

    public ConceptoNomina(
        string nombre,
        TipoConcepto tipo,
        int orden,
        Guid? empresaId = null,
        string? subtipo = null,
        bool esPorcentaje = false,
        decimal? porcentajeBase = null,
        Dinero? valorFijo = null,
        string? formulaCalculo = null,
        bool requiereBase = false,
        bool afectaBase = false)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ReglaNegocioException("El nombre del concepto es obligatorio.");
        }

        if (orden < 0)
        {
            throw new ReglaNegocioException("El orden no puede ser negativo.");
        }

        if (esPorcentaje)
        {
            if (porcentajeBase is null || porcentajeBase is < 0m or > 100m)
            {
                throw new ReglaNegocioException("El porcentaje base debe estar entre 0 y 100.");
            }
        }
        else if (valorFijo is null && !requiereBase)
        {
            throw new ReglaNegocioException("El concepto debe tener valor fijo o requerir base de cálculo.");
        }

        Nombre = nombre.Trim();
        Tipo = tipo;
        Orden = orden;
        EmpresaId = empresaId;
        Subtipo = subtipo?.Trim();
        EsPorcentaje = esPorcentaje;
        PorcentajeBase = porcentajeBase;
        ValorFijo = valorFijo;
        FormulaCalculo = formulaCalculo?.Trim();
        RequiereBase = requiereBase;
        AfectaBase = afectaBase;
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
