namespace GeneradoNominaSystem.Domain.Comun;

/// <summary>
/// Marca de "dato no conocido": el usuario puede escribir NN en cualquier campo
/// de texto no relevante (nombres, direcciones, emails, observaciones...) para
/// poder continuar cuando no tiene el dato. No aplica a claves del negocio
/// (NIT, documentos de identidad) ni a montos, monedas o fechas.
/// </summary>
public static class DatoDesconocido
{
    public const string Valor = "NN";

    public static bool EsDesconocido(string? valor)
        => string.Equals(valor?.Trim(), Valor, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Normaliza variantes (nn, nN, " NN ") al valor canónico "NN".
    /// Devuelve el valor original si no es un desconocido.
    /// </summary>
    public static string? Normalizar(string? valor)
        => EsDesconocido(valor) ? Valor : valor;
}
