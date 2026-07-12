using System.Text.Json.Serialization;

namespace FluentCommand.Query;

/// <summary>
/// Represents a sort expression for a query request.
/// </summary>
public class QuerySort
{
    /// <summary>
    /// Gets or sets the column or field name to sort.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sort direction.
    /// </summary>
    [JsonPropertyName("direction")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(JsonStringEnumConverter<SortDirections>))]
    public SortDirections? Direction { get; set; }
}
