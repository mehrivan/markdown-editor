using Markdown.Application.ReadModels;

namespace Markdown.Application.Services;

public interface IWorkspaceExplorer
{
    Task<IEnumerable<WorkspaceEntry>> GetEntriesAsync(
        string rootPath,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<WorkspaceEntry>> GetEntriesAsync(
        string rootPath,
        int maxDepth,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<WorkspaceEntry>> GetMarkdownFilesAsync(
        string rootPath,
        CancellationToken cancellationToken = default);
}
