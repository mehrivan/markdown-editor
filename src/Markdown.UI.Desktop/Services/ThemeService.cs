using Avalonia.Styling;
using AvaloniaApplication = Avalonia.Application;

namespace Markdown.UI.Desktop.Services;

/// <summary>
/// Implementation of theme management using Avalonia's theming system.
/// Automatically persists theme changes to settings.
/// </summary>
internal sealed class ThemeService : IThemeService
{
    private readonly ISettingsService _settingsService;
    private ThemeMode _currentTheme;

    public ThemeService(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        // Load theme from settings
        var settings = _settingsService.Load();
        _currentTheme = ParseThemeMode(settings.Theme);

        // Apply the initial theme
        ApplyTheme(_currentTheme);
    }

    /// <inheritdoc />
    public ThemeMode CurrentTheme => _currentTheme;

    /// <inheritdoc />
    public event EventHandler<ThemeMode>? ThemeChanged;

    /// <inheritdoc />
    public void SetTheme(ThemeMode theme)
    {
        if (_currentTheme == theme)
        {
            return;
        }

        _currentTheme = theme;
        ApplyTheme(theme);
        ThemeChanged?.Invoke(this, theme);

        // Auto-persist theme change
        _ = PersistThemeAsync(theme);
    }

    /// <inheritdoc />
    public void ToggleTheme()
    {
        var effectiveTheme = GetEffectiveTheme(_currentTheme);
        var newTheme = effectiveTheme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;
        SetTheme(newTheme);
    }

    /// <inheritdoc />
    public ThemeMode GetSystemTheme()
    {
        if (AvaloniaApplication.Current is null)
        {
            return ThemeMode.Light;
        }

        var actualTheme = AvaloniaApplication.Current.ActualThemeVariant;

        return actualTheme == ThemeVariant.Dark ? ThemeMode.Dark : ThemeMode.Light;
    }

    private static void ApplyTheme(ThemeMode theme)
    {
        if (AvaloniaApplication.Current is null)
        {
            return;
        }

        AvaloniaApplication.Current.RequestedThemeVariant = theme switch
        {
            ThemeMode.Light => ThemeVariant.Light,
            ThemeMode.Dark => ThemeVariant.Dark,
            ThemeMode.System => ThemeVariant.Default,
            _ => ThemeVariant.Default
        };
    }

    private ThemeMode GetEffectiveTheme(ThemeMode theme)
    {
        if (theme == ThemeMode.System)
        {
            return GetSystemTheme();
        }

        return theme;
    }

    private async Task PersistThemeAsync(ThemeMode theme)
    {
        try
        {
            var settings = _settingsService.Load();
            settings.Theme = theme.ToString();
            await _settingsService.SaveAsync(settings).ConfigureAwait(false);
        }
        catch
        {
            // Silently ignore persistence errors
        }
    }

    private static ThemeMode ParseThemeMode(string? themeName)
    {
        return themeName?.ToUpperInvariant() switch
        {
            "LIGHT" => ThemeMode.Light,
            "DARK" => ThemeMode.Dark,
            "SYSTEM" => ThemeMode.System,
            _ => ThemeMode.System
        };
    }
}
