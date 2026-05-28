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

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonColumnAttribute"/> class using an options provider type.
    /// </summary>
    /// <param name="jsonOptionsProviderType">The type that provides JSON serializer options.</param>
    public JsonColumnAttribute(Type jsonOptionsProviderType)
    {
        JsonOptionsProviderType = jsonOptionsProviderType ?? throw new ArgumentNullException(nameof(jsonOptionsProviderType));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonColumnAttribute"/> class using a JSON serializer context type and type-info property name.
    /// </summary>
    /// <param name="jsonSerializerContextType">The JSON serializer context type.</param>
    /// <param name="jsonTypeInfoPropertyName">The JSON type-info property name on the context.</param>
    public JsonColumnAttribute(Type jsonSerializerContextType, string jsonTypeInfoPropertyName)
    {
        JsonSerializerContextType = jsonSerializerContextType ?? throw new ArgumentNullException(nameof(jsonSerializerContextType));
        JsonTypeInfoPropertyName = jsonTypeInfoPropertyName ?? throw new ArgumentNullException(nameof(jsonTypeInfoPropertyName));
    }

    /// <summary>
    /// Gets the type that provides JSON serializer options.
    /// </summary>
    public Type? JsonOptionsProviderType { get; }

    /// <summary>
    /// Gets the JSON serializer context type.
    /// </summary>
    public Type? JsonSerializerContextType { get; }

    /// <summary>
    /// Gets the JSON type-info property name on the context.
    /// </summary>
    public string? JsonTypeInfoPropertyName { get; }
}
