namespace Markdown.UI.Desktop.Services;

/// <summary>
/// Event arguments for a successful auto-save operation.
/// </summary>
internal sealed class AutoSaveCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoSaveCompletedEventArgs"/> class.
    /// </summary>
    /// <param name="tabId">The ID of the tab that was saved.</param>
    /// <param name="filePath">The file path that was saved.</param>
    public AutoSaveCompletedEventArgs(Guid tabId, string filePath)
    {
        TabId = tabId;
        FilePath = filePath;
    }

    /// <summary>
    /// Gets the ID of the tab that was saved.
    /// </summary>
    public Guid TabId { get; }

    /// <summary>
    /// Gets the file path that was saved.
    /// </summary>
    public string FilePath { get; }
}

/// <summary>
/// Event arguments for a failed auto-save operation.
/// </summary>
internal sealed class AutoSaveFailedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutoSaveFailedEventArgs"/> class.
    /// </summary>
    /// <param name="tabId">The ID of the tab that failed to save.</param>
    /// <param name="filePath">The file path that failed to save.</param>
    /// <param name="error">The error message describing the failure.</param>
    public AutoSaveFailedEventArgs(Guid tabId, string filePath, string error)
    {
        TabId = tabId;
        FilePath = filePath;
        Error = error;
    }

    /// <summary>
    /// Gets the ID of the tab that failed to save.
    /// </summary>
    public Guid TabId { get; }

    /// <summary>
    /// Gets the file path that failed to save.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the error message describing the failure.
    /// </summary>
    public string Error { get; }
}
