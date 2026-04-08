using System.Diagnostics;

namespace FluentCommand.Attributes;

/// <summary>
/// Indicates that a property or field should be ignored by the source generator when creating a data reader.
/// </summary>
/// <remarks>
/// Apply this attribute directly to a property or field to exclude it from data reader generation.
/// When applied to a class, <see cref="PropertyName"/> is required to identify which property or field to ignore.
/// The attribute can be applied multiple times to a class to ignore multiple members.
/// </remarks>
/// <example>
/// Applied directly to a property:
/// <code>
/// [IgnoreProperty]
/// public string InternalNotes { get; set; }
/// </code>
/// Applied to a class with a required property name:
/// <code>
/// [IgnoreProperty(nameof(Product.InternalNotes))]
/// public class Product { ... }
/// </code>
/// </example>
/// <param name="propertyName">
/// The name of the property or field to ignore. Required when the attribute is applied to a class;
/// optional when applied directly to a property or field.
/// </param>
[Conditional("FLUENTCOMMAND_GENERATOR")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class IgnorePropertyAttribute(string? propertyName = null) : Attribute
{
    /// <summary>
    /// Gets or sets the name of the property or field to ignore.
    /// </summary>
    public string? PropertyName { get; set; } = propertyName;
}
