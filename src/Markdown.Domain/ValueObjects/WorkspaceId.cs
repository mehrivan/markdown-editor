namespace Markdown.Domain.ValueObjects;

public readonly record struct WorkspaceId(Guid Value)
{
    public static WorkspaceId New()
    {
        return new(Guid.NewGuid());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
