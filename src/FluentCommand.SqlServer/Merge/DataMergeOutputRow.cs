namespace FluentCommand.Merge;

/// <summary>
/// Represents a row in the output of a data merge operation, including the merge action and the columns that changed.
/// </summary>
public class DataMergeOutputRow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataMergeOutputRow"/> class.
    /// </summary>
    public DataMergeOutputRow()
    {
        Columns = new List<DataMergeOutputColumn>();
    }

    /// <summary>
    /// Gets or sets the merge action performed on this row (e.g., "INSERT", "UPDATE", "DELETE").
    /// </summary>
    /// <value>
    /// The merge action for this row.
    /// </value>
    public string Action { get; set; }

    /// <summary>
    /// Gets or sets the collection of columns that were affected by the merge operation.
    /// </summary>
    /// <value>
    /// The list of <see cref="DataMergeOutputColumn"/> objects representing the changed columns.
    /// </value>
    public List<DataMergeOutputColumn> Columns { get; set; }

    /// <summary>
    /// Gets the <see cref="DataMergeOutputColumn"/> with the specified column name, or <c>null</c> if not found.
    /// </summary>
    /// <param name="columnName">The name of the column to retrieve.</param>
    /// <returns>
    /// The <see cref="DataMergeOutputColumn"/> with the specified name, or <c>null</c> if no such column exists.
    /// </returns>
    public DataMergeOutputColumn this[string columnName]
    {
        get
        {
            return Columns.FirstOrDefault(c => c.Name == columnName);
        }
    }
}
