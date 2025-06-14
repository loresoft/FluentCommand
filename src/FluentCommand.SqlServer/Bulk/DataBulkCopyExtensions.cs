using Microsoft.Data.SqlClient;

namespace FluentCommand.Bulk;

/// <summary>
/// Provides extension methods for starting and configuring SQL Server bulk copy operations.
/// </summary>
public static class DataBulkCopyExtensions
{
    /// <summary>
    /// Starts a bulk copy operation using the specified data session and destination table name.
    /// </summary>
    /// <param name="session">The <see cref="IDataSession"/> to use for the bulk copy operation.</param>
    /// <param name="destinationTable">The name of the destination table on the SQL Server.</param>
    /// <returns>
    /// An <see cref="IDataBulkCopy"/> instance for configuring and executing the bulk copy operation.
    /// </returns>
    public static IDataBulkCopy BulkCopy(this IDataSession session, string destinationTable)
    {
        var bulkCopy = new DataBulkCopy(session, destinationTable);
        return bulkCopy;
    }
}
