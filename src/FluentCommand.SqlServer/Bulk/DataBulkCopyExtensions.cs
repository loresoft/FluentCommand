using Microsoft.Data.SqlClient;

namespace FluentCommand.Bulk
{
    /// <summary>
    /// Bulk Copy extension methods
    /// </summary>
    public static class DataBulkCopyExtensions
    {
        /// <summary>
        /// Starts a data bulk-copy with the specified destination table name.
        /// </summary>
        /// <param name="session">The session to use for the bulk-copy.</param>
        /// <param name="destinationTable">Name of the destination table on the server.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
        /// </returns>
        public static IDataBulkCopy BulkCopy(this IDataSession session, string destinationTable)
        {
            var bulkCopy = new DataBulkCopy(session, destinationTable);
            return bulkCopy;
        }

    }
}
