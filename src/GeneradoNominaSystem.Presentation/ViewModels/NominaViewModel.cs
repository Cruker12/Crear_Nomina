using System.Collections.ObjectModel;
using System.Windows.Input;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Presentation.Commands;

namespace GeneradoNominaSystem.Presentation.ViewModels;

public sealed class NominaViewModel : ViewModelBase
{
    private readonly INominaService _nominas;
    private readonly IEmpresaService _empresas;
    private readonly IEmpleadoService _empleados;
    private readonly IPeriodoNominaService _periodos;
    private readonly IConceptoNominaService _conceptos;
    private readonly IPlantillaNominaService _plantillas;

    private ObservableCollection<EmpresaDto> _empresasLista = new();
    private EmpresaDto? _empresaSeleccionada;
    private ObservableCollection<EmpleadoDto> _empleadosLista = new();
    private EmpleadoDto? _empleadoSeleccionado;
    private ObservableCollection<PeriodoNominaDto> _periodosLista = new();
    private PeriodoNominaDto? _periodoSeleccionado;
    private ObservableCollection<NominaDto> _nominasLista = new();
    private NominaDto? _seleccionada;
    private ObservableCollection<ConceptoNominaDto> _conceptosLista = new();
    private ConceptoNominaDto? _conceptoSeleccionado;
    private ObservableCollection<PlantillaNominaDto> _plantillasLista = new();
    private PlantillaNominaDto? _plantillaSeleccionada;
    private decimal _nuevoValor;
    private decimal? _nuevaCantidad;
    private decimal? _baseMonto;
    private string _motivo = string.Empty;
    private string _mensaje = string.Empty;
    private bool _esError;
    private bool _ocupado;

