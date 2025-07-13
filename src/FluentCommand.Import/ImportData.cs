using System.Text.Json.Serialization;

namespace FluentCommand.Import;

/// <summary>
/// Represents the data and field mappings for an import operation.
/// </summary>
public class ImportData
{
    /// <summary>
    /// Gets or sets the name of the import file.
    /// </summary>
    /// <value>
    /// The name or path of the file being imported, or <c>null</c> if not specified.
    /// </value>
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the tabular data to be imported.
    /// </summary>
    /// <value>
    /// A two-dimensional array of strings, where each inner array represents a row of data.
    /// </value>
    [JsonPropertyName("data")]
    public string[][] Data { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of field mappings between the import source and target fields.
    /// </summary>
    /// <value>
    /// A list of <see cref="FieldMap"/> objects describing how source fields map to target fields.
    /// </value>
    [JsonPropertyName("mappings")]
    public List<FieldMap> Mappings { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the imported data includes a header row.
    /// </summary>
    /// <value>
    /// <c>true</c> if the first row of <see cref="Data"/> contains column headers; otherwise, <c>false</c>.
    /// </value>
    [JsonPropertyName("hasHeader")]
    public bool HasHeader { get; set; } = true;
}
