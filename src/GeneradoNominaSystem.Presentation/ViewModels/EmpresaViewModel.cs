using System.Collections.ObjectModel;
using System.Windows.Input;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Presentation.Commands;

namespace GeneradoNominaSystem.Presentation.ViewModels;

public sealed class EmpresaViewModel : ViewModelBase
{
    private readonly IEmpresaService _empresas;

    private ObservableCollection<EmpresaDto> _empresasLista = new();
    private EmpresaDto? _seleccionada;
    private EmpresaDto _edicion = new();
    private string _mensaje = string.Empty;
    private bool _esError;
    private bool _ocupado;

    public EmpresaViewModel(IEmpresaService empresas)
    {
        _empresas = empresas;

        RecargarCommand = new RelayCommand(async _ => await RecargarAsync(), _ => !Ocupado);
        GuardarCommand = new RelayCommand(async _ => await GuardarAsync(), _ => !Ocupado);
        NuevaCommand = new RelayCommand(_ => Nueva(), _ => !Ocupado);
        DesactivarCommand = new RelayCommand(
            async _ => await CambiarAsync(false),
            _ => !Ocupado && Seleccionada is not null && Seleccionada.Activo);
        ActivarCommand = new RelayCommand(
            async _ => await CambiarAsync(true),
            _ => !Ocupado && Seleccionada is not null && !Seleccionada.Activo);
        EliminarCommand = new RelayCommand(
            async _ => await EliminarAsync(),
            _ => !Ocupado && Seleccionada is not null);
    }

    public ObservableCollection<EmpresaDto> EmpresasLista
    {
        get => _empresasLista;
        private set => SetProperty(ref _empresasLista, value);
    }

    public EmpresaDto? Seleccionada
    {
        get => _seleccionada;
        set
        {
            if (SetProperty(ref _seleccionada, value) && value is not null)
            {
                Edicion = Clonar(value);
            }
        }
    }

    public EmpresaDto Edicion
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

    public ICommand DesactivarCommand { get; }

    public ICommand ActivarCommand { get; }

    public ICommand EliminarCommand { get; }

    public async Task InicializarAsync() => await RecargarAsync();

    private void Nueva()
    {
        Seleccionada = null;
        Edicion = new EmpresaDto();
        Informar("Formulario limpio para nueva empresa.", esError: false);
    }

    private async Task RecargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            var lista = await _empresas.ListarAsync();
            EmpresasLista = new ObservableCollection<EmpresaDto>(lista);
            Informar($"{lista.Count} empresa(s) cargada(s).", esError: false);
        });
    }

    private async Task GuardarAsync()
    {
        await EjecutarAsync(async () =>
        {
            if (Edicion.Id == Guid.Empty)
            {
                var creada = await _empresas.CrearAsync(Edicion);
                Informar($"Empresa {creada.NombreComercial} creada.", esError: false);
            }
            else
            {
                var actualizada = await _empresas.ActualizarAsync(Edicion);
                Informar($"Empresa {actualizada.NombreComercial} actualizada.", esError: false);
            }

            await RecargarSilenciosoAsync();
        });
    }

    private async Task CambiarAsync(bool activar)
    {
        if (Seleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            if (activar)
            {
                await _empresas.ActivarAsync(Seleccionada.Id);
                Informar("Empresa activada.", esError: false);
            }
            else
            {
                await _empresas.DesactivarAsync(Seleccionada.Id);
                Informar("Empresa desactivada (baja lógica).", esError: false);
            }

            await RecargarSilenciosoAsync();
        });
    }

    private async Task EliminarAsync()
    {
        if (Seleccionada is null)
        {
            return;
        }

        var nombre = Seleccionada.NombreComercial;
        var id = Seleccionada.Id;

        await EjecutarAsync(async () =>
        {
            await _empresas.EliminarAsync(id);
            Seleccionada = null;
            Edicion = new EmpresaDto();
            Informar($"Empresa {nombre} eliminada.", esError: false);
            await RecargarSilenciosoAsync();
        });
    }

    private async Task RecargarSilenciosoAsync()
    {
        var lista = await _empresas.ListarAsync();
        EmpresasLista = new ObservableCollection<EmpresaDto>(lista);
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

    private static EmpresaDto Clonar(EmpresaDto origen)
    {
        return new EmpresaDto
        {
            Id = origen.Id,
            RazonSocial = origen.RazonSocial,
            NombreComercial = origen.NombreComercial,
            Nit = origen.Nit,
            Telefono = origen.Telefono,
            Email = origen.Email,
            LogoRuta = origen.LogoRuta,
            DireccionCompleta = origen.DireccionCompleta,
            Ciudad = origen.Ciudad,
            Departamento = origen.Departamento,
            Pais = origen.Pais,
            CodigoPostal = origen.CodigoPostal,
            DigitoVerificacion = origen.DigitoVerificacion,
            RegimenTributario = origen.RegimenTributario,
            ResponsableIva = origen.ResponsableIva,
            RepresentanteLegal = origen.RepresentanteLegal,
            Activo = origen.Activo,
        };
    }
}
