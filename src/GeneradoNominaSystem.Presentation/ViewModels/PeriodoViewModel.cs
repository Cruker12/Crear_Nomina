using System.Collections.ObjectModel;
using System.Windows.Input;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Presentation.Commands;

namespace GeneradoNominaSystem.Presentation.ViewModels;

public sealed class PeriodoViewModel : ViewModelBase
{
    private readonly IPeriodoNominaService _periodos;
    private readonly IEmpresaService _empresas;

    private ObservableCollection<EmpresaDto> _empresasLista = new();
    private EmpresaDto? _empresaSeleccionada;
    private ObservableCollection<PeriodoNominaDto> _periodosLista = new();
    private PeriodoNominaDto _edicion = new();
    private string _mensaje = string.Empty;
    private bool _esError;
    private bool _ocupado;

    public PeriodoViewModel(IPeriodoNominaService periodos, IEmpresaService empresas)
    {
        _periodos = periodos;
        _empresas = empresas;

        RecargarCommand = new RelayCommand(async _ => await RecargarAsync(), _ => !Ocupado);
        GuardarCommand = new RelayCommand(async _ => await GuardarAsync(), _ => !Ocupado);
        NuevoCommand = new RelayCommand(_ => Nuevo(), _ => !Ocupado);
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
                _ = RecargarPeriodosAsync();
            }
        }
    }

    public ObservableCollection<PeriodoNominaDto> PeriodosLista
    {
        get => _periodosLista;
        private set => SetProperty(ref _periodosLista, value);
    }

    public PeriodoNominaDto Edicion
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

    public IReadOnlyList<TipoPeriodo> Tipos { get; } = Enum.GetValues<TipoPeriodo>();

    public ICommand RecargarCommand { get; }

    public ICommand GuardarCommand { get; }

    public ICommand NuevoCommand { get; }

    public async Task InicializarAsync() => await RecargarAsync();

    private void Nuevo()
    {
        var edicion = new PeriodoNominaDto();
        if (EmpresaSeleccionada is not null)
        {
            edicion.EmpresaId = EmpresaSeleccionada.Id;
        }

        Edicion = edicion;
        Informar("Formulario limpio para nuevo periodo.", esError: false);
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

    private async Task RecargarPeriodosAsync()
    {
        if (EmpresaSeleccionada is null)
        {
            PeriodosLista = new ObservableCollection<PeriodoNominaDto>();
            return;
        }

        await EjecutarAsync(async () =>
        {
            var lista = await _periodos.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            PeriodosLista = new ObservableCollection<PeriodoNominaDto>(lista.OrderBy(p => p.FechaInicio));
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
            var creado = await _periodos.CrearAsync(Edicion);
            Informar($"Periodo {creado.Nombre} creado.", esError: false);
            var lista = await _periodos.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            PeriodosLista = new ObservableCollection<PeriodoNominaDto>(lista.OrderBy(p => p.FechaInicio));
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
