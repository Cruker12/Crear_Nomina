using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Domain.Interfaces.Services;
using GeneradoNominaSystem.Presentation.Commands;

namespace GeneradoNominaSystem.Presentation.ViewModels;

public sealed class HistorialViewModel : ViewModelBase
{
    private readonly IHistorialService _historial;
    private readonly IEmpresaService _empresas;

    private ObservableCollection<EmpresaDto> _empresasLista = new();
    private EmpresaDto? _empresaSeleccionada;
    private ObservableCollection<NominaDto> _nominas = new();
    private ObservableCollection<CotizacionDto> _cotizaciones = new();
    private ObservableCollection<DocumentoDto> _documentos = new();
    private DocumentoDto? _documentoSeleccionado;
    private string _texto = string.Empty;
    private string _estadoNomina = "Todos";
    private string _estadoCotizacion = "Todos";
    private string _tipoDocumento = "Todos";
    private string _formatoDocumento = "Todos";
    private DateTime? _desde;
    private DateTime? _hasta;
    private string _mensaje = string.Empty;
    private bool _esError;
    private bool _ocupado;

    public HistorialViewModel(
        IHistorialService historial,
        IEmpresaService empresas,
        INotificadorDocumentos notificador)
    {
        _historial = historial;
        _empresas = empresas;

        BuscarCommand = new RelayCommand(async _ => await BuscarAsync(), _ => !Ocupado && EmpresaSeleccionada is not null);
        AbrirDocumentoCommand = new RelayCommand(
            _ => AbrirDocumento(),
            _ => !Ocupado && DocumentoSeleccionado is not null);

        notificador.DocumentoGenerado += async (_, _) => await RefrescarTrasDocumentoAsync();
    }

    public ObservableCollection<EmpresaDto> EmpresasLista
    {
        get => _empresasLista;
        private set => SetProperty(ref _empresasLista, value);
    }

    public EmpresaDto? EmpresaSeleccionada
    {
        get => _empresaSeleccionada;
        set
        {
            if (SetProperty(ref _empresaSeleccionada, value))
            {
                _ = BuscarAsync();
            }
        }
    }

    public ObservableCollection<NominaDto> Nominas
    {
        get => _nominas;
        private set => SetProperty(ref _nominas, value);
    }

    public ObservableCollection<CotizacionDto> Cotizaciones
    {
        get => _cotizaciones;
        private set => SetProperty(ref _cotizaciones, value);
    }

    public ObservableCollection<DocumentoDto> Documentos
    {
        get => _documentos;
        private set => SetProperty(ref _documentos, value);
    }

    public DocumentoDto? DocumentoSeleccionado
    {
        get => _documentoSeleccionado;
        set => SetProperty(ref _documentoSeleccionado, value);
    }

    public string Texto
    {
        get => _texto;
        set => SetProperty(ref _texto, value);
    }

    public string EstadoNomina
    {
        get => _estadoNomina;
        set => SetProperty(ref _estadoNomina, value);
    }

    public string EstadoCotizacion
    {
        get => _estadoCotizacion;
        set => SetProperty(ref _estadoCotizacion, value);
    }

    public string TipoDocumento
    {
        get => _tipoDocumento;
        set => SetProperty(ref _tipoDocumento, value);
    }

    public string FormatoDocumento
    {
        get => _formatoDocumento;
        set => SetProperty(ref _formatoDocumento, value);
    }

    public DateTime? Desde
    {
        get => _desde;
        set => SetProperty(ref _desde, value);
    }

    public DateTime? Hasta
    {
        get => _hasta;
        set => SetProperty(ref _hasta, value);
    }

    public string Mensaje
    {
        get => _mensaje;
        private set => SetProperty(ref _mensaje, value);
    }

    public bool EsError
    {
        get => _esError;
        private set => SetProperty(ref _esError, value);
    }

    public bool Ocupado
    {
        get => _ocupado;
        private set => SetProperty(ref _ocupado, value);
    }

    public IReadOnlyList<string> EstadosNomina { get; }
        = new List<string> { "Todos" }.Concat(Enum.GetNames<EstadoNomina>()).ToList();

    public IReadOnlyList<string> EstadosCotizacion { get; }
        = new List<string> { "Todos" }.Concat(Enum.GetNames<EstadoCotizacion>()).ToList();

    public IReadOnlyList<string> TiposDocumento { get; } = ["Todos", "Nomina", "Cotizacion"];

    public IReadOnlyList<string> FormatosDocumento { get; } = ["Todos", "Pdf", "Excel"];

    public ICommand BuscarCommand { get; }

    public ICommand AbrirDocumentoCommand { get; }

    public async Task InicializarAsync()
    {
        await EjecutarAsync(async () =>
        {
            var empresas = await _empresas.ListarAsync();
            EmpresasLista = new ObservableCollection<EmpresaDto>(empresas);
            EmpresaSeleccionada = EmpresasLista.FirstOrDefault(e => e.Activo)
                ?? EmpresasLista.FirstOrDefault();
        });
    }

    /// <summary>
    /// Recarga empresas y resultados. Se llama al entrar a la pestaña
    /// y automáticamente cada vez que se genera un documento.
    /// </summary>
    public async Task RecargarAsync() => await InicializarAsync();

    private async Task RefrescarTrasDocumentoAsync()
    {
        if (EmpresaSeleccionada is null)
        {
            return;
        }

        await BuscarAsync();
    }

    private async Task BuscarAsync()
    {
        if (EmpresaSeleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var id = EmpresaSeleccionada.Id;

            EstadoNomina? estadoN = EstadoNomina != "Todos" ? Enum.Parse<EstadoNomina>(EstadoNomina) : null;
            EstadoCotizacion? estadoC = EstadoCotizacion != "Todos" ? Enum.Parse<EstadoCotizacion>(EstadoCotizacion) : null;
            TipoDocumentoSistema? tipo = TipoDocumento != "Todos" ? Enum.Parse<TipoDocumentoSistema>(TipoDocumento) : null;
            FormatoExportacion? formato = FormatoDocumento != "Todos" ? Enum.Parse<FormatoExportacion>(FormatoDocumento) : null;

            Nominas = new ObservableCollection<NominaDto>(
                await _historial.BuscarNominasAsync(id, Texto, estadoN, Desde, Hasta));
            Cotizaciones = new ObservableCollection<CotizacionDto>(
                await _historial.BuscarCotizacionesAsync(id, Texto, estadoC, Desde, Hasta));
            Documentos = new ObservableCollection<DocumentoDto>(
                await _historial.ListarDocumentosAsync(id, tipo, formato, Desde, Hasta));

            Informar($"{Nominas.Count} nómina(s), {Cotizaciones.Count} cotización(es), {Documentos.Count} documento(s).", esError: false);
        });
    }

    private void AbrirDocumento()
    {
        if (DocumentoSeleccionado is null)
        {
            return;
        }

        try
        {
            if (!File.Exists(DocumentoSeleccionado.RutaArchivo))
            {
                Informar("El archivo ya no existe en disco.", esError: true);
                return;
            }

            Process.Start(new ProcessStartInfo(DocumentoSeleccionado.RutaArchivo) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Informar($"No se pudo abrir: {ex.Message}", esError: true);
        }
    }

    private async Task EjecutarAsync(Func<Task> accion)
    {
        try
        {
            Ocupado = true;
            await accion();
        }
        catch (Exception ex)
        {
            Informar(ex.Message, esError: true);
        }
        finally
        {
            Ocupado = false;
        }
    }

    private void Informar(string mensaje, bool esError)
    {
        Mensaje = mensaje;
        EsError = esError;
    }
}
