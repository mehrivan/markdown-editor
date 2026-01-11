using System.Collections.ObjectModel;

using Markdown.Application.ReadModels;
using Markdown.Application.Services;
using Markdown.Desktop.Services;
using Markdown.UI.Desktop.Models;

namespace Markdown.UI.Desktop.ViewModels;

/// <summary>
/// ViewModel for the file explorer panel, managing workspace navigation.
/// </summary>
public sealed partial class FileExplorerViewModel : ViewModelBase, IDisposable
{
    private readonly IWorkspaceExplorer _workspaceExplorer;
    private readonly IFileWatcherService _fileWatcherService;
    private bool _disposed;

    /// <summary>
    /// The root path of the current workspace.
    /// </summary>
    [ObservableProperty]
    private string? _workspacePath;

    /// <summary>
    /// The root nodes of the file tree.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<FileTreeNode> _rootNodes = [];

    /// <summary>
    /// The currently selected node in the tree.
    /// </summary>
    [ObservableProperty]
    private FileTreeNode? _selectedNode;

    /// <summary>
    /// Indicates whether the workspace is currently being loaded.
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Raised when a file should be opened in the editor.
    /// </summary>
    public event EventHandler<string>? FileOpenRequested;

    /// <summary>
    /// Creates a new FileExplorerViewModel.
    /// </summary>
    /// <param name="workspaceExplorer">The workspace explorer service.</param>
    /// <param name="fileWatcherService">The file watcher service for detecting external changes.</param>
    public FileExplorerViewModel(IWorkspaceExplorer workspaceExplorer, IFileWatcherService fileWatcherService)
    {
        _workspaceExplorer = workspaceExplorer;
        _fileWatcherService = fileWatcherService;
        _fileWatcherService.FileSystemChanged += OnFileSystemChanged;

        RefreshCommand = new AsyncRelayCommand(RefreshAsync, CanRefresh);
        ExpandFolderCommand = new AsyncRelayCommand<FileTreeNode>(ExpandFolderAsync);
        OpenFileCommand = new AsyncRelayCommand<FileTreeNode>(OpenFileAsync);
    }

    /// <summary>
    /// Command to refresh the workspace tree.
    /// </summary>
    public IAsyncRelayCommand RefreshCommand { get; }

    /// <summary>
    /// Command to expand a folder node and load its children.
    /// </summary>
    public IAsyncRelayCommand<FileTreeNode> ExpandFolderCommand { get; }

    /// <summary>
    /// Command to open a file from the tree.
    /// </summary>
    public IAsyncRelayCommand<FileTreeNode> OpenFileCommand { get; }

    /// <summary>
    /// Loads the workspace from the specified path.
    /// </summary>
    /// <param name="path">The workspace root path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task LoadWorkspaceAsync(string path, CancellationToken cancellationToken = default)
    {
        // Stop watching the previous workspace
        _fileWatcherService.StopWatching();

        WorkspacePath = path;
        await RefreshAsync(cancellationToken);

        // Start watching the new workspace
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
        {
            _fileWatcherService.StartWatching(path);
        }
    }

