using System.Text.Json.Serialization;

namespace FluentCommand.Query;

/// <summary>
/// Represents a paged query result.
/// </summary>
/// <typeparam name="T">The type of items returned by the query.</typeparam>
public class QueryResult<T>
{
    /// <summary>
    /// Gets or sets the query result data.
    /// </summary>
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<T>? Data { get; set; }

    /// <summary>
    /// Gets or sets the total number of available items.
    /// </summary>
    [JsonPropertyName("total")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? Total { get; set; }

    /// <summary>
    /// Gets or sets the continuation token used to retrieve the next page of results.
    /// </summary>
    [JsonPropertyName("continuationToken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContinuationToken { get; set; }
}
