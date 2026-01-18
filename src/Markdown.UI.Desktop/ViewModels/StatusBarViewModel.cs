using System.Timers;

using Avalonia.Threading;

namespace Markdown.UI.Desktop.ViewModels;

/// <summary>
/// Types of feedback messages for styling purposes.
/// </summary>
public enum FeedbackType
{
    Success,
    Error,
    Info
}

/// <summary>
/// ViewModel for the status bar displaying document and application status.
/// </summary>
public sealed partial class StatusBarViewModel : ViewModelBase, IDisposable
{
    private System.Timers.Timer? _feedbackTimer;
    private bool _disposed;

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
    /// Indicates whether a loading operation is in progress.
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Message to display during loading operations.
    /// </summary>
    [ObservableProperty]
    private string _loadingMessage = string.Empty;

    /// <summary>
    /// Transient feedback message to display (success, error, info).
    /// </summary>
    [ObservableProperty]
    private string _feedbackMessage = string.Empty;

    /// <summary>
    /// Indicates whether there is a feedback message to display.
    /// </summary>
    [ObservableProperty]
    private bool _hasFeedback;

    /// <summary>
    /// Indicates whether the current feedback is an error (for styling).
    /// </summary>
    [ObservableProperty]
    private bool _isError;

    /// <summary>
    /// Display text for auto-save status.
    /// </summary>
    public string AutoSaveStatus => IsAutoSaveEnabled ? "Auto-save: On" : "Auto-save: Off";

    /// <summary>
    /// Raised when the user requests to toggle auto-save from the status bar.
    /// </summary>
    public event EventHandler? AutoSaveToggleRequested;

    /// <summary>
    /// Command to toggle auto-save on/off from the status bar.
    /// </summary>
    [RelayCommand]
    private void ToggleAutoSave()
    {
        AutoSaveToggleRequested?.Invoke(this, EventArgs.Empty);
    }

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

    /// <summary>
    /// Sets the loading state with a message.
    /// </summary>
    /// <param name="message">The loading message to display.</param>
    public void SetLoading(string message)
    {
        IsLoading = true;
        LoadingMessage = message;
    }

    /// <summary>
    /// Clears the loading state.
    /// </summary>
    public void ClearLoading()
    {
        IsLoading = false;
        LoadingMessage = string.Empty;
    }

    /// <summary>
    /// Shows a transient feedback message that auto-clears after the specified duration.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="type">The type of feedback (affects styling).</param>
    /// <param name="durationMs">Duration in milliseconds before auto-clearing (default 3000ms).</param>
    public void ShowFeedback(string message, FeedbackType type = FeedbackType.Success, int durationMs = 3000)
    {
        // Stop any existing timer
        _feedbackTimer?.Stop();
        _feedbackTimer?.Dispose();

        FeedbackMessage = message;
        HasFeedback = true;
        IsError = type == FeedbackType.Error;

        // Start a new timer to auto-clear the feedback
        _feedbackTimer = new System.Timers.Timer(durationMs);
        _feedbackTimer.Elapsed += OnFeedbackTimerElapsed;
        _feedbackTimer.AutoReset = false;
        _feedbackTimer.Start();
    }

    /// <summary>
    /// Clears any displayed feedback message.
    /// </summary>
    public void ClearFeedback()
    {
        // Must dispatch to UI thread since timer fires on a background thread
        Dispatcher.UIThread.Post(() =>
        {
            HasFeedback = false;
            FeedbackMessage = string.Empty;
            IsError = false;
        });
    }

    /// <summary>
    /// Handles the feedback timer elapsed event.
    /// </summary>
    private void OnFeedbackTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        ClearFeedback();
    }

    /// <summary>
    /// Disposes of resources used by the StatusBarViewModel.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _feedbackTimer?.Stop();
        _feedbackTimer?.Dispose();
        _feedbackTimer = null;
    }
}
