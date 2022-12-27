using FluentCommand.Generators.Internal;

using Microsoft.CodeAnalysis;

namespace FluentCommand.Generators.Models;

public class EntityContext : IEquatable<EntityContext>
{
    public EntityContext(
        EntityClass entityClass,
        IEnumerable<Diagnostic> diagnostics)
    {
        EntityClass = entityClass;
        Diagnostics = new EquatableArray<Diagnostic>(diagnostics);
    }

    public EntityClass EntityClass { get; }

    public EquatableArray<Diagnostic> Diagnostics { get; }

    public bool Equals(EntityContext other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Equals(EntityClass, other.EntityClass)
               && Diagnostics.Equals(other.Diagnostics);
    }

    public override bool Equals(object obj)
    {
        return obj is EntityContext context && Equals(context);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(EntityClass, Diagnostics);
    }

    public static bool operator ==(EntityContext left, EntityContext right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(EntityContext left, EntityContext right)
    {
        return !Equals(left, right);
    }
}
