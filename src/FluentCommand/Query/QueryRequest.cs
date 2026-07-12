using System.Text.Json.Serialization;

namespace FluentCommand.Query;

/// <summary>
/// Represents a query request containing filtering, sorting, and paging options.
/// </summary>
public class QueryRequest
{
    /// <summary>
    /// Gets or sets the sort expressions to apply to the query.
    /// </summary>
    [JsonPropertyName("sort")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<QuerySort>? Sort { get; set; }

    /// <summary>
    /// Gets or sets the root filter expression to apply to the query.
    /// </summary>
    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public QueryFilter? Filter { get; set; }

    /// <summary>
    /// Gets or sets the 1-based page number to retrieve.
    /// </summary>
    [JsonPropertyName("page")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items to include in each page.
    /// </summary>
    [JsonPropertyName("pageSize")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PageSize { get; set; }

    /// <summary>
    /// Gets or sets the continuation token used to retrieve the next page of results.
    /// </summary>
    [JsonPropertyName("continuationToken")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ContinuationToken { get; set; }
}
