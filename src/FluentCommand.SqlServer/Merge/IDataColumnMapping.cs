namespace FluentCommand.Merge;

/// <summary>
/// Provides a fluent interface for configuring data column mappings in a merge operation.
/// </summary>
public interface IDataColumnMapping
{
    /// <summary>
    /// Specifies the source column name to read from the source data.
    /// </summary>
    /// <param name="value">The name of the source column.</param>
    /// <returns>
    /// The current <see cref="IDataColumnMapping"/> instance for chaining.
    /// </returns>
    IDataColumnMapping SourceColumn(string value);

    /// <summary>
    /// Specifies the target column name in the destination table.
    /// </summary>
    /// <param name="value">The name of the target column.</param>
    /// <returns>
    /// The current <see cref="IDataColumnMapping"/> instance for chaining.
    /// </returns>
    IDataColumnMapping TargetColumn(string value);

    /// <summary>
    /// Sets the SQL Server native data type for the source column.
    /// </summary>
    /// <param name="value">The SQL Server native type (e.g., <c>nvarchar</c>, <c>int</c>).</param>
    /// <returns>
    /// The current <see cref="IDataColumnMapping"/> instance for chaining.
    /// </returns>
    IDataColumnMapping NativeType(string value);

    /// <summary>
    /// Indicates whether the column should be included in the bulk copy operation.
    /// </summary>
    /// <param name="value"><c>true</c> to include the column in bulk copy; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>
    /// The current <see cref="IDataColumnMapping"/> instance for chaining.
    /// </returns>
    IDataColumnMapping BulkCopy(bool value = true);

    /// <summary>
    /// Indicates whether the column can be inserted during the merge operation.
    /// </summary>
    /// <param name="value"><c>true</c> to allow insertion; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>
    /// The current <see cref="IDataColumnMapping"/> instance for chaining.
    /// </returns>
    IDataColumnMapping Insert(bool value = true);

    /// <summary>
    /// Indicates whether the column can be updated during the merge operation.
    /// </summary>
    /// <param name="value"><c>true</c> to allow updates; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>
    /// The current <see cref="IDataColumnMapping"/> instance for chaining.
    /// </returns>
    IDataColumnMapping Update(bool value = true);

    /// <summary>
    /// Indicates whether the column is used as part of the key for the merge operation.
    /// </summary>
    /// <param name="value"><c>true</c> if the column is part of the merge key; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>
    /// The current <see cref="IDataColumnMapping"/> instance for chaining.
    /// </returns>
    IDataColumnMapping Key(bool value = true);

    /// <summary>
    /// Indicates whether the column should be ignored and not used in the merge operation.
    /// </summary>
    /// <param name="value"><c>true</c> to ignore the column; otherwise, <c>false</c>. Default is <c>true</c>.</param>
    /// <returns>
    /// The current <see cref="IDataColumnMapping"/> instance for chaining.
    /// </returns>
    IDataColumnMapping Ignore(bool value = true);

}
