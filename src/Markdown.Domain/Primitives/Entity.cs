namespace Markdown.Domain.Primitives;

public abstract class Entity<TId> : IEquatable<Entity<TId>> where TId : notnull
{
    public TId Id { get; protected init; } = default!;

    protected Entity() { }

    protected Entity(TId id)
        => Id = id;

    public bool Equals(Entity<TId>? other)
        => other is not null && Id.Equals(other.Id);

    public override bool Equals(object? obj)
        => obj is Entity<TId> entity && Equals(entity);

    public override int GetHashCode()
        => Id.GetHashCode();

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => left?.Equals(right) ?? (right is null);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !(left == right);
}
