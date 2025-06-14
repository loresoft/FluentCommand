namespace FluentCommand.Merge;

/// <summary>
/// Specifies the strategy for merging data into a target table.
/// </summary>
public enum DataMergeMode
{
    /// <summary>
    /// Automatically determines the best merge mode based on the source data size.
    /// If the source contains more than 1000 rows, <see cref="BulkCopy"/> will be used; otherwise, <see cref="SqlStatement"/> is used.
    /// </summary>
    Auto,
    /// <summary>
    /// Uses SQL Server bulk copy to insert rows into a temporary table, then merges the data into the target table.
    /// This mode is optimized for large data sets.
    /// </summary>
    BulkCopy,
    /// <summary>
    /// Generates and executes a SQL Server MERGE statement to merge the source data directly into the target table.
    /// This mode is suitable for smaller data sets or when bulk copy is not desired.
    /// </summary>
    SqlStatement
}
