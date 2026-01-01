namespace Markdown.Domain.Interfaces;

public interface IDocumentRepository
{
    Task<Result<Document?>> GetByIdAsync(DocumentId id, CancellationToken ct);
    Task<Result<Document>> GetByPathAsync(FilePath path, CancellationToken ct);
    Task<Result> SaveAsync(Document document, CancellationToken ct);
    Task<Result> DeleteAsync(DocumentId id, CancellationToken ct);
}
