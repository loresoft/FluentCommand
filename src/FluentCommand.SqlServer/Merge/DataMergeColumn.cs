namespace FluentCommand.Merge;

/// <summary>
/// Represents a mapping between a source and target column for a data merge operation.
/// </summary>
public class DataMergeColumn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataMergeColumn"/> class with default settings.
    /// </summary>
    /// <remarks>
    /// By default, <see cref="CanInsert"/>, <see cref="CanUpdate"/>, and <see cref="CanBulkCopy"/> are set to <c>true</c>.
    /// </remarks>
    public DataMergeColumn()
    {
        CanInsert = true;
        CanUpdate = true;
        CanBulkCopy = true;
    }

    /// <summary>
    /// Gets or sets the name of the source column used to read from the source data.
    /// This name is also used in the temporary table during the merge operation.
    /// </summary>
    /// <value>
    /// The name of the source column.
    /// </value>
    public string SourceColumn { get; set; }

    /// <summary>
    /// Gets or sets the name of the target column in the destination table.
    /// </summary>
    /// <value>
    /// The name of the target column.
    /// </value>
    public string TargetColumn { get; set; }

    /// <summary>
    /// Gets or sets the SQL Server native data type for the <see cref="SourceColumn"/>.
    /// </summary>
    /// <value>
    /// The SQL Server native data type (e.g., <c>nvarchar(100)</c>, <c>int</c>).
    /// </value>
    public string NativeType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is included in the bulk copy operation.
    /// </summary>
    /// <value>
    /// <c>true</c> if the column is included in bulk copy; otherwise, <c>false</c>. Default is <c>true</c>.
    /// </value>
    public bool CanBulkCopy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column can be inserted into the target table.
    /// </summary>
    /// <value>
    /// <c>true</c> if the column can be inserted; otherwise, <c>false</c>. Default is <c>true</c>.
    /// </value>
    public bool CanInsert { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column can be updated in the target table.
    /// </summary>
    /// <value>
    /// <c>true</c> if the column can be updated; otherwise, <c>false</c>. Default is <c>true</c>.
    /// </value>
    public bool CanUpdate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is used as part of the key for the merge operation.
    /// </summary>
    /// <value>
    /// <c>true</c> if the column is part of the merge key; otherwise, <c>false</c>.
    /// </value>
    public bool IsKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is ignored and not used in the merge operation.
    /// </summary>
    /// <value>
    /// <c>true</c> if the column is ignored; otherwise, <c>false</c>.
    /// </value>
    public bool IsIgnored { get; set; }

    /// <summary>
    /// Returns a string that represents the current <see cref="DataMergeColumn"/> instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> containing the source column, target column, native type, key status, and ignored status.
    /// </returns>
    public override string ToString()
    {
        return $"Source: {SourceColumn}, Target: {TargetColumn}, NativeType: {NativeType}, Key: {IsKey}, Ignored: {IsIgnored}";
    }
}
