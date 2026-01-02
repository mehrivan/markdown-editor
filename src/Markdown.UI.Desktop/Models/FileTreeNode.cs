using System.Collections.ObjectModel;

using Markdown.Application.ReadModels;

namespace Markdown.UI.Desktop.Models;

/// <summary>
/// Observable model representing a node in the file explorer tree view.
/// </summary>
internal sealed partial class FileTreeNode : ObservableObject
{
    /// <summary>
    /// The display name of the file or folder.
    /// </summary>
    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary>
    /// The full path to the file or folder.
    /// </summary>
    [ObservableProperty]
    private string _fullPath = string.Empty;

    /// <summary>
    /// The type of entry (File or Folder).
    /// </summary>
    [ObservableProperty]
    private FileEntryType _type;

    /// <summary>
    /// Whether this folder node is expanded in the tree view.
    /// </summary>
    [ObservableProperty]
    private bool _isExpanded;

    /// <summary>
    /// Whether the node's children are currently being loaded.
    /// </summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// The child nodes of this folder.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<FileTreeNode> _children = [];

    /// <summary>
    /// Indicates whether this node represents a markdown file (.md or .markdown).
    /// </summary>
    public bool IsMarkdownFile => Type == FileEntryType.File &&
        (Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase) ||
         Name.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Indicates whether this node represents a folder.
    /// </summary>
    public bool IsFolder => Type == FileEntryType.Folder;

    /// <summary>
    /// Creates a FileTreeNode from a WorkspaceEntry.
    /// </summary>
    /// <param name="entry">The workspace entry to convert.</param>
    /// <returns>A new FileTreeNode representing the entry.</returns>
    public static FileTreeNode FromWorkspaceEntry(WorkspaceEntry entry)
    {
        return new FileTreeNode
        {
            Name = entry.Name,
            FullPath = entry.Path,
            Type = entry.Type
        };
    }
}
