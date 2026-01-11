namespace Markdown.Desktop.Services;

/// <summary>
/// Service for watching file system changes in a directory.
/// </summary>
public interface IFileWatcherService : IDisposable
{
    /// <summary>
    /// Starts watching the specified directory for file system changes.
    /// </summary>
    /// <param name="path">The directory path to watch.</param>
    void StartWatching(string path);

    /// <summary>
    /// Stops watching the current directory.
    /// </summary>
    void StopWatching();

    /// <summary>
    /// Gets a value indicating whether the service is currently watching a directory.
    /// </summary>
    bool IsWatching { get; }

    /// <summary>
    /// Gets the path currently being watched, or null if not watching.
    /// </summary>
    string? WatchedPath { get; }

    /// <summary>
    /// Raised when a file system change is detected.
    /// </summary>
    event EventHandler<FileSystemChangeEventArgs>? FileSystemChanged;
}

/// <summary>
/// Event arguments for file system change events.
/// </summary>
public sealed class FileSystemChangeEventArgs : EventArgs
{
    /// <summary>
    /// Gets the full path of the affected file or directory.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Gets the old path for rename operations, or null for other operations.
    /// </summary>
    public required string? OldPath { get; init; }

    /// <summary>
    /// Gets the type of change that occurred.
    /// </summary>
    public required FileSystemChangeType ChangeType { get; init; }

    /// <summary>
    /// Gets a value indicating whether the affected item is a directory.
    /// </summary>
    public required bool IsDirectory { get; init; }
}

/// <summary>
/// Specifies the type of file system change.
/// </summary>
public enum FileSystemChangeType
{
    /// <summary>
    /// A file or directory was created.
    /// </summary>
    Created,

    /// <summary>
    /// A file or directory was deleted.
    /// </summary>
    Deleted,

    /// <summary>
    /// A file or directory was renamed.
    /// </summary>
    Renamed
}
