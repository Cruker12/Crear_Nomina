using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Exceptions;

namespace GeneradoNominaSystem.Domain.ValueObjects;

public sealed class ConfiguracionDocumento : ValueObjectBase
{
    public bool MostrarLogo { get; }

    public PosicionLogo LogoPosicion { get; }

    public string? EncabezadoPersonalizado { get; }

    public string? PiePaginaPersonalizado { get; }

    public bool MostrarFirma { get; }

    public string? TextoFirma { get; }

    public bool NumeracionAutomatica { get; }

    public string? PrefijoNumeracion { get; }

    public int SiguienteNumero { get; }

    public string FormatoFecha { get; }

    public string SimboloMoneda { get; }

    public int DecimalesMoneda { get; }

    public ConfiguracionDocumento(
        bool mostrarLogo = true,
        PosicionLogo logoPosicion = PosicionLogo.Izquierda,
        string? encabezadoPersonalizado = null,
        string? piePaginaPersonalizado = null,
        bool mostrarFirma = false,
        string? textoFirma = null,
        bool numeracionAutomatica = true,
        string? prefijoNumeracion = null,
        int siguienteNumero = 1,
        string formatoFecha = "dd/MM/yyyy",
        string simboloMoneda = "$",
        int decimalesMoneda = 2)
    {
        if (siguienteNumero < 1)
        {
            throw new ReglaNegocioException("El siguiente número debe ser mayor o igual a 1.");
        }

        if (decimalesMoneda is < 0 or > 4)
        {
            throw new ReglaNegocioException("Los decimales de moneda deben estar entre 0 y 4.");
        }

        if (string.IsNullOrWhiteSpace(formatoFecha))
        {
            throw new ReglaNegocioException("El formato de fecha es obligatorio.");
        }

        MostrarLogo = mostrarLogo;
        LogoPosicion = logoPosicion;
        EncabezadoPersonalizado = encabezadoPersonalizado?.Trim();
        PiePaginaPersonalizado = piePaginaPersonalizado?.Trim();
        MostrarFirma = mostrarFirma;
        TextoFirma = textoFirma?.Trim();
        NumeracionAutomatica = numeracionAutomatica;
        PrefijoNumeracion = prefijoNumeracion?.Trim();
        SiguienteNumero = siguienteNumero;
        FormatoFecha = formatoFecha.Trim();
        SimboloMoneda = string.IsNullOrWhiteSpace(simboloMoneda) ? "$" : simboloMoneda.Trim();
        DecimalesMoneda = decimalesMoneda;
    }

    public static ConfiguracionDocumento PorDefecto() => new();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MostrarLogo;
        yield return LogoPosicion;
        yield return EncabezadoPersonalizado;
        yield return PiePaginaPersonalizado;
        yield return MostrarFirma;
        yield return TextoFirma;
        yield return NumeracionAutomatica;
        yield return PrefijoNumeracion;
        yield return SiguienteNumero;
        yield return FormatoFecha;
        yield return SimboloMoneda;
        yield return DecimalesMoneda;
    }
}
