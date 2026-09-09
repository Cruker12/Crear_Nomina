using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Entities;

public class PlantillaCotizacion : EntityBase
{
    public Guid EmpresaId { get; private set; }

    public string Nombre { get; private set; }

    public ConfiguracionDocumento ConfiguracionDocumento { get; private set; }

    public bool Activo { get; private set; } = true;

    protected PlantillaCotizacion()
    {
        Nombre = string.Empty;
        ConfiguracionDocumento = ConfiguracionDocumento.PorDefecto();
    }

    public PlantillaCotizacion(Guid empresaId, string nombre, ConfiguracionDocumento? configuracion = null)
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
        ConfiguracionDocumento = configuracion ?? ConfiguracionDocumento.PorDefecto();
    }

    public void Desactivar()
    {
        Activo = false;
        MarcarModificacion();
    }
}
