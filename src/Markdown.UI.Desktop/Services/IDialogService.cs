using Avalonia;

namespace Markdown.UI.Desktop.Services;

/// <summary>
/// Service for displaying native file dialogs and message boxes.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows an open file dialog.
    /// </summary>
    /// <param name="visual">The visual to get the TopLevel from.</param>
    /// <param name="title">The dialog title.</param>
    /// <param name="filters">File filters in format "Display Name|*.ext;*.ext2".</param>
    /// <returns>The selected file path, or null if cancelled.</returns>
    Task<string?> ShowOpenFileDialogAsync(Visual visual, string title, string[] filters);

    /// <summary>
    /// Shows a save file dialog.
    /// </summary>
    /// <param name="visual">The visual to get the TopLevel from.</param>
    /// <param name="title">The dialog title.</param>
    /// <param name="filters">File filters in format "Display Name|*.ext;*.ext2".</param>
    /// <param name="defaultFileName">The default file name to suggest.</param>
    /// <returns>The selected file path, or null if cancelled.</returns>
    Task<string?> ShowSaveFileDialogAsync(Visual visual, string title, string[] filters, string? defaultFileName);

    /// <summary>
    /// Shows a folder picker dialog.
    /// </summary>
    /// <param name="visual">The visual to get the TopLevel from.</param>
    /// <param name="title">The dialog title.</param>
    /// <returns>The selected folder path, or null if cancelled.</returns>
    Task<string?> ShowOpenFolderDialogAsync(Visual visual, string title);

    /// <summary>
    /// Shows a confirmation dialog with Yes/No options.
    /// </summary>
    /// <param name="visual">The visual to get the TopLevel from.</param>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The message to display.</param>
    /// <returns>True if user clicked Yes, false otherwise.</returns>
    Task<bool> ShowConfirmationDialogAsync(Visual visual, string title, string message);

    /// <summary>
    /// Shows an error message dialog.
    /// </summary>
    /// <param name="visual">The visual to get the TopLevel from.</param>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The error message to display.</param>
    Task ShowErrorDialogAsync(Visual visual, string title, string message);

    /// <summary>
    /// Shows an informational message dialog.
    /// </summary>
    /// <param name="visual">The visual to get the TopLevel from.</param>
    /// <param name="title">The dialog title.</param>
    /// <param name="message">The message to display.</param>
    Task ShowInfoDialogAsync(Visual visual, string title, string message);
}
