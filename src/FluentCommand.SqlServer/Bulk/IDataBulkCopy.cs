using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace FluentCommand.Bulk
{
    /// <summary>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
    /// </summary>
    public interface IDataBulkCopy
    {
        /// <summary>
        /// Creates column mappings using the <see cref="DataTable"/> column names for both source and destination. 
        /// Only supported for overloads that take <see cref="DataTable"/>.
        /// </summary>
        /// <param name="value"><c>true</c> to automatically create mapping columns; otherwise false.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy AutoMap(bool value = true);

        /// <summary>
        /// Number of rows in each batch. At the end of each batch, the rows in the batch are sent to the server.
        /// </summary>
        /// <param name="value">Number of rows in each batch.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy BatchSize(int value);

        /// <summary>
        /// Number of seconds for the operation to complete before it times out.
        /// </summary>
        /// <param name="value">Number of seconds for the operation to complete before it times out.</param>
        /// <returns></returns>
        IDataBulkCopy BulkCopyTimeout(int value);

        /// <summary>
        /// Enables or disables a SqlBulkCopy object to stream data from an IDataReader object
        /// </summary>
        /// <param name="value">true if a SqlBulkCopy object can stream data from an IDataReader object; otherwise, false.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy EnableStreaming(bool value = true);

        /// <summary>
        /// Defines the number of rows to be processed before generating a notification event.
        /// </summary>
        /// <param name="value">The number of rows to be processed before generating a notification event.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy NotifyAfter(int value);


        /// <summary>
        /// Preserve source identity values. When not specified, identity values are assigned by the destination. 
        /// </summary>
        /// <param name="value">true to preservesource identity values; otherwise, false.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy KeepIdentity(bool value = true);

        /// <summary>
        /// Check constraints while data is being inserted. By default, constraints are not checked. 
        /// </summary>
        /// <param name="value">true to check constraints; otherwise, false.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy CheckConstraints(bool value = true);

        /// <summary>
        /// Obtain a bulk update lock for the duration of the bulk copy operation. When not specified, row locks are used. 
        /// </summary>
        /// <param name="value">true to obtain a bulk update lock; otherwise, false.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy TableLock(bool value = true);

        /// <summary>
        /// Preserve null values in the destination table regardless of the settings for default values. When not specified, null values are replaced by default values where applicable. 
        /// </summary>
        /// <param name="value">true to preserve null values; otherwise, false.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy KeepNulls(bool value = true);

        /// <summary>
        /// When specified, cause the server to fire the insert triggers for the rows being inserted into the database. 
        /// </summary>
        /// <param name="value">true to cause the server to fire the insert triggers; otherwise, false.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy FireTriggers(bool value = true);

        /// <summary>
        /// When specified, each batch of the bulk-copy operation will occur within a transaction.
        /// </summary>
        /// <param name="value">true to have bulk-copy operation occur within a transaction; otherwise, false.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy UseInternalTransaction(bool value = true);


        /// <summary>
        /// Creates a new column mapping, using column names to refer to source and destination columns.
        /// </summary>
        /// <param name="sourceColumn">The name of the source column within the data source.</param>
        /// <param name="destinationColumn">The name of the destination column within the destination table.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy Mapping(string sourceColumn, string destinationColumn);

        /// <summary>
        /// Creates a new column mapping, using a column ordinal to refer to the source column and a column name for the target column.
        /// </summary>
        /// <param name="sourceColumnOrdinal">The ordinal position of the source column within the data source.</param>
        /// <param name="destinationColumn">The name of the destination column within the destination table.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy Mapping(int sourceColumnOrdinal, string destinationColumn);

        /// <summary>
        /// Creates a new column mapping, using a column name to refer to the source column and a column ordinal for the target column.
        /// </summary>
        /// <param name="sourceColumn">The name of the source column within the data source.</param>
        /// <param name="destinationOrdinal">The ordinal position of the destination column within the destination table.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy Mapping(string sourceColumn, int destinationOrdinal);

        /// <summary>
        /// Creates a new column mapping, using column ordinals to refer to source and destination columns.
        /// </summary>
        /// <param name="sourceColumnOrdinal">The ordinal position of the source column within the data source.</param>
        /// <param name="destinationOrdinal">The ordinal position of the destination column within the destination table.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy Mapping(int sourceColumnOrdinal, int destinationOrdinal);

        /// <summary>
        /// Creates a new column mapping using a strongly typed builder.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="builder">The entity mapping builder.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy Mapping<TEntity>(Action<DataBulkCopyMapping<TEntity>> builder);

        /// <summary>
        /// Ignores the specified source column by removing it from the mapped columns collection.
        /// </summary>
        /// <param name="sourceColumn">The source column to remove from mapping.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy Ignore(string sourceColumn);

        /// <summary>
        /// Ignores the specified source column by removing it from the mapped columns collection.
        /// </summary>
        /// <param name="sourceColumnOrdinal">The ordinal position of the source column within the data source.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy "/> operation.
        /// </returns>
        IDataBulkCopy Ignore(int sourceColumnOrdinal);


        /// <summary>
        /// Copies all items in the supplied <see cref="T:System.Collections.Generic.IEnumerable`1" /> to a destination table.
        /// </summary>
        /// <typeparam name="T">The type of the data elements.</typeparam>
        /// <param name="data">An IEnumerable that will be copied to the destination table.</param>
        void WriteToServer<T>(IEnumerable<T> data);

        /// <summary>
        /// Copies all rows from the supplied <see cref="DataRow"/> array to a destination table. 
        /// </summary>
        /// <param name="rows">An array of DataRow objects that will be copied to the destination table.</param>
        void WriteToServer(DataRow[] rows);

        /// <summary>
        /// Copies all rows in the supplied <see cref="DataTable"/> to a destination table.
        /// </summary>
        /// <param name="table">A DataTable whose rows will be copied to the destination table.</param>
        void WriteToServer(DataTable table);

        /// <summary>
        /// Copies only rows that match the supplied row state in the supplied <see cref="DataTable"/> to a destination table.
        /// </summary>
        /// <param name="table">A DataTable whose rows will be copied to the destination table.</param>
        /// <param name="rowState">A value from the DataRowState enumeration. Only rows matching the row state are copied to the destination.</param>
        void WriteToServer(DataTable table, DataRowState rowState);

        /// <summary>
        /// Copies all rows in the supplied <see cref="IDataReader"/> to a destination table.
        /// </summary>
        /// <param name="reader">A IDataReader whose rows will be copied to the destination table.</param>
        void WriteToServer(IDataReader reader);

    }
}