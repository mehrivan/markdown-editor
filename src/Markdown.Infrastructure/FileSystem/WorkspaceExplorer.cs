namespace Markdown.Infrastructure.FileSystem;

public sealed class WorkspaceExplorer : IWorkspaceExplorer
{
    public async Task<IEnumerable<WorkspaceEntry>> GetEntriesAsync(
        string rootPath,
        CancellationToken cancellationToken = default
    )
    {
        return await GetEntriesAsync(rootPath, maxDepth: int.MaxValue, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<WorkspaceEntry>> GetEntriesAsync(
        string rootPath,
        int maxDepth,
        CancellationToken cancellationToken = default
    )
    {
        if (!Directory.Exists(rootPath))
        {
            return [];
        }

        List<WorkspaceEntry> result = [];

        await Task.Run(
            () =>
            {
                Traverse(new DirectoryInfo(rootPath), depth: 0);
            },
            cancellationToken
        ).ConfigureAwait(false);

        return result;

        void Traverse(DirectoryInfo dir, int depth)
        {
            if (depth > maxDepth || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            foreach (DirectoryInfo directory in dir.GetDirectories())
            {
                result.Add(WorkspaceEntry.FromDirectoryInfo(directory));
                Traverse(directory, depth + 1);
            }

            foreach (FileInfo file in dir.GetFiles())
            {
                result.Add(WorkspaceEntry.FromFileInfo(file));
            }
        }
    }

    public async Task<IEnumerable<WorkspaceEntry>> GetMarkdownFilesAsync(
    string rootPath,
    CancellationToken cancellationToken = default
    )
    {
        IEnumerable<WorkspaceEntry> entries = await GetEntriesAsync(rootPath, maxDepth: int.MaxValue, cancellationToken).ConfigureAwait(false);

        return entries.Where(e => e.IsMarkdownFile);
    }
}
