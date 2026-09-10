namespace GeneradoNominaSystem.Domain.Comun;

/// <summary>
/// Utilidades para nombres de archivo (diálogo de guardar y exportadores).
/// </summary>
public static class NombresArchivos
{
    public static string Sanear(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return "documento";
        }

        foreach (var invalido in Path.GetInvalidFileNameChars())
        {
            nombre = nombre.Replace(invalido, '_');
        }

        var saneado = nombre.Trim();
        return string.IsNullOrWhiteSpace(saneado) ? "documento" : saneado;
    }
}
