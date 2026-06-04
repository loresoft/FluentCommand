using System.Diagnostics;

namespace FluentCommand.Attributes;

/// <summary>
/// Indicates that a property or field should be deserialized from a JSON column by the source-generated data reader.
/// </summary>
/// <remarks>
/// Apply this attribute to properties or fields whose database column contains JSON text.
/// </remarks>
[Conditional("FLUENTCOMMAND_GENERATOR")]
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class JsonColumnAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonColumnAttribute"/> class.
    /// </summary>
    public JsonColumnAttribute()
    {
    }
}
