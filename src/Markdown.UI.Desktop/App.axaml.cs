using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Markdown.UI.Desktop.Services;
using Markdown.UI.Desktop.Views;

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
        ServiceCollection services = new();
        _ = services.AddDesktopServices();
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
