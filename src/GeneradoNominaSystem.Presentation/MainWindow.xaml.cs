using System.Windows;
using GeneradoNominaSystem.Presentation.ViewModels;

namespace GeneradoNominaSystem.Presentation;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel, EmpresaViewModel empresaViewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        EmpresaView.DataContext = empresaViewModel;
        Loaded += async (_, _) => await empresaViewModel.InicializarAsync();
    }
}
