namespace GeneradoNominaSystem.Domain.Interfaces.Services;

/// <summary>
/// Puerto para el diálogo "Guardar como" de la aplicación.
/// Permite al usuario elegir dónde guardar un documento generado.
/// La implementación vive en Presentation (Microsoft.Win32.SaveFileDialog).
/// </summary>
public interface IDialogoGuardarArchivo
{
    string? PedirRutaDestino(
        string titulo,
        string filtro,
        string extensionPorDefecto,
        string nombreSugerido,
        string carpetaInicial);
}
