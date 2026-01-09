using System.Collections.Concurrent;
using Avalonia.Threading;

namespace Markdown.Desktop.Services;

/// <summary>
/// Implementation of <see cref="IFileWatcherService"/> using <see cref="FileSystemWatcher"/>.
/// Includes debouncing to handle rapid file system events.
/// </summary>
internal sealed class FileWatcherService : IFileWatcherService
{
    private const int _debounceDelayMs = 300;

    private FileSystemWatcher? _watcher;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _pendingEvents = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();
    private bool _disposed;

    /// <inheritdoc />
    public bool IsWatching => _watcher is { EnableRaisingEvents: true };

    /// <inheritdoc />
    public string? WatchedPath => _watcher?.Path;

    /// <inheritdoc />
    public event EventHandler<FileSystemChangeEventArgs>? FileSystemChanged;

    /// <inheritdoc />
    public void StartWatching(string path)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        }

        lock (_lock)
        {
            StopWatchingInternal();

            _watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
            _watcher.Error += OnError;
        }
    }

    /// <inheritdoc />
    public void StopWatching()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_lock)
        {
            StopWatchingInternal();
        }
    }

    private void StopWatchingInternal()
    {
        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= OnCreated;
            _watcher.Deleted -= OnDeleted;
            _watcher.Renamed -= OnRenamed;
            _watcher.Error -= OnError;
            _watcher.Dispose();
            _watcher = null;
        }

        CancelAllPendingEvents();
    }

    private void CancelAllPendingEvents()
    {
        foreach (var cts in _pendingEvents.Values)
        {
            cts.Cancel();
            cts.Dispose();
        }
        _pendingEvents.Clear();
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        ScheduleEvent(e.FullPath, null, FileSystemChangeType.Created, Directory.Exists(e.FullPath));
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        // For deleted items, we can't check if it was a directory since it no longer exists
        // We'll need to infer from the path (no extension usually means directory)
        var isDirectory = !Path.HasExtension(e.FullPath);
        ScheduleEvent(e.FullPath, null, FileSystemChangeType.Deleted, isDirectory);
    }

    private void OnRenamed(object sender, RenamedEventArgs e)
    {
        ScheduleEvent(e.FullPath, e.OldFullPath, FileSystemChangeType.Renamed, Directory.Exists(e.FullPath));
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        // FileSystemWatcher can overflow its internal buffer on rapid changes
        // In this case, we could trigger a full refresh, but for now we just log/ignore
        System.Diagnostics.Debug.WriteLine($"FileSystemWatcher error: {e.GetException().Message}");
    }

    private void ScheduleEvent(string path, string? oldPath, FileSystemChangeType changeType, bool isDirectory)
    {
        if (_disposed)
        {
            return;
        }

        // Create a unique key for this event
        var key = $"{changeType}:{path}";

        // Cancel any existing pending event for this key
        if (_pendingEvents.TryRemove(key, out var existingCts))
        {
            existingCts.Cancel();
            existingCts.Dispose();
        }

        var cts = new CancellationTokenSource();
        _pendingEvents[key] = cts;

        _ = DebounceAndRaiseEventAsync(key, path, oldPath, changeType, isDirectory, cts.Token);
    }

    private async Task DebounceAndRaiseEventAsync(
        string key,
        string path,
        string? oldPath,
        FileSystemChangeType changeType,
        bool isDirectory,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(_debounceDelayMs, cancellationToken);

            // Remove from pending events
            _pendingEvents.TryRemove(key, out _);

            if (_disposed)
            {
                return;
            }

            var args = new FileSystemChangeEventArgs
            {
                Path = path,
                OldPath = oldPath,
                ChangeType = changeType,
                IsDirectory = isDirectory
            };

            // Raise event on UI thread
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                FileSystemChanged?.Invoke(this, args);
            });
        }
        catch (OperationCanceledException)
        {
            // Event was cancelled due to newer event for same path, ignore
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error raising file system event: {ex.Message}");
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

        lock (_lock)
        {
            StopWatchingInternal();
        }
    }
}
