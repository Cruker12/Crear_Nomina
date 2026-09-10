namespace GeneradoNominaSystem.Application.DTOs;

public sealed class DetalleCotizacionDto
{
    public Guid Id { get; set; }

    public Guid? ProductoServicioId { get; set; }

    public string ProductoNombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitarioMonto { get; set; }

    public string PrecioUnitarioMoneda { get; set; } = "COP";

    public decimal? DescuentoPorcentaje { get; set; }

    public decimal Subtotal { get; set; }

    public int Orden { get; set; }
}
