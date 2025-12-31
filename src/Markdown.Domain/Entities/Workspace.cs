namespace Markdown.Domain.Entities;

public sealed class Workspace : AggregateRoot<WorkspaceId>
{
    private readonly List<Document> _documents = [];

    public FilePath RootPath { get; }

    public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();

    public Workspace(WorkspaceId id, FilePath rootPath)
    {
        Id = id;
        RootPath = rootPath;
    }

    public Result AddDocument(Document document)
    {
        if (_documents.Any(d => d.Id == document.Id || d.Path == document.Path))
        {
            return Results.Failure("Document already exists.");
        }

        _documents.Add(document);
        return Results.Success();
    }

    public Document? FindDocument(DocumentId id)
    {
        return _documents.FirstOrDefault(d => d.Id == id);
    }

    public Result RemoveDocument(DocumentId documentId)
    {
        Document? doc = FindDocument(documentId);
        if (doc is null)
        {
            return Results.Failure("Document not found.");
        }

        _ = _documents.Remove(doc);
        return Results.Success();
    }
}
