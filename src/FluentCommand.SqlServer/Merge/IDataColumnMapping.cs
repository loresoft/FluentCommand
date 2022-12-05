namespace FluentCommand.Merge;

/// <summary>
/// A fluent <see langword="interface" /> for a data column mapping.
/// </summary>
public interface IDataColumnMapping
{
    /// <summary>
    /// Sets the source column name used to read from source data.
    /// </summary>
    /// <param name="value">The source column name.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> for a data column mapping.
    /// </returns>
    IDataColumnMapping SourceColumn(string value);

    /// <summary>
    /// Sets the target column name.
    /// </summary>
    /// <param name="value">The target column name.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> for a data column mapping.
    /// </returns>
    IDataColumnMapping TargetColumn(string value);

    /// <summary>
    /// Sets the SQL Server native type for the <see cref="SourceColumn"/>.
    /// </summary>
    /// <param name="value">The  SQL Server native type.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> for a data column mapping.
    /// </returns>
    IDataColumnMapping NativeType(string value);

    /// <summary>
    /// Sets a value indicating whether the column is include in the bulk copy operation
    /// </summary>
    /// <param name="value"><c>true</c> if column is included in bulk copy; otherwise, <c>false</c>.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> for a data column mapping.
    /// </returns>
    IDataColumnMapping BulkCopy(bool value = true);

    /// <summary>
    /// Sets a value indicating whether the column can be inserted.
    /// </summary>
    /// <param name="value"><c>true</c> if the column can be inserted; otherwise, <c>false</c>.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> for a data column mapping.
    /// </returns>
    IDataColumnMapping Insert(bool value = true);

    /// <summary>
    /// Sets a value indicating whether the column can be updated.
    /// </summary>
    /// <param name="value"><c>true</c> if the column can be updated; otherwise, <c>false</c>.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> for a data column mapping.
    /// </returns>
    IDataColumnMapping Update(bool value = true);

    /// <summary>
    /// Sets a value indicating whether the column is used as part of the key to merge on.
    /// </summary>
    /// <param name="value"><c>true</c> if the column is part of the key; otherwise, <c>false</c>.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> for a data column mapping.
    /// </returns>
    IDataColumnMapping Key(bool value = true);

    /// <summary>
    /// sets a value indicating whether the column is ignored, not used in anyway.
    /// </summary>
    /// <param name="value"><c>true</c> if the column is ignored; otherwise, <c>false</c>.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> for a data column mapping.
    /// </returns>
    IDataColumnMapping Ignore(bool value = true);

}
