namespace StudentManagement.Domain.Common;

public abstract class Entity
{
    public virtual int Id { get; protected set; }

    /// <summary>Optimistic concurrency token, managed by the persistence layer.</summary>
    public virtual int Version { get; protected set; }

    public virtual bool IsTransient => Id == 0;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        // An unsaved entity has no identity yet, so it is only equal to itself.
        if (IsTransient || other.IsTransient) return false;
        return Id == other.Id && GetUnproxiedType() == other.GetUnproxiedType();
    }

    public override int GetHashCode() =>
        IsTransient ? base.GetHashCode() : HashCode.Combine(GetUnproxiedType(), Id);

    // Lazy-loading proxies are runtime subclasses that forward virtual calls to the real
    // instance, so this yields the mapped type where GetType() would yield the proxy type.
    protected virtual Type GetUnproxiedType() => GetType();
}
