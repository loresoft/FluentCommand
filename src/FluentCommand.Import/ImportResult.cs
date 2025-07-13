using System.Text.Json.Serialization;

namespace FluentCommand.Import;

/// <summary>
/// Represents the result of an import operation, including status message, processed row count, and any errors encountered.
/// </summary>
public class ImportResult
{
    /// <summary>
    /// Gets or sets a status or summary message describing the outcome of the import operation.
    /// </summary>
    /// <value>
    /// A message providing additional information about the import result, or <c>null</c> if not specified.
    /// </value>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the number of rows that were processed during the import operation.
    /// </summary>
    /// <value>
    /// The total number of rows processed.
    /// </value>
    [JsonPropertyName("processed")]
    public int Processed { get; set; }

    /// <summary>
    /// Gets or sets the collection of error messages encountered during the import operation.
    /// </summary>
    /// <value>
    /// A read-only collection of error messages, or <c>null</c> if no errors occurred.
    /// </value>
    [JsonPropertyName("errors")]
    public IReadOnlyCollection<string>? Errors { get; set; }
}
