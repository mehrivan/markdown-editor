using Markdown.Domain.Interfaces;

namespace Markdown.Infrastructure.Services;

public sealed class DocumentService(IDocumentRepository documentRepository) : IDocumentService
{
    private readonly IDocumentRepository _documentRepository = documentRepository;

    public async Task<Result<Document>> OpenAsync(string filePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Results.Failure<Document>("File path cannot be empty.");
        }

        FilePath path = new(filePath);
        return await _documentRepository.GetByPathAsync(path, ct).ConfigureAwait(false);
    }

    public async Task<Result> SaveAsync(Document document, CancellationToken ct = default)
    {
        _ = Guard.NotNull(document);

        return await _documentRepository.SaveAsync(document, ct).ConfigureAwait(false);
    }
}
