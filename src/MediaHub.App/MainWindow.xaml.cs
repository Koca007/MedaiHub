using System.Windows;
using MediaHub.App.ViewModels;

namespace MediaHub.App;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        _viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        Closed += HandleClosed;
    }

    private void HandleClosed(object? sender, EventArgs e)
    {
        _ = sender;
        _ = e;
        _viewModel.Dispose();
    }
}
