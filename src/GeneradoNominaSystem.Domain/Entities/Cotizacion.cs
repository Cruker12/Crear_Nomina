using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;
using GeneradoNominaSystem.Domain.ValueObjects;

namespace GeneradoNominaSystem.Domain.Entities;

public class Cotizacion : EntityBase
{
    private readonly List<DetalleCotizacion> _detalles = new();

    public Guid EmpresaId { get; private set; }

    public string ClienteNombre { get; private set; }

    public string? ClienteDocumento { get; private set; }

    public string? ClienteEmail { get; private set; }

    public string? ClienteTelefono { get; private set; }

    public string NumeroCotizacion { get; private set; }

    public DateTime FechaEmision { get; private set; }

    public DateTime FechaVigencia { get; private set; }

    public EstadoCotizacion Estado { get; private set; } = EstadoCotizacion.Borrador;

    public string? Observaciones { get; private set; }

    public Dinero Subtotal { get; private set; }

    public Dinero TotalDescuentos { get; private set; }

    public Dinero TotalNeto { get; private set; }

    public string Moneda { get; private set; } = Dinero.MonedaPorDefecto;

    public Guid? PlantillaCotizacionId { get; private set; }

    public IReadOnlyList<DetalleCotizacion> Detalles => _detalles.AsReadOnly();

    protected Cotizacion()
    {
        ClienteNombre = string.Empty;
        NumeroCotizacion = string.Empty;
        Subtotal = Dinero.Cero();
        TotalDescuentos = Dinero.Cero();
        TotalNeto = Dinero.Cero();
    }

    public Cotizacion(
        Guid empresaId,
        string clienteNombre,
        string numeroCotizacion,
        DateTime fechaEmision,
        DateTime fechaVigencia,
        string moneda = "COP",
        Guid? plantillaCotizacionId = null)
    {
        if (empresaId == Guid.Empty)
        {
            throw new ReglaNegocioException("La cotización debe pertenecer a una empresa.");
        }

        if (string.IsNullOrWhiteSpace(clienteNombre))
        {
            throw new ReglaNegocioException("El nombre del cliente es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(numeroCotizacion))
        {
            throw new ReglaNegocioException("El número de cotización es obligatorio.");
        }

        if (fechaVigencia < fechaEmision)
        {
            throw new ReglaNegocioException("La vigencia no puede ser anterior a la emisión.");
        }

        if (string.IsNullOrWhiteSpace(moneda) || moneda.Trim().Length != 3)
        {
            throw new ReglaNegocioException("La moneda debe ser un código ISO de 3 letras.");
        }

        EmpresaId = empresaId;
        ClienteNombre = clienteNombre.Trim();
        NumeroCotizacion = numeroCotizacion.Trim();
        FechaEmision = fechaEmision;
        FechaVigencia = fechaVigencia;
        Moneda = moneda.Trim().ToUpperInvariant();
        PlantillaCotizacionId = plantillaCotizacionId;
        Subtotal = Dinero.Cero(Moneda);
        TotalDescuentos = Dinero.Cero(Moneda);
        TotalNeto = Dinero.Cero(Moneda);
    }

    public void AgregarDetalle(DetalleCotizacion detalle)
    {
        if (Estado != EstadoCotizacion.Borrador)
        {
            throw new ReglaNegocioException("Solo se pueden editar cotizaciones en Borrador.");
        }

        if (detalle is null)
        {
            throw new ReglaNegocioException("El detalle es obligatorio.");
        }

        if (!string.Equals(detalle.PrecioUnitario.Moneda, Moneda, StringComparison.OrdinalIgnoreCase))
        {
            throw new ReglaNegocioException("La moneda del detalle debe coincidir con la cotización.");
        }

        _detalles.Add(detalle);
        RecalcularTotales();
        MarcarModificacion();
    }

    public void RemoverDetalle(Guid detalleId)
    {
        if (Estado != EstadoCotizacion.Borrador)
        {
            throw new ReglaNegocioException("Solo se pueden editar cotizaciones en Borrador.");
        }

        var detalle = _detalles.FirstOrDefault(d => d.Id == detalleId);
        if (detalle is null)
        {
            throw new ReglaNegocioException("El detalle no existe en la cotización.");
        }

        _detalles.Remove(detalle);
        RecalcularTotales();
        MarcarModificacion();
    }

    public void CambiarEstado(EstadoCotizacion nuevoEstado)
    {
        Estado = nuevoEstado;
        MarcarModificacion();
    }

    public void AsignarPlantilla(Guid? plantillaId)
    {
        PlantillaCotizacionId = plantillaId;
        MarcarModificacion();
    }

    public void ActualizarDatosCliente(string? documento, string? email, string? telefono, string? observaciones)
    {
        ClienteDocumento = documento?.Trim();
        ClienteEmail = email?.Trim();
        ClienteTelefono = telefono?.Trim();
        Observaciones = observaciones?.Trim();
        MarcarModificacion();
    }

    private void RecalcularTotales()
    {
        var subtotal = Dinero.Cero(Moneda);
        foreach (var d in _detalles)
        {
            subtotal = subtotal.Sumar(d.Subtotal);
        }

        Subtotal = subtotal;
        TotalDescuentos = Dinero.Cero(Moneda);
        TotalNeto = subtotal;
    }
}
