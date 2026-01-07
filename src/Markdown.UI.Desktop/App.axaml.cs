using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Markdown.UI.Desktop.Services;
using Markdown.UI.Desktop.ViewModels;
using Markdown.UI.Desktop.Views;

using Microsoft.Extensions.DependencyInjection;

namespace Markdown.UI.Desktop;

internal partial class App : Avalonia.Application
{
    public IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configure dependency injection
        var services = new ServiceCollection();
        services.AddDesktopServices();
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Create main window with ViewModel from DI container
            var mainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
