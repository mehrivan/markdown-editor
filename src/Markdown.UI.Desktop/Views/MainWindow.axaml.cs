using Avalonia.Controls;

using Markdown.UI.Desktop.ViewModels;

namespace Markdown.UI.Desktop.Views;

/// <summary>
/// Main application window hosting all UI components.
/// </summary>
internal partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Wire up ViewModel when DataContext is set
        DataContextChanged += OnDataContextChanged;
    }

    private async void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainWindowViewModel viewModel)
        {
            // Provide the visual context for dialogs
            viewModel.SetVisualContext(this);

            // Initialize the ViewModel (load last workspace, etc.)
            await viewModel.InitializeAsync();
        }
    }
}
