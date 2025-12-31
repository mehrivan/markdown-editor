namespace Markdown.Domain.Entities;

public sealed class Document : AggregateRoot<DocumentId>
{
    public FilePath Path { get; private set; }
    public MarkdownContent Content { get; private set; }

    public DateTime LastModifiedUtc { get; private set; }

    public Document(
        DocumentId id,
        FilePath path,
        MarkdownContent content,
        DateTime lastModifiedUtc)
    {
        Id = id;
        Path = path;
        Content = content;
        LastModifiedUtc = lastModifiedUtc;
    }

    public Result UpdateContent(MarkdownContent newContent, DateTime utcNow)
    {
        if (newContent == Content)
        {
            return Results.Success();
        }

        Content = newContent;
        LastModifiedUtc = utcNow;
        return Results.Success();
    }

    public Result Move(FilePath newPath)
    {
        Path = newPath;
        return Results.Success();
    }
}