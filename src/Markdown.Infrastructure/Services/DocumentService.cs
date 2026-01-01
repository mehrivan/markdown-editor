using Markdown.Application.Services;
using Markdown.Domain.Entities;
using Markdown.Domain.Interfaces;
using Markdown.Domain.Primitives;
using Markdown.Domain.ValueObjects;

namespace Markdown.Infrastructure.Services;

public sealed class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;

    public DocumentService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    public async Task<Result<Document>> OpenAsync(string filePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Results.Failure<Document>("File path cannot be empty.");
        }

        var path = new FilePath(filePath);
        return await _documentRepository.GetByPathAsync(path, ct).ConfigureAwait(false);
    }

    public async Task<Result> SaveAsync(Document document, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        return await _documentRepository.SaveAsync(document, ct).ConfigureAwait(false);
    }
}
