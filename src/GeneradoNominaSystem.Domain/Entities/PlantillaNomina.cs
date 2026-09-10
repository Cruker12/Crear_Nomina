using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Entities;

public class PlantillaNomina : EntityBase
{
    private readonly List<PlantillaConcepto> _conceptos = new();

    public Guid EmpresaId { get; private set; }

    public string Nombre { get; private set; }

    public string? Descripcion { get; private set; }

    public ConfiguracionDocumento ConfiguracionDocumento { get; private set; }

    public bool Activo { get; private set; } = true;

    public IReadOnlyList<PlantillaConcepto> Conceptos => _conceptos.AsReadOnly();

    protected PlantillaNomina()
    {
        Nombre = string.Empty;
        ConfiguracionDocumento = ConfiguracionDocumento.PorDefecto();
    }

    public PlantillaNomina(
        Guid empresaId,
        string nombre,
        ConfiguracionDocumento? configuracion = null,
        string? descripcion = null)
    {
        if (empresaId == Guid.Empty)
        {
            throw new ReglaNegocioException("La plantilla debe pertenecer a una empresa.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ReglaNegocioException("El nombre de la plantilla es obligatorio.");
        }

        EmpresaId = empresaId;
        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim();
        ConfiguracionDocumento = configuracion ?? ConfiguracionDocumento.PorDefecto();
    }

    public void AgregarConcepto(PlantillaConcepto concepto)
    {
        if (concepto is null)
        {
            throw new ReglaNegocioException("El concepto es obligatorio.");
        }

        if (_conceptos.Any(c => c.ConceptoNominaId == concepto.ConceptoNominaId))
        {
            throw new ReglaNegocioException("El concepto ya está incluido en la plantilla.");
        }

        _conceptos.Add(concepto);
        MarcarModificacion();
    }

    public void RemoverConcepto(Guid conceptoNominaId)
    {
        var concepto = _conceptos.FirstOrDefault(c => c.ConceptoNominaId == conceptoNominaId);
        if (concepto is null)
        {
            throw new ReglaNegocioException("El concepto no está incluido en la plantilla.");
        }

        _conceptos.Remove(concepto);
        MarcarModificacion();
    }

    public void Desactivar()
    {
        Activo = false;
        MarcarModificacion();
    }
}
