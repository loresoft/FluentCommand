using System.Data;

using FluentCommand.Extensions;

using Microsoft.Data.SqlClient;

namespace FluentCommand.Bulk;

/// <summary>
/// A fluent <see langword="class" /> to a <see cref="SqlBulkCopy "/> operation.
/// </summary>
public class DataBulkCopy : DisposableBase, IDataBulkCopy
{
    private readonly IDataSession _dataSession;
    private readonly string _destinationTable;

    private readonly List<SqlBulkCopyColumnMapping> _mapping;
    private readonly List<string> _ignoreColumns;
    private readonly List<int> _ignoreOrdinal;

    private SqlBulkCopyOptions _options = SqlBulkCopyOptions.Default;
    private int? _batchSize;
    private int? _bulkCopyTimeout;
    private bool? _enableStreaming;
    private int? _notifyAfter;
    private bool? _autoMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataBulkCopy" /> class.
    /// </summary>
    /// <param name="dataSession">The data session.</param>
    /// <param name="destinationTable">The destination table.</param>
    public DataBulkCopy(IDataSession dataSession, string destinationTable)
    {
        _mapping = new List<SqlBulkCopyColumnMapping>();
        _ignoreColumns = new List<string>();
        _ignoreOrdinal = new List<int>();

        _dataSession = dataSession;
        _destinationTable = destinationTable;
    }

    /// <summary>
    /// Creates a new column mapping, using column names both source and destination.
    /// </summary>
    /// <param name="value"><c>true</c> to automatically mapping columns; otherwise false.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy AutoMap(bool value = true)
    {
        _autoMap = value;
        return this;
    }

    /// <summary>
    /// Number of rows in each batch. At the end of each batch, the rows in the batch are sent to the server.
    /// </summary>
    /// <param name="value">Number of rows in each batch.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy BatchSize(int value)
    {
        _batchSize = value;
        return this;
    }

    /// <summary>
    /// Number of seconds for the operation to complete before it times out.
    /// </summary>
    /// <param name="value">Number of seconds for the operation to complete before it times out.</param>
    /// <returns></returns>
    public IDataBulkCopy BulkCopyTimeout(int value)
    {
        _bulkCopyTimeout = value;
        return this;
    }

    /// <summary>
    /// Enables or disables a SqlBulkCopy object to stream data from an IDataReader object
    /// </summary>
    /// <param name="value">true if a SqlBulkCopy object can stream data from an IDataReader object; otherwise, false.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy EnableStreaming(bool value = true)
    {
        _enableStreaming = value;
        return this;
    }

    /// <summary>
    /// Defines the number of rows to be processed before generating a notification event.
    /// </summary>
    /// <param name="value">The number of rows to be processed before generating a notification event.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy NotifyAfter(int value)
    {
        _notifyAfter = value;
        return this;
    }


    /// <summary>
    /// Preserve source identity values. When not specified, identity values are assigned by the destination.
    /// </summary>
    /// <param name="value">true to preserve source identity values; otherwise, false.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy KeepIdentity(bool value = true)
    {
        _options = value
            ? _options.SetFlagOn(SqlBulkCopyOptions.KeepIdentity)
            : _options.SetFlagOff(SqlBulkCopyOptions.KeepIdentity);

        return this;
    }

    /// <summary>
    /// Check constraints while data is being inserted. By default, constraints are not checked.
    /// </summary>
    /// <param name="value">true to check constraints; otherwise, false.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy CheckConstraints(bool value = true)
    {
        _options = value
            ? _options.SetFlagOn(SqlBulkCopyOptions.CheckConstraints)
            : _options.SetFlagOff(SqlBulkCopyOptions.CheckConstraints);

        return this;
    }

    /// <summary>
    /// Obtain a bulk update lock for the duration of the bulk copy operation. When not specified, row locks are used.
    /// </summary>
    /// <param name="value">true to obtain a bulk update lock; otherwise, false.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy TableLock(bool value = true)
    {
        _options = value
            ? _options.SetFlagOn(SqlBulkCopyOptions.TableLock)
            : _options.SetFlagOff(SqlBulkCopyOptions.TableLock);

        return this;
    }

