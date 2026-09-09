using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Entities;

public class DetalleCotizacion : EntityBase
{
    public Guid CotizacionId { get; private set; }

    public Guid? ProductoServicioId { get; private set; }

    public string Descripcion { get; private set; }

    public decimal Cantidad { get; private set; }

    public Dinero PrecioUnitario { get; private set; }

    public decimal? DescuentoPorcentaje { get; private set; }

    public Dinero Subtotal { get; private set; }

    public int Orden { get; private set; }

    protected DetalleCotizacion()
    {
        Descripcion = string.Empty;
        PrecioUnitario = Dinero.Cero();
        Subtotal = Dinero.Cero();
    }

    public DetalleCotizacion(
        Guid cotizacionId,
        string descripcion,
        decimal cantidad,
        Dinero precioUnitario,
        int orden,
        Guid? productoServicioId = null,
        decimal? descuentoPorcentaje = null)
    {
        if (cotizacionId == Guid.Empty)
        {
            throw new ReglaNegocioException("El detalle debe pertenecer a una cotización.");
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new ReglaNegocioException("La descripción es obligatoria.");
        }

        if (cantidad <= 0m)
        {
            throw new ReglaNegocioException("La cantidad debe ser mayor a cero.");
        }

        if (precioUnitario is null)
        {
            throw new ReglaNegocioException("El precio unitario es obligatorio.");
        }

        if (precioUnitario.Monto < 0m)
        {
            throw new ReglaNegocioException("El precio unitario no puede ser negativo.");
        }

        if (descuentoPorcentaje is < 0m or > 100m)
        {
            throw new ReglaNegocioException("El descuento debe estar entre 0 y 100.");
        }

        CotizacionId = cotizacionId;
        Descripcion = descripcion.Trim();
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
        Orden = orden;
        ProductoServicioId = productoServicioId;
        DescuentoPorcentaje = descuentoPorcentaje;
        Subtotal = CalcularSubtotal();
    }

    private Dinero CalcularSubtotal()
    {
        var bruto = PrecioUnitario.MultiplicarPor(Cantidad);
        if (DescuentoPorcentaje is null or 0m)
        {
            return bruto;
        }

        var factor = 1m - (DescuentoPorcentaje.Value / 100m);
        return bruto.MultiplicarPor(factor);
    }
}