    public NominaViewModel(
        INominaService nominas,
        IEmpresaService empresas,
        IEmpleadoService empleados,
        IPeriodoNominaService periodos,
        IConceptoNominaService conceptos,
        IPlantillaNominaService plantillas)
    {
        _nominas = nominas;
        _empresas = empresas;
        _empleados = empleados;
        _periodos = periodos;
        _conceptos = conceptos;
        _plantillas = plantillas;

        RecargarCommand = new RelayCommand(async _ => await RecargarAsync(), _ => !Ocupado);
        CrearCommand = new RelayCommand(
            async _ => await CrearAsync(),
            _ => !Ocupado && EmpleadoSeleccionado is not null && PeriodoSeleccionado is not null);
        CrearDesdePlantillaCommand = new RelayCommand(
            async _ => await CrearDesdePlantillaAsync(),
            _ => !Ocupado && EmpleadoSeleccionado is not null && PeriodoSeleccionado is not null && PlantillaSeleccionada is not null);
        AgregarDetalleCommand = new RelayCommand(
            async _ => await AgregarDetalleAsync(),
            _ => !Ocupado && Seleccionada is not null && ConceptoSeleccionado is not null);
        CalcularCommand = new RelayCommand(
            async _ => await CalcularAsync(),
            _ => !Ocupado && Seleccionada is not null);
        AprobarCommand = new RelayCommand(
            async _ => await AprobarAsync(),
            _ => !Ocupado && Seleccionada is not null);
        PagarCommand = new RelayCommand(
            async _ => await PagarAsync(),
            _ => !Ocupado && Seleccionada is not null);
        AnularCommand = new RelayCommand(
            async _ => await AnularAsync(),
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
                _ = RecargarDependientesAsync();
            }
        }
    }

    public ObservableCollection<EmpleadoDto> EmpleadosLista
    {
        get => _empleadosLista;
        private set => SetProperty(ref _empleadosLista, value);
    }

    public EmpleadoDto? EmpleadoSeleccionado
    {
        get => _empleadoSeleccionado;
        set
        {
            if (SetProperty(ref _empleadoSeleccionado, value))
            {
                if (value is not null)
                {
                    BaseMonto = value.SalarioBaseMonto;
                }

                _ = RecargarNominasAsync();
            }
        }
    }

    public ObservableCollection<PeriodoNominaDto> PeriodosLista
    {
        get => _periodosLista;
        private set => SetProperty(ref _periodosLista, value);
    }

    public PeriodoNominaDto? PeriodoSeleccionado
    {
        get => _periodoSeleccionado;
        set
        {
            if (SetProperty(ref _periodoSeleccionado, value))
            {
                _ = RecargarNominasAsync();
            }
        }
    }

    public ObservableCollection<NominaDto> NominasLista
    {
        get => _nominasLista;
        private set => SetProperty(ref _nominasLista, value);
    }

    public NominaDto? Seleccionada
    {
        get => _seleccionada;
        set => SetProperty(ref _seleccionada, value);
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

    public ObservableCollection<PlantillaNominaDto> PlantillasLista
    {
        get => _plantillasLista;
        private set => SetProperty(ref _plantillasLista, value);
    }

    public PlantillaNominaDto? PlantillaSeleccionada
    {
        get => _plantillaSeleccionada;
        set => SetProperty(ref _plantillaSeleccionada, value);
    }

    public decimal NuevoValor
    {
        get => _nuevoValor;
        set => SetProperty(ref _nuevoValor, value);
    }

    public decimal? NuevaCantidad
    {
        get => _nuevaCantidad;
        set => SetProperty(ref _nuevaCantidad, value);
    }

    public decimal? BaseMonto
    {
        get => _baseMonto;
        set => SetProperty(ref _baseMonto, value);
    }

    public string Motivo
    {
        get => _motivo;
        set => SetProperty(ref _motivo, value);
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

    public ICommand CrearCommand { get; }

    public ICommand CrearDesdePlantillaCommand { get; }

    public ICommand AgregarDetalleCommand { get; }

    public ICommand CalcularCommand { get; }

    public ICommand AprobarCommand { get; }

    public ICommand PagarCommand { get; }

    public ICommand AnularCommand { get; }

    public async Task InicializarAsync() => await RecargarAsync();

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

    private async Task RecargarDependientesAsync()
    {
        if (EmpresaSeleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var empleados = await _empleados.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            EmpleadosLista = new ObservableCollection<EmpleadoDto>(empleados.Where(e => e.Estado == Domain.Enums.EstadoEmpleado.Activo));
            var periodos = await _periodos.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            PeriodosLista = new ObservableCollection<PeriodoNominaDto>(periodos.Where(p => p.Activo));
            var conceptos = await _conceptos.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            ConceptosLista = new ObservableCollection<ConceptoNominaDto>(conceptos.Where(c => c.Activo).OrderBy(c => c.Orden));
            var plantillas = await _plantillas.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            PlantillasLista = new ObservableCollection<PlantillaNominaDto>(plantillas.Where(p => p.Activo));

            EmpleadoSeleccionado = EmpleadosLista.FirstOrDefault();
            PeriodoSeleccionado = PeriodosLista.FirstOrDefault();
            PlantillaSeleccionada = PlantillasLista.FirstOrDefault();
        });
    }

    private async Task RecargarNominasAsync()
    {
        if (PeriodoSeleccionado is null)
        {
            NominasLista = new ObservableCollection<NominaDto>();
            return;
        }

        await EjecutarAsync(async () =>
        {
            var lista = await _nominas.ListarPorPeriodoAsync(PeriodoSeleccionado.Id);
            if (EmpleadoSeleccionado is not null)
            {
                lista = lista.Where(n => n.EmpleadoId == EmpleadoSeleccionado.Id).ToList();
            }

            NominasLista = new ObservableCollection<NominaDto>(lista);
        });
    }

    private async Task CrearAsync()
    {
        if (EmpresaSeleccionada is null || EmpleadoSeleccionado is null || PeriodoSeleccionado is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var creada = await _nominas.CrearAsync(EmpresaSeleccionada.Id, EmpleadoSeleccionado.Id, PeriodoSeleccionado.Id);
            Informar($"Nómina creada en Borrador para {creada.EmpleadoNombre}.", esError: false);
            await RecargarNominasAsync();
        });
    }

    private async Task CrearDesdePlantillaAsync()
    {
        if (EmpresaSeleccionada is null || EmpleadoSeleccionado is null || PeriodoSeleccionado is null || PlantillaSeleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var creada = await _nominas.CrearDesdePlantillaAsync(
                EmpresaSeleccionada.Id,
                EmpleadoSeleccionado.Id,
                PeriodoSeleccionado.Id,
                PlantillaSeleccionada.Id);
            Informar($"Nómina creada desde {PlantillaSeleccionada.Nombre} con {creada.Detalles.Count} detalle(s).", esError: false);
            await RecargarNominasAsync();
        });
    }

    private async Task AgregarDetalleAsync()
    {
        if (Seleccionada is null || ConceptoSeleccionado is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var actualizada = await _nominas.AgregarDetalleAsync(
                Seleccionada.Id,
                ConceptoSeleccionado.Id,
                NuevoValor,
                "COP",
                NuevaCantidad);
            Seleccionada = actualizada;
            SincronizarSeleccionada();
            NuevoValor = 0m;
            NuevaCantidad = null;
            Informar($"Detalle {ConceptoSeleccionado.Nombre} agregado.", esError: false);
        });
    }

    private async Task CalcularAsync()
    {
        if (Seleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var calculada = await _nominas.CalcularAsync(Seleccionada.Id, BaseMonto);
            Seleccionada = calculada;
            SincronizarSeleccionada();
            Informar($"Calculada. Neto: {calculada.TotalNeto} {calculada.Moneda}.", esError: false);
        });
    }

    private async Task AprobarAsync()
    {
        if (Seleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _nominas.AprobarAsync(Seleccionada.Id);
            await RefrescarSeleccionadaAsync();
            Informar("Nómina aprobada.", esError: false);
        });
    }

    private async Task PagarAsync()
    {
        if (Seleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _nominas.MarcarPagadaAsync(Seleccionada.Id);
            await RefrescarSeleccionadaAsync();
            Informar("Nómina pagada.", esError: false);
        });
    }

    private async Task AnularAsync()
    {
        if (Seleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _nominas.AnularAsync(Seleccionada.Id, Motivo);
            await RefrescarSeleccionadaAsync();
            Informar("Nómina anulada.", esError: false);
        });
    }

    private async Task RefrescarSeleccionadaAsync()
    {
        if (Seleccionada is null)
        {
            return;
        }

        var fresca = await _nominas.ObtenerPorIdAsync(Seleccionada.Id);
        if (fresca is not null)
        {
            Seleccionada = fresca;
            SincronizarSeleccionada();
        }
    }

    private void SincronizarSeleccionada()
    {
        if (Seleccionada is null)
        {
            return;
        }

        var item = NominasLista.FirstOrDefault(n => n.Id == Seleccionada.Id);
        if (item is not null)
        {
            var index = NominasLista.IndexOf(item);
            NominasLista[index] = Seleccionada;
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
