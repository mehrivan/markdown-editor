using Markdown.Application.ReadModels;

namespace Markdown.Application.Services;

/// <summary>
/// Application service for exploring workspace file structure.
/// </summary>
public interface IWorkspaceExplorer {
    /// <summary>
    /// Get all entries in a workspace directory.
    /// </summary>
    Task<IEnumerable<WorkspaceEntry>> GetEntriesAsync(
        string rootPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get only markdown files in a workspace.
    /// </summary>
    Task<IEnumerable<WorkspaceEntry>> GetMarkdownFilesAsync(
        string rootPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get entries with optional depth limit.
    /// </summary>
    Task<IEnumerable<WorkspaceEntry>> GetEntriesAsync(
        string rootPath,
        int maxDepth,
        CancellationToken cancellationToken = default);
}
