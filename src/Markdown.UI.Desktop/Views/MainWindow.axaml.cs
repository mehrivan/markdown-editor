using Avalonia;
using Avalonia.Controls;

using Markdown.UI.Desktop.Services;
using Markdown.UI.Desktop.ViewModels;

using Microsoft.Extensions.DependencyInjection;

namespace Markdown.UI.Desktop.Views;

/// <summary>
/// Main application window hosting all UI components.
/// </summary>
internal partial class MainWindow : Window
{
    private readonly ISettingsService _settingsService;

    public MainWindow()
    {
        InitializeComponent();

        // Get settings service from DI container
        _settingsService = ((App)Avalonia.Application.Current!).Services
            .GetRequiredService<ISettingsService>();

        // Restore window state on load
        RestoreWindowState();

        // Wire up ViewModel when DataContext is set
        DataContextChanged += OnDataContextChanged;

        // Save state when window closes
        Closing += OnWindowClosing;
    }

    private void RestoreWindowState()
    {
        var settings = _settingsService.Load();

        // Set window dimensions
        Width = settings.WindowWidth;
        Height = settings.WindowHeight;

        // Restore sidebar width
        SidebarGrid.ColumnDefinitions[0].Width = new GridLength(settings.SidebarWidth);

        // Validate position is on a visible screen
        if (IsPositionValid(settings.WindowX, settings.WindowY, settings.WindowWidth, settings.WindowHeight))
        {
            Position = new PixelPoint((int)settings.WindowX, (int)settings.WindowY);
        }
        else
        {
            // Fallback to center screen when saved position is invalid
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        // Apply maximized state after position/size
        if (settings.IsMaximized)
        {
            WindowState = WindowState.Maximized;
        }
    }

    private bool IsPositionValid(double x, double y, double width, double height)
    {
        // First launch: position is 0,0 with default dimensions - use center screen
        if (x == 0 && y == 0)
        {
            return false;
        }

        // Check if window would be visible on any screen
        var screens = Screens?.All;
        if (screens is null || !screens.Any())
        {
            return false;
        }

        var windowRect = new Rect(x, y, width, height);

        foreach (var screen in screens)
        {
            var screenBounds = screen.WorkingArea;
            var screenRect = new Rect(screenBounds.X, screenBounds.Y, screenBounds.Width, screenBounds.Height);

            // Check if at least 100px of window is visible on this screen
            if (windowRect.Intersects(screenRect))
            {
                var intersection = windowRect.Intersect(screenRect);
                if (intersection.Width >= 100 && intersection.Height >= 100)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        SaveWindowState();
    }

    private void SaveWindowState()
    {
        var settings = _settingsService.Load();

        // Save maximized state
        settings.IsMaximized = WindowState == WindowState.Maximized;

        // Only save position/size if window is in normal state
        if (WindowState == WindowState.Normal)
        {
            settings.WindowX = Position.X;
            settings.WindowY = Position.Y;
            settings.WindowWidth = Width;
            settings.WindowHeight = Height;
        }

        // Always save sidebar width (it's independent of window state)
        settings.SidebarWidth = SidebarGrid.ColumnDefinitions[0].Width.Value;

        _ = _settingsService.SaveAsync(settings);
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