    /// <summary>
    /// Preserve null values in the destination table regardless of the settings for default values. When not specified, null values are replaced by default values where applicable.
    /// </summary>
    /// <param name="value">true to preserve null values; otherwise, false.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy KeepNulls(bool value = true)
    {
        _options = value
            ? _options.SetFlagOn(SqlBulkCopyOptions.KeepNulls)
            : _options.SetFlagOff(SqlBulkCopyOptions.KeepNulls);

        return this;
    }

    /// <summary>
    /// When specified, cause the server to fire the insert triggers for the rows being inserted into the database.
    /// </summary>
    /// <param name="value">true to cause the server to fire the insert triggers; otherwise, false.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy FireTriggers(bool value = true)
    {
        _options = value
            ? _options.SetFlagOn(SqlBulkCopyOptions.FireTriggers)
            : _options.SetFlagOff(SqlBulkCopyOptions.FireTriggers);

        return this;
    }

    /// <summary>
    /// When specified, each batch of the bulk-copy operation will occur within a transaction.
    /// </summary>
    /// <param name="value">true to have bulk-copy operation occur within a transaction; otherwise, false.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy UseInternalTransaction(bool value = true)
    {
        _options = value
            ? _options.SetFlagOn(SqlBulkCopyOptions.UseInternalTransaction)
            : _options.SetFlagOff(SqlBulkCopyOptions.UseInternalTransaction);

        return this;
    }


    /// <summary>
    /// Creates a new column mapping, using column names to refer to source and destination columns.
    /// </summary>
    /// <param name="sourceColumn">The name of the source column within the data source.</param>
    /// <param name="destinationColumn">The name of the destination column within the destination table.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy Mapping(string sourceColumn, string destinationColumn)
    {
        var map = new SqlBulkCopyColumnMapping(sourceColumn, destinationColumn);
        _mapping.Add(map);
        return this;
    }

    /// <summary>
    /// Creates a new column mapping, using a column ordinal to refer to the source column and a column name for the target column.
    /// </summary>
    /// <param name="sourceColumnOrdinal">The ordinal position of the source column within the data source.</param>
    /// <param name="destinationColumn">The name of the destination column within the destination table.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy Mapping(int sourceColumnOrdinal, string destinationColumn)
    {
        var map = new SqlBulkCopyColumnMapping(sourceColumnOrdinal, destinationColumn);
        _mapping.Add(map);
        return this;
    }

    /// <summary>
    /// Creates a new column mapping, using a column name to refer to the source column and a column ordinal for the target column.
    /// </summary>
    /// <param name="sourceColumn">The name of the source column within the data source.</param>
    /// <param name="destinationOrdinal">The ordinal position of the destination column within the destination table.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy Mapping(string sourceColumn, int destinationOrdinal)
    {
        var map = new SqlBulkCopyColumnMapping(sourceColumn, destinationOrdinal);
        _mapping.Add(map);
        return this;
    }

    /// <summary>
    /// Creates a new column mapping, using column ordinals to refer to source and destination columns.
    /// </summary>
    /// <param name="sourceColumnOrdinal">The ordinal position of the source column within the data source.</param>
    /// <param name="destinationOrdinal">The ordinal position of the destination column within the destination table.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy Mapping(int sourceColumnOrdinal, int destinationOrdinal)
    {
        var map = new SqlBulkCopyColumnMapping(sourceColumnOrdinal, destinationOrdinal);
        _mapping.Add(map);
        return this;
    }

    /// <summary>
    /// Creates a new column mapping using a strongly typed builder.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="builder">The entity mapping builder.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/></exception>
    public IDataBulkCopy Mapping<TEntity>(Action<DataBulkCopyMapping<TEntity>> builder)
        where TEntity : class
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        var dataMapping = new DataBulkCopyMapping<TEntity>(this);
        builder(dataMapping);

