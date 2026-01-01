using Markdown.Domain.Interfaces;

namespace Markdown.Infrastructure.FileSystem;

public sealed class FileDocumentRepository : IDocumentRepository
{
    public Task<Result<Document?>> GetByIdAsync(DocumentId id, CancellationToken ct)
    {
        return Task.FromResult(Results.Success<Document?>(null));
    }

    public async Task<Result<Document>> GetByPathAsync(FilePath path, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(path);

        try
        {
            string filePath = path.Value;

            if (!File.Exists(filePath))
            {
                return Results.Failure<Document>($"File not found: {filePath}");
            }

            string content = await File.ReadAllTextAsync(filePath, ct).ConfigureAwait(false);
            DateTime lastModified = File.GetLastWriteTimeUtc(filePath);

            var document = new Document(
                DocumentId.New(),
                path,
                new MarkdownContent(content),
                lastModified);

            return Results.Success(document);
        }
        catch (Exception ex)
        {
            return Results.Failure<Document>($"Failed to open document: {ex.Message}");
        }
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
