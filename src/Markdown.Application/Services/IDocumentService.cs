using Markdown.Domain.Entities;
using Markdown.Domain.Primitives;

namespace Markdown.Application.Services;

public interface IDocumentService
{
    Task<Result<Document>> OpenAsync(string filePath, CancellationToken ct = default);
    Task<Result> SaveAsync(Document document, CancellationToken ct = default);
}
