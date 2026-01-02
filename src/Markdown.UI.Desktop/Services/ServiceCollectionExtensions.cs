using Markdown.Infrastructure.FileSystem;
using Markdown.Infrastructure.Services;

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

        // UI Services (to be added in later tasks)
        // services.AddSingleton<IDialogService, DialogService>();
        // services.AddSingleton<IAutoSaveService, AutoSaveService>();
        // services.AddSingleton<ISettingsService, SettingsService>();
        // services.AddSingleton<IThemeService, ThemeService>();

        // ViewModels (to be added in later tasks)
        // services.AddTransient<MainWindowViewModel>();
        // services.AddTransient<FileExplorerViewModel>();
        // services.AddTransient<StatusBarViewModel>();

        return services;
    }
}
