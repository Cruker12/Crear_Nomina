using System.Windows;
using GeneradoNominaSystem.Presentation.ViewModels;

namespace GeneradoNominaSystem.Presentation;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel, EmpresaViewModel empresaViewModel, EmpleadoViewModel empleadoViewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        EmpresaView.DataContext = empresaViewModel;
        EmpleadoView.DataContext = empleadoViewModel;
        Loaded += async (_, _) =>
        {
            await empresaViewModel.InicializarAsync();
            await empleadoViewModel.InicializarAsync();
        };
    }
}
