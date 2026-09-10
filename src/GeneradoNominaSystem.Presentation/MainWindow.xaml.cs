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
        NominaViewModel nominaViewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        EmpresaView.DataContext = empresaViewModel;
        EmpleadoView.DataContext = empleadoViewModel;
        ConceptoView.DataContext = conceptoViewModel;
        PeriodoView.DataContext = periodoViewModel;
        NominaView.DataContext = nominaViewModel;
        Loaded += async (_, _) =>
        {
            await empresaViewModel.InicializarAsync();
            await empleadoViewModel.InicializarAsync();
            await conceptoViewModel.InicializarAsync();
            await periodoViewModel.InicializarAsync();
            await nominaViewModel.InicializarAsync();
        };
    }
}
