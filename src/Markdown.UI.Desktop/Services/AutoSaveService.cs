using System.Collections.Concurrent;
using Markdown.Domain.Primitives;

namespace Markdown.UI.Desktop.Services;

/// <summary>
/// Implementation of auto-save service with debounce functionality.
/// Uses CancellationTokenSource to manage pending save operations.
/// </summary>
internal sealed class AutoSaveService : IAutoSaveService
{
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _pendingSaves = new();
    private readonly object _lock = new();
    private bool _disposed;

    /// <inheritdoc />
    public bool IsEnabled { get; set; } = true;

    /// <inheritdoc />
    public int DelayMilliseconds { get; set; } = 3000;

    /// <inheritdoc />
    public event EventHandler<AutoSaveCompletedEventArgs>? SaveCompleted;

    /// <inheritdoc />
    public event EventHandler<AutoSaveFailedEventArgs>? SaveFailed;

    /// <inheritdoc />
    public void ScheduleSave(Guid tabId, string? filePath, Func<Task<Result>> saveAction)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(saveAction);

        // Skip if auto-save is disabled or no file path
        if (!IsEnabled || string.IsNullOrEmpty(filePath))
        {
            return;
        }

        // Cancel any existing pending save for this tab
        CancelPendingSave(tabId);

        // Create new cancellation token source
        var cts = new CancellationTokenSource();

        if (!_pendingSaves.TryAdd(tabId, cts))
        {
            cts.Dispose();
            return;
        }

        // Capture the file path for the closure
        var capturedFilePath = filePath;

        // Start the delayed save operation
        _ = ExecuteDelayedSaveAsync(tabId, capturedFilePath, saveAction, cts.Token);
    }

    /// <inheritdoc />
    public void CancelPendingSave(Guid tabId)
    {
        if (_pendingSaves.TryRemove(tabId, out var cts))
        {
            try
            {
                cts.Cancel();
            }
            finally
            {
                cts.Dispose();
            }
        }
    }

    /// <inheritdoc />
    public void CancelAll()
    {
        lock (_lock)
        {
            foreach (var kvp in _pendingSaves)
            {
                try
                {
                    kvp.Value.Cancel();
                    kvp.Value.Dispose();
                }
                catch
                {
                    // Ignore disposal errors
                }
            }

            _pendingSaves.Clear();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        CancelAll();
    }

    private async Task ExecuteDelayedSaveAsync(
        Guid tabId,
        string filePath,
        Func<Task<Result>> saveAction,
        CancellationToken cancellationToken)
    {
        try
        {
            // Wait for the debounce delay
            await Task.Delay(DelayMilliseconds, cancellationToken).ConfigureAwait(false);

            // Check if we were cancelled during the delay
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            // Execute the save action
            var result = await saveAction().ConfigureAwait(false);

            // Remove from pending saves
            _pendingSaves.TryRemove(tabId, out _);

            // Raise appropriate event
            if (result.IsSuccess)
            {
                SaveCompleted?.Invoke(this, new AutoSaveCompletedEventArgs(tabId, filePath));
            }
            else
            {
                SaveFailed?.Invoke(this, new AutoSaveFailedEventArgs(tabId, filePath, result.Error ?? "Unknown error"));
            }
        }
        catch (TaskCanceledException)
        {
            // Normal cancellation, ignore
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation, ignore
        }
        catch (Exception ex)
        {
            // Remove from pending saves
            _pendingSaves.TryRemove(tabId, out _);

            // Raise failure event
            SaveFailed?.Invoke(this, new AutoSaveFailedEventArgs(tabId, filePath, ex.Message));
        }
    }
}
