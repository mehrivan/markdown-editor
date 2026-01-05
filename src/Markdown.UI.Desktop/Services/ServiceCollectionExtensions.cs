using Markdown.Infrastructure.FileSystem;
using Markdown.Infrastructure.Services;
using Markdown.UI.Desktop.ViewModels;

namespace Markdown.UI.Desktop.Services;

/// <summary>
/// Extension methods for configuring dependency injection services.
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all desktop application services to the service collection.
    /// </summary>
    public static IServiceCollection AddDesktopServices(this IServiceCollection services)
    {
        // Infrastructure services
        _ = services.AddSingleton<IDocumentRepository, FileDocumentRepository>();
        _ = services.AddSingleton<IWorkspaceRepository, FileWorkspaceRepository>();
        _ = services.AddSingleton<IWorkspaceExplorer, WorkspaceExplorer>();
        _ = services.AddSingleton<IDocumentService, DocumentService>();

        // UI Services
        _ = services.AddSingleton<ISettingsService, SettingsService>();
        _ = services.AddSingleton<IThemeService, ThemeService>();
        _ = services.AddSingleton<IDialogService, DialogService>();
        _ = services.AddSingleton<IAutoSaveService, AutoSaveService>();

        // ViewModels
        _ = services.AddTransient<MainWindowViewModel>();
        _ = services.AddTransient<StatusBarViewModel>();
        _ = services.AddTransient<EditorViewModel>();
        _ = services.AddTransient<TabViewModel>();

        return services;
    }
}
