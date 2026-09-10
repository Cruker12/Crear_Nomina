using System.Windows;
using System.Windows.Controls;
using GeneradoNominaSystem.Presentation.ViewModels;

namespace GeneradoNominaSystem.Presentation;

public partial class MainWindow : Window
{
    private readonly HistorialViewModel _historialViewModel;
    public MainWindow(
        MainViewModel viewModel,
        EmpresaViewModel empresaViewModel,
        EmpleadoViewModel empleadoViewModel,
        ConceptoViewModel conceptoViewModel,
        PeriodoViewModel periodoViewModel,
        NominaViewModel nominaViewModel,
        PlantillaViewModel plantillaViewModel,
        ProductoViewModel productoViewModel,
        CotizacionViewModel cotizacionViewModel,
        PlantillaCotizacionViewModel plantillaCotizacionViewModel,
        HistorialViewModel historialViewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        EmpresaView.DataContext = empresaViewModel;
        EmpleadoView.DataContext = empleadoViewModel;
        ConceptoView.DataContext = conceptoViewModel;
        PeriodoView.DataContext = periodoViewModel;
        NominaView.DataContext = nominaViewModel;
        PlantillaView.DataContext = plantillaViewModel;
        ProductoView.DataContext = productoViewModel;
        CotizacionView.DataContext = cotizacionViewModel;
        PlantillaCotizacionView.DataContext = plantillaCotizacionViewModel;
        HistorialView.DataContext = historialViewModel;
        _historialViewModel = historialViewModel;
        Loaded += async (_, _) =>
        {
            await empresaViewModel.InicializarAsync();
            await empleadoViewModel.InicializarAsync();
            await conceptoViewModel.InicializarAsync();
            await periodoViewModel.InicializarAsync();
            await nominaViewModel.InicializarAsync();
            await plantillaViewModel.InicializarAsync();
            await productoViewModel.InicializarAsync();
            await cotizacionViewModel.InicializarAsync();
            await plantillaCotizacionViewModel.InicializarAsync();
            await historialViewModel.InicializarAsync();
        };
    }

    private async void Pestanas_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0 || e.AddedItems[0] is not TabItem { Header: "Historial" })
        {
            return;
        }

        await _historialViewModel.RecargarAsync();
    }
}
