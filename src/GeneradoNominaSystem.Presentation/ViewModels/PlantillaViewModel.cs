using System.Collections.ObjectModel;
using System.Windows.Input;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Presentation.Commands;

namespace GeneradoNominaSystem.Presentation.ViewModels;

public sealed class PlantillaViewModel : ViewModelBase
{
    private readonly IPlantillaNominaService _plantillas;
    private readonly IEmpresaService _empresas;
    private readonly IConceptoNominaService _conceptos;

    private ObservableCollection<EmpresaDto> _empresasLista = new();
    private EmpresaDto? _empresaSeleccionada;
    private ObservableCollection<PlantillaNominaDto> _plantillasLista = new();
    private PlantillaNominaDto? _seleccionada;
    private PlantillaNominaDto _edicion = new();
    private ObservableCollection<ConceptoNominaDto> _conceptosLista = new();
    private ConceptoNominaDto? _conceptoSeleccionado;
    private PlantillaConceptoDto? _conceptoEnPlantillaSeleccionado;
    private int _nuevoOrden;
    private bool _nuevoObligatorio;
    private decimal? _nuevoValorDefecto;
    private string _mensaje = string.Empty;
    private bool _esError;
    private bool _ocupado;

    public PlantillaViewModel(
        IPlantillaNominaService plantillas,
        IEmpresaService empresas,
        IConceptoNominaService conceptos)
    {
        _plantillas = plantillas;
        _empresas = empresas;
        _conceptos = conceptos;

        RecargarCommand = new RelayCommand(async _ => await RecargarAsync(), _ => !Ocupado);
        GuardarCommand = new RelayCommand(async _ => await GuardarAsync(), _ => !Ocupado);
        NuevaCommand = new RelayCommand(_ => Nueva(), _ => !Ocupado);
        AgregarConceptoCommand = new RelayCommand(
            async _ => await AgregarConceptoAsync(),
            _ => !Ocupado && Seleccionada is not null && ConceptoSeleccionado is not null);
        QuitarConceptoCommand = new RelayCommand(
            async _ => await QuitarConceptoAsync(),
            _ => !Ocupado && Seleccionada is not null);
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
                _ = RecargarTodoAsync();
            }
        }
    }

    public ObservableCollection<PlantillaNominaDto> PlantillasLista
    {
        get => _plantillasLista;
        private set => SetProperty(ref _plantillasLista, value);
    }

    public PlantillaNominaDto? Seleccionada
    {
        get => _seleccionada;
        set => SetProperty(ref _seleccionada, value);
    }

    public PlantillaNominaDto Edicion
    {
        get => _edicion;
        set => SetProperty(ref _edicion, value);
    }

    public ObservableCollection<ConceptoNominaDto> ConceptosLista
    {
        get => _conceptosLista;
        private set => SetProperty(ref _conceptosLista, value);
    }

    public ConceptoNominaDto? ConceptoSeleccionado
    {
        get => _conceptoSeleccionado;
        set => SetProperty(ref _conceptoSeleccionado, value);
    }

    public PlantillaConceptoDto? ConceptoEnPlantillaSeleccionado
    {
        get => _conceptoEnPlantillaSeleccionado;
        set => SetProperty(ref _conceptoEnPlantillaSeleccionado, value);
    }

    public int NuevoOrden
    {
        get => _nuevoOrden;
        set => SetProperty(ref _nuevoOrden, value);
    }

    public bool NuevoObligatorio
    {
        get => _nuevoObligatorio;
        set => SetProperty(ref _nuevoObligatorio, value);
    }

    public decimal? NuevoValorDefecto
    {
        get => _nuevoValorDefecto;
        set => SetProperty(ref _nuevoValorDefecto, value);
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

    public ICommand RecargarCommand { get; }

    public ICommand GuardarCommand { get; }

    public ICommand NuevaCommand { get; }

    public ICommand AgregarConceptoCommand { get; }

    public ICommand QuitarConceptoCommand { get; }

    public async Task InicializarAsync() => await RecargarAsync();

    private void Nueva()
    {
        var edicion = new PlantillaNominaDto();
        if (EmpresaSeleccionada is not null)
        {
            edicion.EmpresaId = EmpresaSeleccionada.Id;
        }

        Edicion = edicion;
        Seleccionada = null;
        Informar("Formulario limpio para nueva plantilla.", esError: false);
    }

    private async Task RecargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            var empresas = await _empresas.ListarAsync();
            EmpresasLista = new ObservableCollection<EmpresaDto>(empresas);
            EmpresaSeleccionada = EmpresasLista.FirstOrDefault(e => e.Activo)
                ?? EmpresasLista.FirstOrDefault();
        });
    }

    private async Task RecargarTodoAsync()
    {
        if (EmpresaSeleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var plantillas = await _plantillas.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            PlantillasLista = new ObservableCollection<PlantillaNominaDto>(plantillas);
            var conceptos = await _conceptos.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            ConceptosLista = new ObservableCollection<ConceptoNominaDto>(conceptos.Where(c => c.Activo).OrderBy(c => c.Orden));
            Edicion.EmpresaId = EmpresaSeleccionada.Id;
            Seleccionada = PlantillasLista.FirstOrDefault();
        });
    }

    private async Task GuardarAsync()
    {
        await EjecutarAsync(async () =>
        {
            if (EmpresaSeleccionada is null)
            {
                Informar("Selecciona una empresa primero.", esError: true);
                return;
            }

            Edicion.EmpresaId = EmpresaSeleccionada.Id;
            var creada = await _plantillas.CrearAsync(Edicion);
            Informar($"Plantilla {creada.Nombre} creada.", esError: false);
            var lista = await _plantillas.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            PlantillasLista = new ObservableCollection<PlantillaNominaDto>(lista);
            Seleccionada = PlantillasLista.FirstOrDefault(p => p.Id == creada.Id);
        });
    }

    private async Task AgregarConceptoAsync()
    {
        if (Seleccionada is null || ConceptoSeleccionado is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var actualizada = await _plantillas.AgregarConceptoAsync(
                Seleccionada.Id,
                ConceptoSeleccionado.Id,
                NuevoOrden,
                NuevoObligatorio,
                NuevoValorDefecto);
            Seleccionada = actualizada;
            SincronizarSeleccionada();
            Informar($"Concepto {ConceptoSeleccionado.Nombre} agregado.", esError: false);
        });
    }

    private async Task QuitarConceptoAsync()
    {
        if (Seleccionada is null || ConceptoEnPlantillaSeleccionado is null)
        {
            Informar("Selecciona un concepto de la plantilla para quitarlo.", esError: true);
            return;
        }

        var conceptoId = ConceptoEnPlantillaSeleccionado.ConceptoNominaId;
        await EjecutarAsync(async () =>
        {
            await _plantillas.QuitarConceptoAsync(Seleccionada.Id, conceptoId);
            var fresca = await _plantillas.ObtenerPorIdAsync(Seleccionada.Id);
            if (fresca is not null)
            {
                Seleccionada = fresca;
                SincronizarSeleccionada();
            }

            Informar("Concepto quitado de la plantilla.", esError: false);
        });
    }

    private void SincronizarSeleccionada()
    {
        if (Seleccionada is null)
        {
            return;
        }

        var item = PlantillasLista.FirstOrDefault(p => p.Id == Seleccionada.Id);
        if (item is not null)
        {
            PlantillasLista[PlantillasLista.IndexOf(item)] = Seleccionada;
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