    /// <summary>
    /// Refreshes the workspace tree.
    /// </summary>
    private async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(WorkspacePath))
        {
            return;
        }

        IsLoading = true;
        try
        {
            RootNodes.Clear();

            // Load only the first level (depth 1)
            var entries = await _workspaceExplorer.GetEntriesAsync(
                WorkspacePath,
                maxDepth: 1,
                cancellationToken);

            foreach (var entry in entries.OrderBy(e => e.Type).ThenBy(e => e.Name))
            {
                var node = FileTreeNode.FromWorkspaceEntry(entry);

                // Add a placeholder for folders to show expand arrow
                if (node.IsFolder)
                {
                    node.Children.Add(CreateLoadingPlaceholder());
                }

                RootNodes.Add(node);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Checks if the refresh command can execute.
    /// </summary>
    private bool CanRefresh() => !string.IsNullOrEmpty(WorkspacePath) && !IsLoading;

    /// <summary>
    /// Expands a folder node and loads its children.
    /// </summary>
    /// <param name="folder">The folder node to expand.</param>
    private async Task ExpandFolderAsync(FileTreeNode? folder)
    {
        if (folder is null || !folder.IsFolder || folder.IsLoading)
        {
            return;
        }

        // Check if already loaded (no placeholder or real children exist)
        if (folder.Children.Count > 0 && !IsLoadingPlaceholder(folder.Children[0]))
        {
            return;
        }

        folder.IsLoading = true;
        try
        {
            folder.Children.Clear();

            var entries = await _workspaceExplorer.GetEntriesAsync(
                folder.FullPath,
                maxDepth: 1);

            foreach (var entry in entries.OrderBy(e => e.Type).ThenBy(e => e.Name))
            {
                var node = FileTreeNode.FromWorkspaceEntry(entry);

                // Add placeholder for subfolders
                if (node.IsFolder)
                {
                    node.Children.Add(CreateLoadingPlaceholder());
                }

                folder.Children.Add(node);
            }

            folder.IsExpanded = true;
        }
        finally
        {
            folder.IsLoading = false;
        }
    }

    /// <summary>
    /// Opens a file node in the editor.
    /// </summary>
    /// <param name="node">The file node to open.</param>
    private Task OpenFileAsync(FileTreeNode? node)
    {
        if (node is null || node.IsFolder)
        {
            return Task.CompletedTask;
        }

        FileOpenRequested?.Invoke(this, node.FullPath);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a placeholder node used to show the loading state.
    /// </summary>
    private static FileTreeNode CreateLoadingPlaceholder()
    {
        return new FileTreeNode
        {
            Name = "Loading...",
            FullPath = string.Empty,
            Type = FileEntryType.File
        };
    }

    /// <summary>
    /// Checks if a node is a loading placeholder.
    /// </summary>
    private static bool IsLoadingPlaceholder(FileTreeNode node)
    {
        return string.IsNullOrEmpty(node.FullPath) && node.Name == "Loading...";
    }

    /// <summary>
    /// Handles file system change events from the file watcher service.
    /// </summary>
    private void OnFileSystemChanged(object? sender, FileSystemChangeEventArgs e)
    {
        if (string.IsNullOrEmpty(WorkspacePath))
        {
            return;
        }

        switch (e.ChangeType)
        {
            case FileSystemChangeType.Created:
                HandleFileCreated(e.Path, e.IsDirectory);
                break;
            case FileSystemChangeType.Deleted:
                HandleFileDeleted(e.Path);
                break;
            case FileSystemChangeType.Renamed:
                HandleFileRenamed(e.OldPath!, e.Path, e.IsDirectory);
                break;
        }
    }

    /// <summary>
    /// Handles a file or folder being created.
    /// </summary>
    private void HandleFileCreated(string path, bool isDirectory)
    {
        var parentPath = Path.GetDirectoryName(path);
        if (parentPath is null)
        {
            return;
        }

        // Find the parent node
        var parentNode = FindNodeByPath(parentPath);

        // If parent is not loaded or not expanded, no need to add
        if (parentNode is null)
        {
            // Check if it's a direct child of the workspace root
            if (string.Equals(parentPath, WorkspacePath, StringComparison.OrdinalIgnoreCase))
            {
                AddNodeToCollection(RootNodes, path, isDirectory);
            }
            return;
        }

        // If parent has only a loading placeholder, don't add (will load when expanded)
        if (parentNode.Children.Count == 1 && IsLoadingPlaceholder(parentNode.Children[0]))
        {
            return;
        }

        AddNodeToCollection(parentNode.Children, path, isDirectory);
    }

    /// <summary>
    /// Adds a new node to a collection in sorted order.
    /// </summary>
    private static void AddNodeToCollection(ObservableCollection<FileTreeNode> collection, string path, bool isDirectory)
    {
        var newNode = new FileTreeNode
        {
            Name = Path.GetFileName(path),
            FullPath = path,
            Type = isDirectory ? FileEntryType.Folder : FileEntryType.File
        };

        // Add placeholder for folders
        if (isDirectory)
        {
            newNode.Children.Add(new FileTreeNode
            {
                Name = "Loading...",
                FullPath = string.Empty,
                Type = FileEntryType.File
            });
        }

        // Insert in sorted order: folders first, then alphabetically
        var insertIndex = 0;
        for (var i = 0; i < collection.Count; i++)
        {
            var existing = collection[i];
            if (IsLoadingPlaceholder(existing))
            {
                continue;
            }

            // Folders come before files
            if (isDirectory && !existing.IsFolder)
            {
                break;
            }

            // Within same type, sort alphabetically
            if (isDirectory == existing.IsFolder &&
                string.Compare(newNode.Name, existing.Name, StringComparison.OrdinalIgnoreCase) < 0)
            {
                break;
            }

            insertIndex = i + 1;
        }

        collection.Insert(insertIndex, newNode);
    }

    /// <summary>
    /// Handles a file or folder being deleted.
    /// </summary>
    private void HandleFileDeleted(string path)
    {
        var parentPath = Path.GetDirectoryName(path);
        if (parentPath is null)
        {
            return;
        }

        // Check if it's a direct child of the workspace root
        if (string.Equals(parentPath, WorkspacePath, StringComparison.OrdinalIgnoreCase))
        {
            RemoveNodeFromCollection(RootNodes, path);
            return;
        }

        // Find the parent node
        var parentNode = FindNodeByPath(parentPath);
        if (parentNode is not null)
        {
            RemoveNodeFromCollection(parentNode.Children, path);
        }
    }

    /// <summary>
    /// Removes a node from a collection by path.
    /// </summary>
    private static void RemoveNodeFromCollection(ObservableCollection<FileTreeNode> collection, string path)
    {
        for (var i = 0; i < collection.Count; i++)
        {
            if (string.Equals(collection[i].FullPath, path, StringComparison.OrdinalIgnoreCase))
            {
                collection.RemoveAt(i);
                return;
            }
        }
    }

    /// <summary>
    /// Handles a file or folder being renamed.
    /// </summary>
    private void HandleFileRenamed(string oldPath, string newPath, bool isDirectory)
    {
        var node = FindNodeByPath(oldPath);
        if (node is not null)
        {
            node.Name = Path.GetFileName(newPath);
            node.FullPath = newPath;

            // If it's a directory, update all children paths recursively
            if (isDirectory && node.Children.Count > 0 && !IsLoadingPlaceholder(node.Children[0]))
            {
                UpdateChildPaths(node, oldPath, newPath);
            }
        }
    }

    /// <summary>
    /// Recursively updates paths of all children after a parent folder rename.
    /// </summary>
    private static void UpdateChildPaths(FileTreeNode parent, string oldBasePath, string newBasePath)
    {
        foreach (var child in parent.Children)
        {
            if (IsLoadingPlaceholder(child))
            {
                continue;
            }

            // Replace the old base path with the new one
            if (child.FullPath.StartsWith(oldBasePath, StringComparison.OrdinalIgnoreCase))
            {
                child.FullPath = newBasePath + child.FullPath[oldBasePath.Length..];
            }

            // Recurse into folders
            if (child.IsFolder && child.Children.Count > 0)
            {
                UpdateChildPaths(child, oldBasePath, newBasePath);
            }
        }
    }

    /// <summary>
    /// Finds a node in the tree by its full path.
    /// </summary>
    private FileTreeNode? FindNodeByPath(string path)
    {
        if (string.IsNullOrEmpty(WorkspacePath))
        {
            return null;
        }

        // Get relative path from workspace root
        if (!path.StartsWith(WorkspacePath, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // If it's the workspace root itself, return null (no node for root)
        if (string.Equals(path, WorkspacePath, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var relativePath = path[WorkspacePath.Length..].TrimStart(Path.DirectorySeparatorChar);
        var pathParts = relativePath.Split(Path.DirectorySeparatorChar);

        var currentCollection = RootNodes;
        FileTreeNode? currentNode = null;

        foreach (var part in pathParts)
        {
            currentNode = null;
            foreach (var node in currentCollection)
            {
                if (string.Equals(node.Name, part, StringComparison.OrdinalIgnoreCase))
                {
                    currentNode = node;
                    break;
                }
            }

            if (currentNode is null)
            {
                return null;
            }

            currentCollection = currentNode.Children;
        }

        return currentNode;
    }

    /// <summary>
    /// Disposes the view model and stops file watching.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _fileWatcherService.FileSystemChanged -= OnFileSystemChanged;
        _fileWatcherService.StopWatching();
    }
}
