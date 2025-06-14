namespace FluentCommand.Merge;

/// <summary>
/// Provides a fluent API for configuring a <see cref="DataMergeColumn"/> mapping for data merge operations.
/// </summary>
public class DataMergeColumnMapping : IDataColumnMapping
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataMergeColumnMapping"/> class.
    /// </summary>
    /// <param name="mergeColumn">The <see cref="DataMergeColumn"/> instance to be configured.</param>
    public DataMergeColumnMapping(DataMergeColumn mergeColumn)
    {
        MergeColumn = mergeColumn;
    }

    /// <summary>
    /// Gets the current <see cref="DataMergeColumn"/> being configured.
    /// </summary>
    /// <value>
    /// The <see cref="DataMergeColumn"/> instance being updated.
    /// </value>
    public DataMergeColumn MergeColumn { get; }

    /// <summary>
    /// Sets the source column name used to read from the source data.
    /// </summary>
    /// <param name="value">The name of the source column.</param>
    /// <returns>
    /// The same <see cref="IDataColumnMapping"/> instance for fluent chaining.
    /// </returns>
    public IDataColumnMapping SourceColumn(string value)
    {
        MergeColumn.SourceColumn = value;
        return this;
    }

    /// <summary>
    /// Sets the target column name in the destination table.
    /// </summary>
    /// <param name="value">The name of the target column.</param>
    /// <returns>
    /// The same <see cref="IDataColumnMapping"/> instance for fluent chaining.
    /// </returns>
    public IDataColumnMapping TargetColumn(string value)
    {
        MergeColumn.TargetColumn = value;
        return this;
    }

    /// <summary>
    /// Sets the SQL Server native data type for the <see cref="SourceColumn"/>.
    /// </summary>
    /// <param name="value">The SQL Server native data type (e.g., <c>nvarchar(100)</c>, <c>int</c>).</param>
    /// <returns>
    /// The same <see cref="IDataColumnMapping"/> instance for fluent chaining.
    /// </returns>
    public IDataColumnMapping NativeType(string value)
    {
        MergeColumn.NativeType = value;
        return this;
    }

    /// <summary>
    /// Sets a value indicating whether the column is included in the bulk copy operation.
    /// </summary>
    /// <param name="value"><c>true</c> to include the column in bulk copy; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>
    /// The same <see cref="IDataColumnMapping"/> instance for fluent chaining.
    /// </returns>
    public IDataColumnMapping BulkCopy(bool value = true)
    {
        MergeColumn.CanBulkCopy = value;
        return this;
    }

    /// <summary>
    /// Sets a value indicating whether the column can be inserted into the target table.
    /// </summary>
    /// <param name="value"><c>true</c> if the column can be inserted; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>
    /// The same <see cref="IDataColumnMapping"/> instance for fluent chaining.
    /// </returns>
    public IDataColumnMapping Insert(bool value = true)
    {
        MergeColumn.CanInsert = value;
        return this;
    }

    /// <summary>
    /// Sets a value indicating whether the column can be updated in the target table.
    /// </summary>
    /// <param name="value"><c>true</c> if the column can be updated; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>
    /// The same <see cref="IDataColumnMapping"/> instance for fluent chaining.
    /// </returns>
    public IDataColumnMapping Update(bool value = true)
    {
        MergeColumn.CanUpdate = value;
        return this;
    }

    /// <summary>
    /// Sets a value indicating whether the column is used as part of the key for the merge operation.
    /// When set to <c>true</c>, the column is marked as a key and will not be updated.
    /// </summary>
    /// <param name="value"><c>true</c> if the column is part of the merge key; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>
    /// The same <see cref="IDataColumnMapping"/> instance for fluent chaining.
    /// </returns>
    public IDataColumnMapping Key(bool value = true)
    {
        MergeColumn.IsKey = value;

        // only change if true
        if (value)
            MergeColumn.CanUpdate = false;

        return this;
    }

    /// <summary>
    /// Sets a value indicating whether the column is ignored and not used in the merge operation.
    /// </summary>
    /// <param name="value"><c>true</c> if the column is ignored; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>
    /// The same <see cref="IDataColumnMapping"/> instance for fluent chaining.
    /// </returns>
    public IDataColumnMapping Ignore(bool value = true)
    {
        MergeColumn.IsIgnored = value;
        return this;
    }
}
