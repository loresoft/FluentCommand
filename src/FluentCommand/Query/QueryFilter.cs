using System.Text.Json.Serialization;

using FluentCommand.Converters;

namespace FluentCommand.Query;

/// <summary>
/// Represents a filter expression or a group of nested filter expressions.
/// </summary>
public class QueryFilter
{
    /// <summary>
    /// Gets or sets the column or field name to filter.
    /// </summary>
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the value to compare against the column or field.
    /// </summary>
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(ObjectValueJsonConverter))]
    public object? Value { get; set; }

    /// <summary>
    /// Gets or sets the comparison operator for the filter.
    /// </summary>
    [JsonPropertyName("operator")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(JsonStringEnumConverter<FilterOperators>))]
    public FilterOperators? Operator { get; set; }

    /// <summary>
    /// Gets or sets the logical operator used to combine nested filters.
    /// </summary>
    [JsonPropertyName("logic")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(JsonStringEnumConverter<LogicalOperators>))]
    public LogicalOperators? Logic { get; set; }

    /// <summary>
    /// Gets or sets the nested filters that make up this filter group.
    /// </summary>
    [JsonPropertyName("filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IList<QueryFilter>? Filters { get; set; }
}
