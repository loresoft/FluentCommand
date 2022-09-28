namespace FluentCommand.Merge;

/// <summary>
/// How the data should be merged into the table
/// </summary>
public enum DataMergeMode
{
    /// <summary>
    /// Automatic determine the best mode.  If source over 1000 row, <see cref="BulkCopy"/> will be used.
    /// </summary>
    Auto,
    /// <summary>
    /// Use SqlServer bulk copy to insert rows into a temporary table, then merge to target table
    /// </summary>
    BulkCopy,
    /// <summary>
    /// Generate a SqlServer merge statement to execute.
    /// </summary>
    SqlStatement
}