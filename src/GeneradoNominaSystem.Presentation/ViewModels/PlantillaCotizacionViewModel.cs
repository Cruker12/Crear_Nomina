using System.Collections.ObjectModel;
using System.Windows.Input;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Presentation.Commands;

namespace GeneradoNominaSystem.Presentation.ViewModels;

public sealed class PlantillaCotizacionViewModel : ViewModelBase
{
    private readonly IPlantillaCotizacionService _plantillas;
    private readonly IEmpresaService _empresas;

    private ObservableCollection<EmpresaDto> _empresasLista = new();
    private EmpresaDto? _empresaSeleccionada;
    private ObservableCollection<PlantillaCotizacionDto> _plantillasLista = new();
    private PlantillaCotizacionDto _edicion = new();
    private string _mensaje = string.Empty;
    private bool _esError;
    private bool _ocupado;

    public PlantillaCotizacionViewModel(IPlantillaCotizacionService plantillas, IEmpresaService empresas)
    {
        _plantillas = plantillas;
        _empresas = empresas;

        RecargarCommand = new RelayCommand(async _ => await RecargarAsync(), _ => !Ocupado);
        GuardarCommand = new RelayCommand(async _ => await GuardarAsync(), _ => !Ocupado);
        NuevaCommand = new RelayCommand(_ => Nueva(), _ => !Ocupado);
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
                _ = RecargarPlantillasAsync();
            }
        }
    }

    public ObservableCollection<PlantillaCotizacionDto> PlantillasLista
    {
        get => _plantillasLista;
        private set => SetProperty(ref _plantillasLista, value);
    }

    public PlantillaCotizacionDto Edicion
    {
        get => _edicion;
        set => SetProperty(ref _edicion, value);
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

    public async Task InicializarAsync() => await RecargarAsync();

    private void Nueva()
    {
        var edicion = new PlantillaCotizacionDto();
        if (EmpresaSeleccionada is not null)
        {
            edicion.EmpresaId = EmpresaSeleccionada.Id;
        }

        Edicion = edicion;
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

    private async Task RecargarPlantillasAsync()
    {
        if (EmpresaSeleccionada is null)
        {
            PlantillasLista = new ObservableCollection<PlantillaCotizacionDto>();
            return;
        }

        await EjecutarAsync(async () =>
        {
            var lista = await _plantillas.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            PlantillasLista = new ObservableCollection<PlantillaCotizacionDto>(lista.Where(p => p.Activo));
            Edicion.EmpresaId = EmpresaSeleccionada.Id;
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
            await RecargarPlantillasAsync();
            Edicion = new PlantillaCotizacionDto { EmpresaId = EmpresaSeleccionada.Id };
        });
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
