using System.Data;
using System.Linq.Expressions;

using Microsoft.Data.SqlClient;

namespace FluentCommand.Bulk;

/// <summary>
/// Provides a fluent interface for configuring and executing <see cref="SqlBulkCopy"/> operations to efficiently copy large amounts of data into a SQL Server table.
/// </summary>
public interface IDataBulkCopy
{
    /// <summary>
    /// Automatically creates column mappings using the <see cref="DataTable"/> column names for both source and destination.
    /// Only supported for overloads that take <see cref="DataTable"/> or <see cref="IDataReader"/>.
    /// </summary>
    /// <param name="value"><c>true</c> to automatically create mapping columns; otherwise, <c>false</c>. The default is <c>true</c>.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy AutoMap(bool value = true);

    /// <summary>
    /// Sets the number of rows in each batch. At the end of each batch, the rows in the batch are sent to the server.
    /// </summary>
    /// <param name="value">The number of rows in each batch. A value of zero indicates that all rows are sent in a single batch.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy BatchSize(int value);

    /// <summary>
    /// Sets the number of seconds for the operation to complete before it times out.
    /// </summary>
    /// <param name="value">The timeout duration, in seconds. A value of zero indicates no timeout.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy BulkCopyTimeout(int value);

    /// <summary>
    /// Enables or disables streaming data from an <see cref="IDataReader"/> object.
    /// </summary>
    /// <param name="value"><c>true</c> to enable streaming; otherwise, <c>false</c>. The default is <c>true</c>.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy EnableStreaming(bool value = true);

    /// <summary>
    /// Sets the number of rows to be processed before generating a notification event.
    /// </summary>
    /// <param name="value">The number of rows to process before notification. A value of zero disables notification.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy NotifyAfter(int value);

    /// <summary>
    /// Preserves source identity values. When not specified, identity values are assigned by the destination.
    /// </summary>
    /// <param name="value"><c>true</c> to preserve source identity values; otherwise, <c>false</c>. The default is <c>true</c>.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy KeepIdentity(bool value = true);

    /// <summary>
    /// Checks constraints while data is being inserted. By default, constraints are not checked.
    /// </summary>
    /// <param name="value"><c>true</c> to check constraints; otherwise, <c>false</c>. The default is <c>true</c>.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy CheckConstraints(bool value = true);

    /// <summary>
    /// Obtains a bulk update lock for the duration of the bulk copy operation. When not specified, row locks are used.
    /// </summary>
    /// <param name="value"><c>true</c> to obtain a bulk update lock; otherwise, <c>false</c>. The default is <c>true</c>.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy TableLock(bool value = true);

    /// <summary>
    /// Preserves null values in the destination table regardless of the settings for default values. When not specified, null values are replaced by default values where applicable.
    /// </summary>
    /// <param name="value"><c>true</c> to preserve null values; otherwise, <c>false</c>. The default is <c>true</c>.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy KeepNulls(bool value = true);

    /// <summary>
    /// Causes the server to fire the insert triggers for the rows being inserted into the database.
    /// </summary>
    /// <param name="value"><c>true</c> to fire insert triggers; otherwise, <c>false</c>. The default is <c>true</c>.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy FireTriggers(bool value = true);

    /// <summary>
    /// Specifies that each batch of the bulk-copy operation will occur within a transaction.
    /// </summary>
    /// <param name="value"><c>true</c> to use an internal transaction for each batch; otherwise, <c>false</c>. The default is <c>true</c>.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy UseInternalTransaction(bool value = true);

    /// <summary>
    /// Creates a new column mapping using column names to refer to source and destination columns.
    /// </summary>
    /// <param name="sourceColumn">The name of the source column within the data source.</param>
    /// <param name="destinationColumn">The name of the destination column within the destination table.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy Mapping(string sourceColumn, string destinationColumn);

    /// <summary>
    /// Creates a new column mapping using a column ordinal to refer to the source column and a column name for the destination column.
    /// </summary>
    /// <param name="sourceColumnOrdinal">The ordinal position of the source column within the data source.</param>
    /// <param name="destinationColumn">The name of the destination column within the destination table.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy Mapping(int sourceColumnOrdinal, string destinationColumn);

    /// <summary>
    /// Creates a new column mapping using a column name to refer to the source column and a column ordinal for the destination column.
    /// </summary>
    /// <param name="sourceColumn">The name of the source column within the data source.</param>
    /// <param name="destinationOrdinal">The ordinal position of the destination column within the destination table.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy Mapping(string sourceColumn, int destinationOrdinal);

