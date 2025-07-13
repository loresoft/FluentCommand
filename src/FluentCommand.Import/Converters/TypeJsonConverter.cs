using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FluentCommand.Import.Converters;

/// <summary>
/// A JSON converter for serializing and deserializing <see cref="Type"/> objects using assembly-qualified names.
/// </summary>
public class TypeJsonConverter : JsonConverter<Type>
{
    /// <summary>
    /// Reads and converts the JSON string to a <see cref="Type"/> instance.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The <see cref="Type"/> instance, or <c>null</c> if not found.</returns>
    public override Type? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var typeName = reader.GetString();
        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        // Try to resolve the type using assembly-qualified name first
        var type = Type.GetType(typeName, throwOnError: false);
        if (type != null)
            return type;

        // Try to resolve by iterating loaded assemblies (for non-assembly-qualified names)
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName, throwOnError: false);
            if (type != null)
                return type;
        }

        return null;
    }

    /// <summary>
    /// Writes the <see cref="Type"/> instance as an assembly-qualified name.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="value">The value.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
    {
        // Use AssemblyQualifiedName for more robust deserialization
        writer.WriteStringValue(value.AssemblyQualifiedName);
    }
}
