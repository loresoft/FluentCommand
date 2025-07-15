using System.Text.Json.Serialization;

namespace FluentCommand.Import;

/// <summary>
/// Represents the definition and configuration of a field for import operations, including metadata, data type, mapping, and transformation options.
/// </summary>
public class FieldDefinition
{
    /// <summary>
    /// Gets or sets the display name of the field, typically used for user interfaces or reporting.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier or name of the field.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the .NET <see cref="Type"/> representing the data type of the field.
    /// </summary>
    [JsonPropertyName("dataType")]
    [JsonConverter(typeof(Converters.TypeJsonConverter))]
    public Type? DataType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this field is a key field, used to uniquely identify records.
    /// </summary>
    /// <value>
    /// <c>true</c> if this field is a key; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("isKey")]
    public bool IsKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this field can be inserted during import operations. The default is <c>true</c>.
    /// </summary>
    /// <value>
    /// <c>true</c> if this field can be inserted; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("canInsert")]
    public bool CanInsert { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this field can be updated during import operations. The default is <c>true</c>.
    /// </summary>
    /// <value>
    /// <c>true</c> if this field can be updated; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("canUpdate")]
    public bool CanUpdate { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this field can be mapped by users during import configuration. The default is <c>true</c>.
    /// </summary>
    /// <value>
    /// <c>true</c> if this field can be mapped; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("canMap")]
    public bool CanMap { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this field is required for import. Required fields must be provided in the import data.
    /// </summary>
    /// <value>
    /// <c>true</c> if the field is required; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("isRequired")]
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the default value behavior for the field during import, as specified by <see cref="FieldDefault"/>.
    /// </summary>
    /// <value>
    /// The <see cref="FieldDefault"/> option that determines how the default value is assigned.
    /// </value>
    [JsonPropertyName("default")]
    public FieldDefault? Default { get; set; }

    /// <summary>
    /// Gets or sets the static default value for the field, used when <see cref="Default"/> is set to <see cref="FieldDefault.Static"/>.
    /// </summary>
    /// <value>
    /// The static default value to assign to the field if no value is provided during import.
    /// </value>
    [JsonPropertyName("defaultValue")]
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Type"/> of the field translator, which is used to transform or convert field values during import.
    /// The type must implement <see cref="IFieldTranslator"/>.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> implementing the translation logic for this field.
    /// </value>
    [JsonPropertyName("translator")]
    [JsonConverter(typeof(Converters.TypeJsonConverter))]
    [Obsolete("Use TranslatorKey instead. This property will be removed in a future version.")]
    public Type? Translator { get; set; }

    /// <summary>
    /// Gets or sets the service key used to resolve the <see cref="IFieldTranslator"/> service from the dependency injection container.
    /// The service must be registered in the DI container with this key and implement <see cref="IFieldTranslator"/>.
    /// </summary>
    /// <value>
    /// The service key used to resolve the translator service from the dependency injection container.
    /// </value>
    [JsonPropertyName("translatorKey")]
    public string? TranslatorKey { get; set; }

    /// <summary>
    /// Gets or sets the list of match or validation expressions associated with this field. These expressions can be used for mapping or validating field values during import.
    /// </summary>
    /// <value>
    /// A list of string expressions for matching or validation purposes.
    /// </value>
    [JsonPropertyName("expressions")]
    public List<string> Expressions { get; set; } = [];

    /// <summary>
    /// Returns a string that represents the current <see cref="FieldDefinition"/> instance, including display name, field name, and data type.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> containing the display name, field name, and data type of the field.
    /// </returns>
    public override string ToString()
    {
        return $"Display: {DisplayName}, Name: {Name}, DataType: {DataType?.Name}";
    }
}
