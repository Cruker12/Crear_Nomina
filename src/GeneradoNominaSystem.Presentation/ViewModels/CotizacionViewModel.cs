using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using GeneradoNominaSystem.Application.DTOs;
using GeneradoNominaSystem.Application.Interfaces;
using GeneradoNominaSystem.Domain.Enums;
using GeneradoNominaSystem.Presentation.Commands;

namespace GeneradoNominaSystem.Presentation.ViewModels;

public sealed class CotizacionViewModel : ViewModelBase
{
    private readonly ICotizacionService _cotizaciones;
    private readonly IEmpresaService _empresas;
    private readonly IProductoServicioService _productos;
    private readonly IPlantillaCotizacionService _plantillas;
    private readonly IDocumentoService _documentos;

    private ObservableCollection<EmpresaDto> _empresasLista = new();
    private EmpresaDto? _empresaSeleccionada;
    private ObservableCollection<CotizacionDto> _cotizacionesLista = new();
    private CotizacionDto? _seleccionada;
    private CotizacionDto _edicion = new();
    private ObservableCollection<ProductoServicioDto> _productosLista = new();
    private ProductoServicioDto? _productoSeleccionado;
    private ObservableCollection<PlantillaCotizacionDto> _plantillasLista = new();
    private PlantillaCotizacionDto? _plantillaSeleccionada;
    private string _nuevaDescripcion = string.Empty;
    private decimal _nuevaCantidad = 1m;
    private decimal _nuevoPrecio;
    private decimal? _nuevoDescuento;
    private string _mensaje = string.Empty;
    private bool _esError;
    private bool _ocupado;

    public CotizacionViewModel(
        ICotizacionService cotizaciones,
        IEmpresaService empresas,
        IProductoServicioService productos,
        IPlantillaCotizacionService plantillas,
        IDocumentoService documentos)
    {
        _cotizaciones = cotizaciones;
        _empresas = empresas;
        _productos = productos;
        _plantillas = plantillas;
        _documentos = documentos;

        RecargarCommand = new RelayCommand(async _ => await RecargarAsync(), _ => !Ocupado);
        CrearCommand = new RelayCommand(async _ => await CrearAsync(), _ => !Ocupado);
        NuevaCommand = new RelayCommand(_ => Nueva(), _ => !Ocupado);
        AgregarDetalleCommand = new RelayCommand(
            async _ => await AgregarDetalleAsync(),
            _ => !Ocupado && Seleccionada is not null);
        QuitarDetalleCommand = new RelayCommand(
            async param => await QuitarDetalleAsync(param as DetalleCotizacionDto),
            _ => !Ocupado && Seleccionada is not null);
        EnviarCommand = new RelayCommand(
            async _ => await CambiarEstadoAsync(EstadoCotizacion.Enviada),
            _ => !Ocupado && Seleccionada is not null);
        AceptarCommand = new RelayCommand(
            async _ => await CambiarEstadoAsync(EstadoCotizacion.Aceptada),
            _ => !Ocupado && Seleccionada is not null);
        AsignarPlantillaCommand = new RelayCommand(
            async _ => await AsignarPlantillaAsync(),
            _ => !Ocupado && Seleccionada is not null && PlantillaSeleccionada is not null);
        ExportarPdfCommand = new RelayCommand(
            async _ => await ExportarAsync(FormatoExportacion.Pdf),
            _ => !Ocupado && Seleccionada is not null);
        ExportarExcelCommand = new RelayCommand(
            async _ => await ExportarAsync(FormatoExportacion.Excel),
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

    public ObservableCollection<CotizacionDto> CotizacionesLista
    {
        get => _cotizacionesLista;
        private set => SetProperty(ref _cotizacionesLista, value);
    }

    public CotizacionDto? Seleccionada
    {
        get => _seleccionada;
        set => SetProperty(ref _seleccionada, value);
    }

    public CotizacionDto Edicion
    {
        get => _edicion;
        set => SetProperty(ref _edicion, value);
    }

    public ObservableCollection<ProductoServicioDto> ProductosLista
    {
        get => _productosLista;
        private set => SetProperty(ref _productosLista, value);
    }

    public ObservableCollection<PlantillaCotizacionDto> PlantillasLista
    {
        get => _plantillasLista;
        private set => SetProperty(ref _plantillasLista, value);
    }

    public PlantillaCotizacionDto? PlantillaSeleccionada
    {
        get => _plantillaSeleccionada;
        set => SetProperty(ref _plantillaSeleccionada, value);
    }

    public ProductoServicioDto? ProductoSeleccionado
    {
        get => _productoSeleccionado;
        set
        {
            if (SetProperty(ref _productoSeleccionado, value) && value is not null)
            {
                NuevaDescripcion = value.Nombre;
                NuevoPrecio = value.PrecioUnitarioMonto;
            }
        }
    }

    public string NuevaDescripcion
    {
        get => _nuevaDescripcion;
        set => SetProperty(ref _nuevaDescripcion, value);
    }

    public decimal NuevaCantidad
    {
        get => _nuevaCantidad;
        set => SetProperty(ref _nuevaCantidad, value);
    }

    public decimal NuevoPrecio
    {
        get => _nuevoPrecio;
        set => SetProperty(ref _nuevoPrecio, value);
    }

    public decimal? NuevoDescuento
    {
        get => _nuevoDescuento;
        set => SetProperty(ref _nuevoDescuento, value);
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

    public ICommand NuevaCommand { get; }

    public ICommand AgregarDetalleCommand { get; }

    public ICommand QuitarDetalleCommand { get; }

    public ICommand EnviarCommand { get; }

    public ICommand AceptarCommand { get; }

    public ICommand AsignarPlantillaCommand { get; }

    public ICommand ExportarPdfCommand { get; }

    public ICommand ExportarExcelCommand { get; }

    public async Task InicializarAsync() => await RecargarAsync();

    private void Nueva()
    {
        var edicion = new CotizacionDto();
        if (EmpresaSeleccionada is not null)
        {
            edicion.EmpresaId = EmpresaSeleccionada.Id;
            edicion.Moneda = "COP";
        }

        Edicion = edicion;
        Seleccionada = null;
        Informar("Formulario limpio para nueva cotización.", esError: false);
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
            var cotizaciones = await _cotizaciones.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            CotizacionesLista = new ObservableCollection<CotizacionDto>(cotizaciones);
            var productos = await _productos.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            ProductosLista = new ObservableCollection<ProductoServicioDto>(productos.Where(p => p.Activo));
            var plantillas = await _plantillas.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            PlantillasLista = new ObservableCollection<PlantillaCotizacionDto>(plantillas.Where(p => p.Activo));
            PlantillaSeleccionada = PlantillasLista.FirstOrDefault();
            Edicion.EmpresaId = EmpresaSeleccionada.Id;
            Edicion.Moneda = "COP";
            Seleccionada = CotizacionesLista.FirstOrDefault();
        });
    }

    private async Task CrearAsync()
    {
        await EjecutarAsync(async () =>
        {
            if (EmpresaSeleccionada is null)
            {
                Informar("Selecciona una empresa primero.", esError: true);
                return;
            }

            Edicion.EmpresaId = EmpresaSeleccionada.Id;
            Edicion.PlantillaCotizacionId = PlantillaSeleccionada?.Id;
            var creada = await _cotizaciones.CrearAsync(Edicion);
            Informar($"Cotización {creada.NumeroCotizacion} creada.", esError: false);
            var lista = await _cotizaciones.ListarPorEmpresaAsync(EmpresaSeleccionada.Id);
            CotizacionesLista = new ObservableCollection<CotizacionDto>(lista);
            Seleccionada = CotizacionesLista.FirstOrDefault(c => c.Id == creada.Id);
        });
    }

    private async Task AgregarDetalleAsync()
    {
        if (Seleccionada is null || EmpresaSeleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var actualizada = await _cotizaciones.AgregarDetalleAsync(
                Seleccionada.Id,
                NuevaDescripcion,
                NuevaCantidad,
                NuevoPrecio,
                "COP",
                ProductoSeleccionado?.Id,
                NuevoDescuento);
            Seleccionada = actualizada;
            SincronizarSeleccionada();
            NuevaDescripcion = string.Empty;
            NuevaCantidad = 1m;
            NuevoPrecio = 0m;
            NuevoDescuento = null;
            Informar("Detalle agregado.", esError: false);
        });
    }

