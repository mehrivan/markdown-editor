using System.Collections.ObjectModel;

using Markdown.Application.ReadModels;
using Markdown.Application.Services;
using Markdown.UI.Desktop.Models;

namespace Markdown.UI.Desktop.ViewModels;

/// <summary>
/// ViewModel for the file explorer panel, managing workspace navigation.
/// </summary>
internal sealed partial class FileExplorerViewModel : ViewModelBase
{
    private readonly IWorkspaceExplorer _workspaceExplorer;

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
    public FileExplorerViewModel(IWorkspaceExplorer workspaceExplorer)
    {
        _workspaceExplorer = workspaceExplorer;

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
        WorkspacePath = path;
        await RefreshAsync(cancellationToken);
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
}
