namespace Markdown.UI.Desktop.Models;

/// <summary>
/// Application settings that are persisted across sessions.
/// </summary>
internal sealed class AppSettings
{
    /// <summary>
    /// The width of the main window in pixels.
    /// </summary>
    public double WindowWidth { get; set; } = 1200;

    /// <summary>
    /// The height of the main window in pixels.
    /// </summary>
    public double WindowHeight { get; set; } = 800;

    /// <summary>
    /// The X position of the main window.
    /// </summary>
    public double WindowX { get; set; }

    /// <summary>
    /// The Y position of the main window.
    /// </summary>
    public double WindowY { get; set; }

    /// <summary>
    /// Whether the sidebar (file explorer) is visible.
    /// </summary>
    public bool IsSidebarVisible { get; set; } = true;

    /// <summary>
    /// The width of the sidebar in pixels.
    /// </summary>
    public double SidebarWidth { get; set; } = 250;

    /// <summary>
    /// The current theme: "Light", "Dark", or "System".
    /// </summary>
    public string Theme { get; set; } = "System";

    /// <summary>
    /// Whether auto-save is enabled.
    /// </summary>
    public bool AutoSaveEnabled { get; set; } = true;

    /// <summary>
    /// The delay in milliseconds before auto-save triggers after the last keystroke.
    /// </summary>
    public int AutoSaveDelayMs { get; set; } = 3000;

    /// <summary>
    /// The path to the last opened workspace folder.
    /// </summary>
    public string? LastWorkspacePath { get; set; }
}
