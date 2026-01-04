using System.Text.Json;

namespace Markdown.UI.Desktop.Services;

/// <summary>
/// JSON-based implementation of settings persistence.
/// Settings are stored in the user's application data folder.
/// </summary>
internal sealed class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _settingsFilePath;

    public SettingsService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var settingsDirectory = Path.Combine(appDataPath, "Mehrivan", "MarkdownEditor");
        _settingsFilePath = Path.Combine(settingsDirectory, "settings.json");
    }

    /// <inheritdoc />
    public string SettingsFilePath => _settingsFilePath;

    /// <inheritdoc />
    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                return new AppSettings();
            }

            var json = File.ReadAllText(_settingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);

            return settings ?? new AppSettings();
        }
        catch (JsonException)
        {
            // Settings file is corrupted, return defaults
            return new AppSettings();
        }
        catch (IOException)
        {
            // File access error, return defaults
            return new AppSettings();
        }
    }

    /// <inheritdoc />
    public async Task SaveAsync(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            await File.WriteAllTextAsync(_settingsFilePath, json).ConfigureAwait(false);
        }
        catch (IOException)
        {
            // Failed to save settings, silently ignore
            // In a production app, this could be logged
        }
        catch (UnauthorizedAccessException)
        {
            // No write permission, silently ignore
        }
    }
}
