using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Entities;

public class ProductoServicio : EntityBase
{
    public Guid EmpresaId { get; private set; }

    public string Nombre { get; private set; }

    public string? Descripcion { get; private set; }

    public string Unidad { get; private set; }

    public Dinero PrecioUnitario { get; private set; }

    public bool Activo { get; private set; } = true;

    protected ProductoServicio()
    {
        Nombre = string.Empty;
        Unidad = "Unidad";
        PrecioUnitario = Dinero.Cero();
    }

    public ProductoServicio(
        Guid empresaId,
        string nombre,
        Dinero precioUnitario,
        string unidad = "Unidad",
        string? descripcion = null)
    {
        if (empresaId == Guid.Empty)
        {
            throw new ReglaNegocioException("Debe pertenecer a una empresa.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ReglaNegocioException("El nombre es obligatorio.");
        }

        if (precioUnitario is null)
        {
            throw new ReglaNegocioException("El precio unitario es obligatorio.");
        }

        if (precioUnitario.Monto < 0m)
        {
            throw new ReglaNegocioException("El precio unitario no puede ser negativo.");
        }

        EmpresaId = empresaId;
        Nombre = nombre.Trim();
        PrecioUnitario = precioUnitario;
        Unidad = string.IsNullOrWhiteSpace(unidad) ? "Unidad" : unidad.Trim();
        Descripcion = descripcion?.Trim();
    }

    public void Desactivar()
    {
        Activo = false;
        MarcarModificacion();
    }
}
