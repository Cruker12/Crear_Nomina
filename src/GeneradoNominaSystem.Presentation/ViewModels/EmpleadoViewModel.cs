using System.Collections.ObjectModel;
using System.Windows.Input;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Presentation.Commands;

namespace GeneradoNominaSystem.Presentation.ViewModels;

public sealed class EmpleadoViewModel : ViewModelBase
{
    private readonly IEmpleadoService _empleados;
    private readonly IEmpresaService _empresas;

    private ObservableCollection<EmpresaDto> _empresasLista = new();
    private EmpresaDto? _empresaSeleccionada;
    private ObservableCollection<EmpleadoDto> _empleadosLista = new();
    private EmpleadoDto? _seleccionado;
    private EmpleadoDto _edicion = new();
    private string _mensaje = string.Empty;
    private bool _esError;
    private bool _ocupado;

    public EmpleadoViewModel(IEmpleadoService empleados, IEmpresaService empresas)
    {
        _empleados = empleados;
        _empresas = empresas;

        RecargarCommand = new RelayCommand(async _ => await RecargarAsync(), _ => !Ocupado);
        GuardarCommand = new RelayCommand(async _ => await GuardarAsync(), _ => !Ocupado);
        NuevoCommand = new RelayCommand(_ => Nuevo(), _ => !Ocupado);
        DesactivarCommand = new RelayCommand(
            async _ => await CambiarEstadoAsync(EstadoEmpleado.Inactivo),
            _ => !Ocupado && Seleccionado is not null && Seleccionado.Estado == EstadoEmpleado.Activo);
        ActivarCommand = new RelayCommand(
            async _ => await CambiarEstadoAsync(EstadoEmpleado.Activo),
            _ => !Ocupado && Seleccionado is not null && Seleccionado.Estado != EstadoEmpleado.Activo);
        EliminarCommand = new RelayCommand(
            async _ => await EliminarAsync(),
            _ => !Ocupado && Seleccionado is not null);
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
                _ = RecargarEmpleadosAsync();
            }
        }
    }

    public ObservableCollection<EmpleadoDto> EmpleadosLista
    {
        get => _empleadosLista;
        private set => SetProperty(ref _empleadosLista, value);
    }

    public EmpleadoDto? Seleccionado
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

    public EmpleadoDto Edicion
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

    public IReadOnlyList<TipoDocumentoIdentidad> TiposDocumento { get; }
        = Enum.GetValues<TipoDocumentoIdentidad>();

    public IReadOnlyList<TipoContrato> TiposContrato { get; }
        = Enum.GetValues<TipoContrato>();

    public ICommand RecargarCommand { get; }

    public ICommand GuardarCommand { get; }

    public ICommand NuevoCommand { get; }

    public ICommand DesactivarCommand { get; }

    public ICommand ActivarCommand { get; }

    public ICommand EliminarCommand { get; }

    public async Task InicializarAsync() => await RecargarAsync();

    private void Nuevo()
    {
        Seleccionado = null;
        var edicion = new EmpleadoDto();
        if (EmpresaSeleccionada is not null)
        {
            edicion.EmpresaId = EmpresaSeleccionada.Id;
        }

        Edicion = edicion;
        Informar("Formulario limpio para nuevo empleado.", esError: false);
    }

    private async Task RecargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            var empresas = await _empresas.ListarAsync();
            EmpresasLista = new ObservableCollection<EmpresaDto>(empresas);
            EmpresaSeleccionada = EmpresasLista.FirstOrDefault(e => e.Activo)
                ?? EmpresasLista.FirstOrDefault();

            if (EmpresaSeleccionada is null)
            {
                Informar("Primero crea una empresa en la pestaña Empresas.", esError: false);
            }
        });
    }

    private async Task RecargarEmpleadosAsync()
    {
        if (EmpresaSeleccionada is null)
        {
            EmpleadosLista = new ObservableCollection<EmpleadoDto>();
            return;
        }

        await EjecutarAsync(async () =>
        {
            var lista = await _empleados.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            EmpleadosLista = new ObservableCollection<EmpleadoDto>(lista);
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

            if (Edicion.Id == Guid.Empty)
            {
                var creado = await _empleados.CrearAsync(Edicion);
                Informar($"Empleado {creado.Nombres} {creado.Apellidos} creado.", esError: false);
            }
            else
            {
                var actualizado = await _empleados.ActualizarAsync(Edicion);
                Informar($"Empleado {actualizado.Nombres} {actualizado.Apellidos} actualizado.", esError: false);
            }

            var lista = await _empleados.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            EmpleadosLista = new ObservableCollection<EmpleadoDto>(lista);
        });
    }

    private async Task CambiarEstadoAsync(EstadoEmpleado nuevoEstado)
    {
        if (Seleccionado is null || EmpresaSeleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _empleados.CambiarEstadoAsync(Seleccionado.Id, nuevoEstado);
            var mensaje = nuevoEstado switch
            {
                EstadoEmpleado.Activo => "Empleado activado.",
                EstadoEmpleado.Inactivo => "Empleado desactivado (baja lógica).",
                _ => $"Empleado en estado {nuevoEstado}.",
            };
            Informar(mensaje, esError: false);
            var lista = await _empleados.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            EmpleadosLista = new ObservableCollection<EmpleadoDto>(lista);
        });
    }

    private async Task EliminarAsync()
    {
        if (Seleccionado is null || EmpresaSeleccionada is null)
        {
            return;
        }

        var nombre = $"{Seleccionado.Nombres} {Seleccionado.Apellidos}";
        var id = Seleccionado.Id;
        var empresaId = EmpresaSeleccionada.Id;

        await EjecutarAsync(async () =>
        {
            await _empleados.EliminarAsync(id);
            Seleccionado = null;
            Edicion = new EmpleadoDto { EmpresaId = empresaId };
            Informar($"Empleado {nombre} eliminado.", esError: false);
            var lista = await _empleados.ListarPorEmpresaAsync(empresaId);
            EmpleadosLista = new ObservableCollection<EmpleadoDto>(lista);
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

    private static EmpleadoDto Clonar(EmpleadoDto origen)
    {
        return new EmpleadoDto
        {
            Id = origen.Id,
            EmpresaId = origen.EmpresaId,
            EmpresaNombre = origen.EmpresaNombre,
            TipoDocumento = origen.TipoDocumento,
            NumeroDocumento = origen.NumeroDocumento,
            Nombres = origen.Nombres,
            Apellidos = origen.Apellidos,
            Email = origen.Email,
            Telefono = origen.Telefono,
            DireccionCompleta = origen.DireccionCompleta,
            Ciudad = origen.Ciudad,
            Departamento = origen.Departamento,
            Pais = origen.Pais,
            CodigoPostal = origen.CodigoPostal,
            FechaIngreso = origen.FechaIngreso,
            Cargo = origen.Cargo,
            DepartamentoArea = origen.DepartamentoArea,
            TipoContrato = origen.TipoContrato,
            SalarioBaseMonto = origen.SalarioBaseMonto,
            SalarioBaseMoneda = origen.SalarioBaseMoneda,
            Estado = origen.Estado,
        };
    }
}
