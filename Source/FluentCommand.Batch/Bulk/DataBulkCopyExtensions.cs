using System;
using System.Data.SqlClient;

namespace FluentCommand.Bulk
{
    public static class DataBulkCopyExtensions
    {
        /// <summary>
        /// Starts a data bulk-copy with the specified destination table name.
        /// </summary>
        /// <param name="sesssion">The sesssion to use for the bulk-copy.</param>
        /// <param name="destinationTable">Name of the destination table on the server.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static IDataBulkCopy BulkCopy(this IDataSession sesssion, string destinationTable)
        {
            var bulkCopy = new DataBulkCopy(sesssion, destinationTable);
            return bulkCopy;
        }

    }
}
