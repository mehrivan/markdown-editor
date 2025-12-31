namespace Markdown.Domain.ValueObjects;

public readonly record struct DocumentId(Guid Value)
{
    public static DocumentId New()
    {
        return new(Guid.NewGuid());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
