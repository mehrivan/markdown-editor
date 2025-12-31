using Markdown.Domain.Interfaces;

namespace Markdown.Infrastructure.FileSystem;

public sealed class FileDocumentRepository : IDocumentRepository
{
    public Task<Result<Document?>> GetByIdAsync(DocumentId id, CancellationToken ct)
    {
        return Task.FromResult(Results.Success<Document?>(null));
    }

    public async Task<Result> SaveAsync(Document document, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(document);

        try
        {
            string path = document.Path.Value;
            _ = Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            await File.WriteAllTextAsync(path, document.Content.Value, ct).ConfigureAwait(false);

            return Results.Success();
        }
        catch (Exception ex)
        {
            return Results.Failure($"Failed to save document: {ex.Message}");
        }
    }

    public Task<Result> DeleteAsync(DocumentId id, CancellationToken ct)
    {
        return Task.FromResult(Results.Failure("Delete is not supported yet."));
    }
}
