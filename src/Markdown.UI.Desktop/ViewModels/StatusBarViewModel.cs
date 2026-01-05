namespace Markdown.UI.Desktop.ViewModels;

/// <summary>
/// ViewModel for the status bar displaying document and application status.
/// </summary>
internal sealed partial class StatusBarViewModel : ViewModelBase
{
    /// <summary>
    /// Current cursor line position (1-based).
    /// </summary>
    [ObservableProperty]
    private int _line = 1;

    /// <summary>
    /// Current cursor column position (1-based).
    /// </summary>
    [ObservableProperty]
    private int _column = 1;

    /// <summary>
    /// Total number of lines in the document.
    /// </summary>
    [ObservableProperty]
    private int _totalLines;

    /// <summary>
    /// File encoding (e.g., "UTF-8").
    /// </summary>
    [ObservableProperty]
    private string _encoding = "UTF-8";

    /// <summary>
    /// Indicates whether auto-save is enabled.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AutoSaveStatus))]
    private bool _isAutoSaveEnabled = true;

    /// <summary>
    /// Indicates whether the current document has unsaved changes.
    /// </summary>
    [ObservableProperty]
    private bool _isModified;

    /// <summary>
    /// Display text for auto-save status.
    /// </summary>
    public string AutoSaveStatus => IsAutoSaveEnabled ? "Auto-save: On" : "Auto-save: Off";

    /// <summary>
    /// Updates the cursor position and line count from the active editor.
    /// </summary>
    /// <param name="line">Current line number (1-based).</param>
    /// <param name="column">Current column number (1-based).</param>
    /// <param name="totalLines">Total number of lines in the document.</param>
    public void UpdateFromEditor(int line, int column, int totalLines)
    {
        Line = line;
        Column = column;
        TotalLines = totalLines;
    }

    /// <summary>
    /// Updates the modified state indicator.
    /// </summary>
    /// <param name="isModified">Whether the document has unsaved changes.</param>
    public void UpdateModifiedState(bool isModified)
    {
        IsModified = isModified;
    }

    /// <summary>
    /// Updates the auto-save status display.
    /// </summary>
    /// <param name="enabled">Whether auto-save is enabled.</param>
    public void UpdateAutoSaveStatus(bool enabled)
    {
        IsAutoSaveEnabled = enabled;
    }

    /// <summary>
    /// Resets the status bar to default values (for when no document is open).
    /// </summary>
    public void Reset()
    {
        Line = 1;
        Column = 1;
        TotalLines = 0;
        IsModified = false;
    }
}
