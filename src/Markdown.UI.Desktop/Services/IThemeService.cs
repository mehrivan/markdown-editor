namespace Markdown.UI.Desktop.Services;

/// <summary>
/// Service for managing application theme (Light/Dark/System).
/// </summary>
internal interface IThemeService
{
    /// <summary>
    /// Gets the current theme mode.
    /// </summary>
    ThemeMode CurrentTheme { get; }

    /// <summary>
    /// Sets the application theme.
    /// </summary>
    /// <param name="theme">The theme mode to apply.</param>
    void SetTheme(ThemeMode theme);

    /// <summary>
    /// Toggles between Light and Dark themes.
    /// If current theme is System, switches to Dark.
    /// </summary>
    void ToggleTheme();

    /// <summary>
    /// Gets the system's current theme preference.
    /// </summary>
    /// <returns>Light or Dark based on system settings.</returns>
    ThemeMode GetSystemTheme();

    /// <summary>
    /// Raised when the theme changes.
    /// </summary>
    event EventHandler<ThemeMode>? ThemeChanged;
}

/// <summary>
/// Represents the application theme mode.
/// </summary>
internal enum ThemeMode
{
    /// <summary>
    /// Light theme with bright backgrounds.
    /// </summary>
    Light,

    /// <summary>
    /// Dark theme with dark backgrounds.
    /// </summary>
    Dark,

    /// <summary>
    /// Follow the system's theme preference.
    /// </summary>
    System
}
