using System.Text.Json.Serialization;

namespace FluentCommand.Import;

/// <summary>
/// Represents the definition and configuration of a field for import operations.
/// </summary>
public class FieldDefinition
{
    /// <summary>
    /// Gets or sets the display name of the field, used for UI or reporting purposes.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the unique name of the field.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the data type of the field.
    /// </summary>
    [JsonPropertyName("dataType")]
    [JsonConverter(typeof(Converters.TypeJsonConverter))]
    public Type? DataType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this field is a key field.
    /// </summary>
    /// <value>
    /// <c>true</c> if this field is a key; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("isKey")]
    public bool IsKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this field can be inserted during import. The default is <c>true</c>.
    /// </summary>
    /// <value>
    /// <c>true</c> if this field can be inserted; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("canInsert")]
    public bool CanInsert { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this field can be updated during import. The default is <c>true</c>.
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
    /// Gets or sets a value indicating whether this field is required for import.
    /// </summary>
    /// <value>
    /// <c>true</c> if the field is required; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("isRequired")]
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the default value behavior for the field during import.
    /// </summary>
    /// <value>
    /// The <see cref="FieldDefault"/> option specifying how the default value is determined.
    /// </value>
    [JsonPropertyName("default")]
    public FieldDefault? Default { get; set; }

    /// <summary>
    /// Gets or sets the static default value for the field, used when <see cref="Default"/> is set to <see cref="FieldDefault.Static"/>.
    /// </summary>
    /// <value>
    /// The static default value for the field.
    /// </value>
    [JsonPropertyName("defaultValue")]
    public object? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the type of the field translator, used to transform or convert field values during import.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> of the field translator.
    /// </value>
    [JsonPropertyName("translator")]
    [JsonConverter(typeof(Converters.TypeJsonConverter))]
    public Type? Translator { get; set; }

    /// <summary>
    /// Gets or sets the list of match expressions used for mapping or validation.
    /// </summary>
    /// <value>
    /// A list of string expressions for matching or validation.
    /// </value>
    [JsonPropertyName("expressions")]
    public List<string> Expressions { get; set; } = [];

    /// <summary>
    /// Returns a string that represents the current <see cref="FieldDefinition"/> instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> that contains the display name, field name, and data type.
    /// </returns>
    public override string ToString()
    {
        return $"Display: {DisplayName}, Name: {Name}, DataType: {DataType?.Name}";
    }
}