    /// <summary>
    /// Creates a new column mapping using column ordinals to refer to source and destination columns.
    /// </summary>
    /// <param name="sourceColumnOrdinal">The ordinal position of the source column within the data source.</param>
    /// <param name="destinationOrdinal">The ordinal position of the destination column within the destination table.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy Mapping(int sourceColumnOrdinal, int destinationOrdinal);

    /// <summary>
    /// Creates a new column mapping using a strongly typed builder for mapping entity properties to destination columns.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="builder">The entity mapping builder delegate used to configure column mappings.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy Mapping<TEntity>(Action<DataBulkCopyMapping<TEntity>> builder) where TEntity : class;

    /// <summary>
    /// Ignores the specified source column by removing it from the mapped columns collection.
    /// </summary>
    /// <param name="sourceColumn">The name of the source column to exclude from mapping.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy Ignore(string sourceColumn);

    /// <summary>
    /// Ignores the specified source column by removing it from the mapped columns collection.
    /// </summary>
    /// <param name="sourceColumnOrdinal">The ordinal position of the source column to exclude from mapping.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy Ignore(int sourceColumnOrdinal);

    /// <summary>
    /// Ignores the specified entity property by removing it from the mapped columns collection.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="sourceProperty">An expression that identifies the entity property to exclude from mapping.</param>
    /// <returns>
    /// The same <see cref="IDataBulkCopy"/> instance for fluent chaining.
    /// </returns>
    IDataBulkCopy Ignore<TEntity, TValue>(Expression<Func<TEntity, TValue>> sourceProperty) where TEntity : class;

    /// <summary>
    /// Copies all items in the supplied <see cref="IEnumerable{TEntity}"/> to the destination table using bulk copy.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="data">An enumerable collection of entities to be copied to the destination table.</param>
    void WriteToServer<TEntity>(IEnumerable<TEntity> data) where TEntity : class;

    /// <summary>
    /// Asynchronously copies all items in the supplied <see cref="IEnumerable{TEntity}"/> to the destination table using bulk copy.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="data">An enumerable collection of entities to be copied to the destination table.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteToServerAsync<TEntity>(IEnumerable<TEntity> data, CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Copies all rows from the supplied <see cref="DataRow"/> array to the destination table using bulk copy.
    /// </summary>
    /// <param name="rows">An array of <see cref="DataRow"/> objects to be copied to the destination table.</param>
    void WriteToServer(DataRow[] rows);

    /// <summary>
    /// Asynchronously copies all rows from the supplied <see cref="DataRow"/> array to the destination table using bulk copy.
    /// </summary>
    /// <param name="rows">An array of <see cref="DataRow"/> objects to be copied to the destination table.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteToServerAsync(DataRow[] rows, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies all rows in the supplied <see cref="DataTable"/> to the destination table using bulk copy.
    /// </summary>
    /// <param name="table">A <see cref="DataTable"/> whose rows will be copied to the destination table.</param>
    void WriteToServer(DataTable table);

    /// <summary>
    /// Copies only rows that match the supplied row state in the supplied <see cref="DataTable"/> to the destination table using bulk copy.
    /// </summary>
    /// <param name="table">A <see cref="DataTable"/> whose rows will be copied to the destination table.</param>
    /// <param name="rowState">A value from the <see cref="DataRowState"/> enumeration. Only rows matching the row state are copied to the destination.</param>
    void WriteToServer(DataTable table, DataRowState rowState);

    /// <summary>
    /// Asynchronously copies rows from the supplied <see cref="DataTable"/> to the destination table using bulk copy.
    /// </summary>
    /// <param name="table">A <see cref="DataTable"/> whose rows will be copied to the destination table.</param>
    /// <param name="rowState">A value from the <see cref="DataRowState"/> enumeration. Only rows matching the row state are copied to the destination. The default value of <c>0</c> copies all rows.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteToServerAsync(DataTable table, DataRowState rowState = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies all rows in the supplied <see cref="IDataReader"/> to the destination table using bulk copy.
    /// </summary>
    /// <param name="reader">An <see cref="IDataReader"/> whose rows will be copied to the destination table.</param>
    void WriteToServer(IDataReader reader);

    /// <summary>
    /// Asynchronously copies all rows in the supplied <see cref="IDataReader"/> to the destination table using bulk copy.
    /// </summary>
    /// <param name="reader">An <see cref="IDataReader"/> whose rows will be copied to the destination table.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteToServerAsync(IDataReader reader, CancellationToken cancellationToken = default);
}
