using FluentCommand.Generators.Internal;

namespace FluentCommand.Generators.Models;

public sealed class EntityProperty : IEquatable<EntityProperty>
{
    public EntityProperty(
        string propertyName,
        string columnName,
        string propertyType,
        string parameterName = null,
        string converterName = null)
    {
        PropertyName = propertyName;
        ColumnName = columnName;
        PropertyType = propertyType;
        ParameterName = parameterName;
        ConverterName = converterName;
    }

    public string PropertyName { get; }

    public string ColumnName { get; }

    public string PropertyType { get; }

    public string ParameterName { get; }

    public string ConverterName { get; }

    public bool Equals(EntityProperty other)
    {
        if (ReferenceEquals(null, other))
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return PropertyName == other.PropertyName
               && PropertyType == other.PropertyType
               && ParameterName == other.ParameterName
               && ConverterName == other.ConverterName;
    }

    public override bool Equals(object obj)
    {
        return obj is EntityProperty property && Equals(property);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PropertyName, PropertyType, ParameterName, ConverterName);
    }

    public static bool operator ==(EntityProperty left, EntityProperty right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(EntityProperty left, EntityProperty right)
    {
        return !Equals(left, right);
    }
}
