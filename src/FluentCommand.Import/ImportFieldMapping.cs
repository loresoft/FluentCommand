using System.Text.Json.Serialization;

namespace FluentCommand.Import;

/// <summary>
/// Represents the association between a field definition and its mapping information for an import operation.
/// </summary>
public class ImportFieldMapping
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImportFieldMapping"/> class
    /// with the specified <see cref="FieldDefinition"/> and <see cref="FieldMap"/>.
    /// </summary>
    /// <param name="fieldDefinition">The <see cref="FieldDefinition"/> describing the field's configuration.</param>
    /// <param name="fieldMap">The <see cref="FieldMap"/> containing mapping information, or <c>null</c> if not mapped.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldDefinition"/> is <c>null</c>.</exception>
    [JsonConstructor]
    public ImportFieldMapping(FieldDefinition fieldDefinition, FieldMap? fieldMap = null)
    {
        Definition = fieldDefinition ?? throw new ArgumentNullException(nameof(fieldDefinition));
        FieldMap = fieldMap;
    }

    /// <summary>
    /// Gets the <see cref="FieldDefinition"/> describing the configuration and rules for the field.
    /// </summary>
    /// <value>
    /// The field definition.
    /// </value>
    [JsonPropertyName("definition")]
    public FieldDefinition Definition { get; }

    /// <summary>
    /// Gets the <see cref="FieldMap"/> containing mapping information for the field, or <c>null</c> if not mapped.
    /// </summary>
    /// <value>
    /// The field mapping.
    /// </value>
    [JsonPropertyName("fieldMap")]
    public FieldMap? FieldMap { get; }
}
