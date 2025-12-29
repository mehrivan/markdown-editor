namespace Markdown.Domain.Interfaces;

public interface IWorkspaceRepository {
    Task<Result<Workspace?>> GetByIdAsync(WorkspaceId id, CancellationToken ct);
    Task<Result> SaveAsync(Workspace workspace, CancellationToken ct);
}
