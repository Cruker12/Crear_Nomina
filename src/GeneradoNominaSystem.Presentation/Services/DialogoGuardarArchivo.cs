using GeneradoNominaSystem.Domain.Interfaces.Services;

namespace GeneradoNominaSystem.Presentation.Services;

public sealed class DialogoGuardarArchivo : IDialogoGuardarArchivo
{
    public string? PedirRutaDestino(
        string titulo,
        string filtro,
        string extensionPorDefecto,
        string nombreSugerido,
        string carpetaInicial)
    {
        var dialogo = new Microsoft.Win32.SaveFileDialog
        {
            Title = titulo,
            Filter = filtro,
            DefaultExt = extensionPorDefecto,
            AddExtension = true,
            OverwritePrompt = true,
            FileName = nombreSugerido,
            InitialDirectory = carpetaInicial,
        };

        return dialogo.ShowDialog() == true ? dialogo.FileName : null;
    }
}
