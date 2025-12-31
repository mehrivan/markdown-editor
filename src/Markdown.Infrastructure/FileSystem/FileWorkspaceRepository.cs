using Markdown.Domain.Interfaces;

namespace Markdown.Infrastructure.FileSystem;

public sealed class FileWorkspaceRepository : IWorkspaceRepository
{
    public Task<Result<Workspace?>> GetByIdAsync(WorkspaceId id, CancellationToken ct)
    {
        return Task.FromResult(Results.Success<Workspace?>(null));
    }

    public Task<Result> SaveAsync(Workspace workspace, CancellationToken ct)
    {
        return Task.FromResult(Results.Success());
    }
}
