namespace FluentCommand.Merge;

/// <summary>
/// Represents a column in the output of a data merge operation, including its name, value type, and value changes.
/// </summary>
public class DataMergeOutputColumn
{
    /// <summary>
    /// Gets or sets the name of the column.
    /// </summary>
    /// <value>
    /// The name of the column as it appears in the data source or target.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the current value of the column after the merge operation.
    /// </summary>
    /// <value>
    /// The value of the column after the merge.
    /// </value>
    public object Current { get; set; }

    /// <summary>
    /// Gets or sets the original value of the column before the merge operation.
    /// </summary>
    /// <value>
    /// The value of the column before the merge.
    /// </value>
    public object Original { get; set; }

    /// <summary>
    /// Gets or sets the data type of the column value.
    /// </summary>
    /// <value>
    /// The <see cref="System.Type"/> representing the type of the column value.
    /// </value>
    public Type Type { get; set; }
}
