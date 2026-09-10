using System.Collections.ObjectModel;
using System.Windows.Input;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Presentation.Commands;

namespace GeneradoNominaSystem.Presentation.ViewModels;

public sealed class ProductoViewModel : ViewModelBase
{
    private readonly IProductoServicioService _productos;
    private readonly IEmpresaService _empresas;

    private ObservableCollection<EmpresaDto> _empresasLista = new();
    private EmpresaDto? _empresaSeleccionada;
    private ObservableCollection<ProductoServicioDto> _productosLista = new();
    private ProductoServicioDto? _seleccionado;
    private ProductoServicioDto _edicion = new();
    private string _mensaje = string.Empty;
    private bool _esError;
    private bool _ocupado;

    public ProductoViewModel(IProductoServicioService productos, IEmpresaService empresas)
    {
        _productos = productos;
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
                _ = RecargarProductosAsync();
            }
        }
    }

    public ObservableCollection<ProductoServicioDto> ProductosLista
    {
        get => _productosLista;
        private set => SetProperty(ref _productosLista, value);
    }

    public ProductoServicioDto? Seleccionado
    {
        get => _seleccionado;
        set => SetProperty(ref _seleccionado, value);
    }

    public ProductoServicioDto Edicion
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

    public ICommand NuevoCommand { get; }

    public ICommand DesactivarCommand { get; }

    public ICommand ActivarCommand { get; }

    public ICommand EliminarCommand { get; }

    public async Task InicializarAsync() => await RecargarAsync();

    private void Nuevo()
    {
        var edicion = new ProductoServicioDto();
        if (EmpresaSeleccionada is not null)
        {
            edicion.EmpresaId = EmpresaSeleccionada.Id;
        }

        Edicion = edicion;
        Informar("Formulario limpio para nuevo producto/servicio.", esError: false);
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

    private async Task RecargarProductosAsync()
    {
        if (EmpresaSeleccionada is null)
        {
            ProductosLista = new ObservableCollection<ProductoServicioDto>();
            return;
        }

        await EjecutarAsync(async () =>
        {
            var lista = await _productos.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            ProductosLista = new ObservableCollection<ProductoServicioDto>(lista);
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
            var creado = await _productos.CrearAsync(Edicion);
            Informar($"Producto/servicio {creado.Nombre} creado.", esError: false);
            await RecargarProductosAsync();
            Edicion = new ProductoServicioDto { EmpresaId = EmpresaSeleccionada.Id };
        });
    }

    private async Task CambiarAsync(bool activar)
    {
        if (Seleccionado is null || EmpresaSeleccionada is null)
        {
            return;
        }

        var id = Seleccionado.Id;
        var empresaId = EmpresaSeleccionada.Id;

        await EjecutarAsync(async () =>
        {
            if (activar)
            {
                await _productos.ActivarAsync(id);
                Informar("Producto/servicio activado.", esError: false);
            }
            else
            {
                await _productos.DesactivarAsync(id);
                Informar("Producto/servicio desactivado (baja lógica).", esError: false);
            }

            await RecargarSilenciosoAsync(empresaId, id);
        });
    }

    private async Task EliminarAsync()
    {
        if (Seleccionado is null || EmpresaSeleccionada is null)
        {
            return;
        }

        var nombre = Seleccionado.Nombre;
        var id = Seleccionado.Id;
        var empresaId = EmpresaSeleccionada.Id;

        await EjecutarAsync(async () =>
        {
            await _productos.EliminarAsync(id);
            Seleccionado = null;
            Edicion = new ProductoServicioDto { EmpresaId = empresaId };
            Informar($"Producto/servicio {nombre} eliminado.", esError: false);
            await RecargarSilenciosoAsync(empresaId, null);
        });
    }

    private async Task RecargarSilenciosoAsync(Guid empresaId, Guid? seleccionarId)
    {
        var lista = await _productos.ListarPorEmpresaAsync(empresaId);
        ProductosLista = new ObservableCollection<ProductoServicioDto>(lista);
        Seleccionado = seleccionarId.HasValue
            ? ProductosLista.FirstOrDefault(p => p.Id == seleccionarId.Value)
            : ProductosLista.FirstOrDefault();
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
