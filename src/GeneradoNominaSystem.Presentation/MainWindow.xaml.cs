using System.Windows;
using GeneradoNominaSystem.Presentation.ViewModels;

namespace GeneradoNominaSystem.Presentation;

public partial class MainWindow : Window
{
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
        PlantillaCotizacionViewModel plantillaCotizacionViewModel)
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
        };
    }
}
