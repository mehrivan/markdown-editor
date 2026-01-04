using Markdown.Domain.Primitives;

namespace Markdown.UI.Desktop.Services;

/// <summary>
/// Service for managing automatic document saving with debounce functionality.
/// </summary>
internal interface IAutoSaveService : IDisposable
{
    /// <summary>
    /// Gets or sets whether auto-save is enabled.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the delay in milliseconds before auto-save triggers after the last change.
    /// </summary>
    int DelayMilliseconds { get; set; }

    /// <summary>
    /// Schedules a save operation for the specified tab.
    /// If a save is already pending for this tab, it will be cancelled and rescheduled (debounce).
    /// </summary>
    /// <param name="tabId">The unique identifier of the tab.</param>
    /// <param name="filePath">The file path to save to. If null, no save is scheduled.</param>
    /// <param name="saveAction">The async action that performs the save and returns a Result.</param>
    void ScheduleSave(Guid tabId, string? filePath, Func<Task<Result>> saveAction);

    /// <summary>
    /// Cancels any pending save operation for the specified tab.
    /// </summary>
    /// <param name="tabId">The unique identifier of the tab.</param>
    void CancelPendingSave(Guid tabId);

    /// <summary>
    /// Cancels all pending save operations.
    /// </summary>
    void CancelAll();

    /// <summary>
    /// Raised when an auto-save operation completes successfully.
    /// </summary>
    event EventHandler<AutoSaveCompletedEventArgs>? SaveCompleted;

    /// <summary>
    /// Raised when an auto-save operation fails.
    /// </summary>
    event EventHandler<AutoSaveFailedEventArgs>? SaveFailed;
}
