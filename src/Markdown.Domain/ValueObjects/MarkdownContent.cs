namespace Markdown.Domain.ValueObjects;

public sealed record MarkdownContent
{
    public string Value { get; }

    public MarkdownContent(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool IsEmpty
        => string.IsNullOrEmpty(Value);

    public override string ToString()
    {
        return Value;
    }
}