    private async Task QuitarDetalleAsync(DetalleCotizacionDto? item)    {
        if (Seleccionada is null || item is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _cotizaciones.QuitarDetalleAsync(Seleccionada.Id, item.Id);
            var fresca = await _cotizaciones.ObtenerPorIdAsync(Seleccionada.Id);
            if (fresca is not null)
            {
                Seleccionada = fresca;
                SincronizarSeleccionada();
            }

            Informar("Detalle quitado.", esError: false);
        });
    }

    private async Task CambiarEstadoAsync(EstadoCotizacion nuevoEstado)
    {
        if (Seleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _cotizaciones.CambiarEstadoAsync(Seleccionada.Id, nuevoEstado);
            var fresca = await _cotizaciones.ObtenerPorIdAsync(Seleccionada.Id);
            if (fresca is not null)
            {
                Seleccionada = fresca;
                SincronizarSeleccionada();
            }

            Informar($"Cotización marcada como {nuevoEstado}.", esError: false);
        });
    }

    private async Task AsignarPlantillaAsync()
    {
        if (Seleccionada is null || PlantillaSeleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            await _cotizaciones.AsignarPlantillaAsync(Seleccionada.Id, PlantillaSeleccionada.Id);
            var fresca = await _cotizaciones.ObtenerPorIdAsync(Seleccionada.Id);
            if (fresca is not null)
            {
                Seleccionada = fresca;
                SincronizarSeleccionada();
            }

            Informar($"Plantilla {PlantillaSeleccionada.Nombre} asignada.", esError: false);
        });
    }

    private async Task ExportarAsync(FormatoExportacion formato)
    {
        if (Seleccionada is null)
        {
            return;
        }

        await EjecutarAsync(async () =>
        {
            var carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "GeneradoNominaSystem");
            Directory.CreateDirectory(carpeta);

            var documento = await _documentos.ExportarCotizacionAsync(Seleccionada.Id, formato, carpeta);
            Informar($"Documento {documento.NumeroDocumento} guardado en {documento.RutaArchivo}.", esError: false);
        });
    }

    private void SincronizarSeleccionada()
    {
        if (Seleccionada is null)
        {
            return;
        }

        var item = CotizacionesLista.FirstOrDefault(c => c.Id == Seleccionada.Id);
        if (item is not null)
        {
            CotizacionesLista[CotizacionesLista.IndexOf(item)] = Seleccionada;
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
