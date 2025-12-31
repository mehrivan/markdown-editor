namespace Markdown.Domain.ValueObjects;

public sealed record FilePath
{
    public string Value { get; }

    public FilePath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("File path cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public override string ToString()
    {
        return Value;
    }
}
