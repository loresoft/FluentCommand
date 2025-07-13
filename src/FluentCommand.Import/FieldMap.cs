using System.Text.Json.Serialization;

namespace FluentCommand.Import;

/// <summary>
/// Represents a mapping between an import source field and its target field definition.
/// </summary>
public class FieldMap
{
    /// <summary>
    /// Gets or sets the zero-based index of the field in the import source.
    /// </summary>
    /// <value>
    /// The field index, or <c>null</c> if not specified.
    /// </value>
    [JsonPropertyName("index")]
    public int? Index { get; set; }

    /// <summary>
    /// Gets or sets the name of the field being mapped.
    /// </summary>
    /// <value>
    /// The name of the field.
    /// </value>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Returns a string that represents the current <see cref="FieldMap"/> instance,
    /// including the field name and index.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> representation of this <see cref="FieldMap"/>.
    /// </returns>
    public override string ToString()
    {
        return $"Name: {Name}, Index: {Index}";
    }
}
