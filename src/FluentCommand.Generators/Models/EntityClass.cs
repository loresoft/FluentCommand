using FluentCommand.Generators.Internal;

namespace FluentCommand.Generators.Models;

public sealed class EntityClass : IEquatable<EntityClass>
{
    public EntityClass(
        InitializationMode initializationMode,
        string entityNamespace,
        string entityName,
        IEnumerable<EntityProperty> properties)
    {
        InitializationMode = initializationMode;
        EntityNamespace = entityNamespace;
        EntityName = entityName;
        Properties = new EquatableArray<EntityProperty>(properties);
    }

    public InitializationMode InitializationMode { get; }

    public string EntityNamespace { get; }

    public string EntityName { get; }

    public EquatableArray<EntityProperty> Properties { get; }

    public bool Equals(EntityClass other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;

        return InitializationMode == other.InitializationMode
               && EntityNamespace == other.EntityNamespace
               && EntityName == other.EntityName
               && Properties.Equals(other.Properties);
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj is EntityClass other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(InitializationMode, EntityNamespace, EntityName, Properties);
    }

    public static bool operator ==(EntityClass left, EntityClass right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(EntityClass left, EntityClass right)
    {
        return !Equals(left, right);
    }
}