        return this;
    }

    /// <summary>
    /// Ignores the specified source column by removing it from the mapped columns collection.
    /// </summary>
    /// <param name="sourceColumn">The source column to remove from mapping.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy Ignore(string sourceColumn)
    {
        _ignoreColumns.Add(sourceColumn);
        return this;
    }

    /// <summary>
    /// Ignores the specified source column by removing it from the mapped columns collection.
    /// </summary>
    /// <param name="sourceColumnOrdinal">The ordinal position of the source column within the data source.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public IDataBulkCopy Ignore(int sourceColumnOrdinal)
    {
        _ignoreOrdinal.Add(sourceColumnOrdinal);
        return this;
    }


    /// <summary>
    /// Copies all items in the supplied <see cref="T:System.Collections.Generic.IEnumerable`1" /> to a destination table.
    /// </summary>
    /// <typeparam name="TEntity">The type of the data elements.</typeparam>
    /// <param name="data">An IEnumerable that will be copied to the destination table.</param>
    public void WriteToServer<TEntity>(IEnumerable<TEntity> data)
        where TEntity : class
    {
        using var dataReader = new ListDataReader<TEntity>(data);
        WriteToServer(dataReader);
    }

    /// <summary>
    /// Copies all rows from the supplied <see cref="DataRow" /> array to a destination table.
    /// </summary>
    /// <param name="rows">An array of DataRow objects that will be copied to the destination table.</param>
    public void WriteToServer(DataRow[] rows)
    {
        AssertDisposed();

        try
        {
            _dataSession.EnsureConnection();

            using var bulkCopy = Create();

            bulkCopy.WriteToServer(rows);
            bulkCopy.Close();
        }
        finally
        {
            _dataSession.ReleaseConnection();
            Dispose();
        }
    }

    /// <summary>
    /// Copies all rows in the supplied <see cref="DataTable" /> to a destination table.
    /// </summary>
    /// <param name="table">A DataTable whose rows will be copied to the destination table.</param>
    public void WriteToServer(DataTable table)
    {
        WriteToServer(table, 0);
    }

    /// <summary>
    /// Copies only rows that match the supplied row state in the supplied <see cref="DataTable" /> to a destination table.
    /// </summary>
    /// <param name="table">A DataTable whose rows will be copied to the destination table.</param>
    /// <param name="rowState">A value from the DataRowState enumeration. Only rows matching the row state are copied to the destination.</param>
    public void WriteToServer(DataTable table, DataRowState rowState)
    {
        AssertDisposed();

        try
        {
            // resolve auto map
            if (_autoMap == true)
                foreach (DataColumn column in table.Columns)
                    Mapping(column.ColumnName, column.ColumnName);

            _dataSession.EnsureConnection();

            using var bulkCopy = Create();

            bulkCopy.WriteToServer(table, rowState);
            bulkCopy.Close();
        }
        finally
        {
            _dataSession.ReleaseConnection();
            Dispose();
        }
    }

    /// <summary>
    /// Copies all rows in the supplied <see cref="IDataReader" /> to a destination table.
    /// </summary>
    /// <param name="reader">A IDataReader whose rows will be copied to the destination table.</param>
    public void WriteToServer(IDataReader reader)
    {
        AssertDisposed();

        try
        {
            _dataSession.EnsureConnection();

            using var bulkCopy = Create();

            bulkCopy.WriteToServer(reader);
            bulkCopy.Close();
        }
        finally
        {
            _dataSession.ReleaseConnection();
            Dispose();
        }
    }


    private SqlBulkCopy Create()
    {
        var sqlConnection = _dataSession.Connection as SqlConnection;
        if (sqlConnection == null)
            throw new InvalidOperationException(
                "Bulk-Copy only supported by SQL Server.  Make sure DataSession was created with a valid SqlConnection.");

        var sqlTransaction = _dataSession.Transaction as SqlTransaction;

        var bulkCopy = new SqlBulkCopy(sqlConnection, _options, sqlTransaction);
        bulkCopy.DestinationTableName = _destinationTable;

        if (_batchSize.HasValue)
            bulkCopy.BatchSize = _batchSize.Value;

        if (_bulkCopyTimeout.HasValue)
            bulkCopy.BulkCopyTimeout = _bulkCopyTimeout.Value;

        if (_enableStreaming.HasValue)
            bulkCopy.EnableStreaming = _enableStreaming.Value;

        if (_notifyAfter.HasValue)
            bulkCopy.NotifyAfter = _notifyAfter.Value;

        // filter out ignored columns
        var mappings = _mapping
            .Where(m => !_ignoreColumns.Contains(m.SourceColumn)
                && !_ignoreOrdinal.Contains(m.SourceOrdinal));

        foreach (var mapping in mappings)
            bulkCopy.ColumnMappings.Add(mapping);

        return bulkCopy;
    }
}
