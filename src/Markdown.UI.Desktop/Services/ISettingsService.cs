namespace Markdown.UI.Desktop.Services;

/// <summary>
/// Service for loading and saving application settings.
/// </summary>
internal interface ISettingsService
{
    /// <summary>
    /// Gets the full path to the settings file.
    /// </summary>
    string SettingsFilePath { get; }

    /// <summary>
    /// Loads the application settings from disk.
    /// Returns default settings if the file doesn't exist or is corrupted.
    /// </summary>
    AppSettings Load();

    /// <summary>
    /// Saves the application settings to disk asynchronously.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    Task SaveAsync(AppSettings settings);
}
