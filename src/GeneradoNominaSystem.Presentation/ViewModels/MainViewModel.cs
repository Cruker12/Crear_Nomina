using Microsoft.Extensions.Configuration;

namespace GeneradoNominaSystem.Presentation.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private string _titulo = "Generado Nomina System";

    public MainViewModel(IConfiguration configuration)
    {
        var nombre = configuration["App:Name"];
        var version = configuration["App:Version"];

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            _titulo = string.IsNullOrWhiteSpace(version) ? nombre : $"{nombre} v{version}";
        }
    }

    public string Titulo
    {
        get => _titulo;
        set => SetProperty(ref _titulo, value);
    }
}
