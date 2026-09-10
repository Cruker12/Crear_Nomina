using System.Collections.ObjectModel;
using System.Windows.Input;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Presentation.Commands;

namespace GeneradoNominaSystem.Presentation.ViewModels;

public sealed class ConceptoViewModel : ViewModelBase
{
    private readonly IConceptoNominaService _conceptos;
    private readonly IEmpresaService _empresas;

    private ObservableCollection<EmpresaDto> _empresasLista = new();
    private EmpresaDto? _empresaSeleccionada;
    private ObservableCollection<ConceptoNominaDto> _conceptosLista = new();
    private ConceptoNominaDto? _seleccionado;
    private ConceptoNominaDto _edicion = new();
    private string _mensaje = string.Empty;
    private bool _esError;
    private bool _ocupado;

    public ConceptoViewModel(IConceptoNominaService conceptos, IEmpresaService empresas)
    {
        _conceptos = conceptos;
        _empresas = empresas;

        RecargarCommand = new RelayCommand(async _ => await RecargarAsync(), _ => !Ocupado);
        GuardarCommand = new RelayCommand(async _ => await GuardarAsync(), _ => !Ocupado);
        NuevoCommand = new RelayCommand(_ => Nuevo(), _ => !Ocupado);
        DesactivarCommand = new RelayCommand(
            async _ => await CambiarAsync(false),
            _ => !Ocupado && Seleccionado is not null && Seleccionado.Activo);
        ActivarCommand = new RelayCommand(
            async _ => await CambiarAsync(true),
            _ => !Ocupado && Seleccionado is not null && !Seleccionado.Activo);
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
                _ = RecargarConceptosAsync();
            }
        }
    }

    public ObservableCollection<ConceptoNominaDto> ConceptosLista
    {
        get => _conceptosLista;
        private set => SetProperty(ref _conceptosLista, value);
    }

    public ConceptoNominaDto? Seleccionado
    {
        get => _seleccionado;
        set
        {
            if (SetProperty(ref _seleccionado, value) && value is not null)
            {
                Edicion = Clonar(value);
            }
        }
    }

    public ConceptoNominaDto Edicion
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

    public IReadOnlyList<TipoConcepto> Tipos { get; } = Enum.GetValues<TipoConcepto>();

    public ICommand RecargarCommand { get; }

    public ICommand GuardarCommand { get; }

    public ICommand NuevoCommand { get; }

    public ICommand DesactivarCommand { get; }

    public ICommand ActivarCommand { get; }

    public async Task InicializarAsync() => await RecargarAsync();

    private void Nuevo()
    {
        Seleccionado = null;
        var edicion = new ConceptoNominaDto();
        if (EmpresaSeleccionada is not null)
        {
            edicion.EmpresaId = EmpresaSeleccionada.Id;
        }

        Edicion = edicion;
        Informar("Formulario limpio para nuevo concepto.", esError: false);
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

    private async Task RecargarConceptosAsync()
    {
        if (EmpresaSeleccionada is null)
        {
            ConceptosLista = new ObservableCollection<ConceptoNominaDto>();
            return;
        }

        await EjecutarAsync(async () =>
        {
            var lista = await _conceptos.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            ConceptosLista = new ObservableCollection<ConceptoNominaDto>(lista.OrderBy(c => c.Orden));
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
            var creado = await _conceptos.CrearAsync(Edicion);
            Informar($"Concepto {creado.Nombre} creado.", esError: false);
            var lista = await _conceptos.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            ConceptosLista = new ObservableCollection<ConceptoNominaDto>(lista.OrderBy(c => c.Orden));
        });
    }

    private async Task CambiarAsync(bool activar)
    {
        if (Seleccionado is null || EmpresaSeleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            if (activar)
            {
                await _conceptos.ActivarAsync(Seleccionado.Id);
            }
            else
            {
                await _conceptos.DesactivarAsync(Seleccionado.Id);
            }

            var lista = await _conceptos.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            ConceptosLista = new ObservableCollection<ConceptoNominaDto>(lista.OrderBy(c => c.Orden));
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

    private static ConceptoNominaDto Clonar(ConceptoNominaDto o)
    {
        return new ConceptoNominaDto
        {
            Id = o.Id,
            EmpresaId = o.EmpresaId,
            Nombre = o.Nombre,
            Tipo = o.Tipo,
            Subtipo = o.Subtipo,
            EsPorcentaje = o.EsPorcentaje,
            PorcentajeBase = o.PorcentajeBase,
            ValorFijoMonto = o.ValorFijoMonto,
            ValorFijoMoneda = o.ValorFijoMoneda,
            Orden = o.Orden,
            Activo = o.Activo,
        };
    }
}
