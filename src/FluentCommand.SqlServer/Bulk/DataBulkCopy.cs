using System.Data;
using System.Linq.Expressions;

using FluentCommand.Extensions;

using Microsoft.Data.SqlClient;

namespace FluentCommand.Bulk;

/// <summary>
/// Provides a fluent API for performing <see cref="SqlBulkCopy"/> operations to efficiently copy large amounts of data into a SQL Server table.
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
    /// Initializes a new instance of the <see cref="DataBulkCopy"/> class for the specified data session and destination table.
    /// </summary>
    /// <param name="dataSession">The data session used for the bulk copy operation.</param>
    /// <param name="destinationTable">The name of the destination table in SQL Server.</param>
    public DataBulkCopy(IDataSession dataSession, string destinationTable)
    {
        _mapping = new List<SqlBulkCopyColumnMapping>();
        _ignoreColumns = new List<string>();
        _ignoreOrdinal = new List<int>();

        _dataSession = dataSession;
        _destinationTable = destinationTable;
    }

    /// <inheritdoc/>
    public IDataBulkCopy AutoMap(bool value = true)
    {
        _autoMap = value;
        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy BatchSize(int value)
    {
        _batchSize = value;
        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy BulkCopyTimeout(int value)
    {
        _bulkCopyTimeout = value;
        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy EnableStreaming(bool value = true)
    {
        _enableStreaming = value;
        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy NotifyAfter(int value)
    {
        _notifyAfter = value;
        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy KeepIdentity(bool value = true)
    {
        _options = value
            ? _options.SetFlagOn(SqlBulkCopyOptions.KeepIdentity)
            : _options.SetFlagOff(SqlBulkCopyOptions.KeepIdentity);

        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy CheckConstraints(bool value = true)
    {
        _options = value
            ? _options.SetFlagOn(SqlBulkCopyOptions.CheckConstraints)
            : _options.SetFlagOff(SqlBulkCopyOptions.CheckConstraints);

        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy TableLock(bool value = true)
    {
        _options = value
            ? _options.SetFlagOn(SqlBulkCopyOptions.TableLock)
            : _options.SetFlagOff(SqlBulkCopyOptions.TableLock);

        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy KeepNulls(bool value = true)
    {
        _options = value
            ? _options.SetFlagOn(SqlBulkCopyOptions.KeepNulls)
            : _options.SetFlagOff(SqlBulkCopyOptions.KeepNulls);

        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy FireTriggers(bool value = true)
    {
        _options = value
            ? _options.SetFlagOn(SqlBulkCopyOptions.FireTriggers)
            : _options.SetFlagOff(SqlBulkCopyOptions.FireTriggers);

        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy UseInternalTransaction(bool value = true)
    {
        _options = value
            ? _options.SetFlagOn(SqlBulkCopyOptions.UseInternalTransaction)
            : _options.SetFlagOff(SqlBulkCopyOptions.UseInternalTransaction);

        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy Mapping(string sourceColumn, string destinationColumn)
    {
        var map = new SqlBulkCopyColumnMapping(sourceColumn, destinationColumn);
        _mapping.Add(map);
        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy Mapping(int sourceColumnOrdinal, string destinationColumn)
    {
        var map = new SqlBulkCopyColumnMapping(sourceColumnOrdinal, destinationColumn);
        _mapping.Add(map);
        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy Mapping(string sourceColumn, int destinationOrdinal)
    {
        var map = new SqlBulkCopyColumnMapping(sourceColumn, destinationOrdinal);
        _mapping.Add(map);
        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy Mapping(int sourceColumnOrdinal, int destinationOrdinal)
    {
        var map = new SqlBulkCopyColumnMapping(sourceColumnOrdinal, destinationOrdinal);
        _mapping.Add(map);
        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy Mapping<TEntity>(Action<DataBulkCopyMapping<TEntity>> builder)
        where TEntity : class
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        var dataMapping = new DataBulkCopyMapping<TEntity>(this);
        builder(dataMapping);

        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy Ignore(string sourceColumn)
    {
        _ignoreColumns.Add(sourceColumn);
        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy Ignore(int sourceColumnOrdinal)
    {
        _ignoreOrdinal.Add(sourceColumnOrdinal);
        return this;
    }

    /// <inheritdoc/>
    public IDataBulkCopy Ignore<TEntity, TValue>(Expression<Func<TEntity, TValue>> sourceProperty)
        where TEntity : class
    {
        return Mapping<TEntity>(b => b.Ignore(sourceProperty));
    }

    /// <inheritdoc/>
    public void WriteToServer<TEntity>(IEnumerable<TEntity> data)
        where TEntity : class
    {
        using var dataReader = new ListDataReader<TEntity>(data);
        WriteToServer(dataReader);
    }

    /// <inheritdoc/>
    public async Task WriteToServerAsync<TEntity>(IEnumerable<TEntity> data, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        using var dataReader = new ListDataReader<TEntity>(data);
        await WriteToServerAsync(dataReader, cancellationToken);
    }


    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task WriteToServerAsync(DataRow[] rows, CancellationToken cancellationToken = default)
    {
        AssertDisposed();

        try
        {
            await _dataSession.EnsureConnectionAsync(cancellationToken);

            using var bulkCopy = Create();

            await bulkCopy.WriteToServerAsync(rows, cancellationToken);

            bulkCopy.Close();
        }
        finally
        {
            _dataSession.ReleaseConnection();
            Dispose();
        }
    }


    /// <inheritdoc/>
    public void WriteToServer(DataTable table)
    {
        WriteToServer(table, 0);
    }

    /// <inheritdoc/>
    public void WriteToServer(DataTable table, DataRowState rowState)
    {
        AssertDisposed();

        try
        {
            ApplyAutoMapping(table);

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

    /// <inheritdoc/>
    public async Task WriteToServerAsync(DataTable table, DataRowState rowState = 0, CancellationToken cancellationToken = default)
    {
        AssertDisposed();

        try
        {
            ApplyAutoMapping(table);

            await _dataSession.EnsureConnectionAsync(cancellationToken);

            using var bulkCopy = Create();

            await bulkCopy.WriteToServerAsync(table, rowState, cancellationToken);

            bulkCopy.Close();
        }
        finally
        {
            _dataSession.ReleaseConnection();
            Dispose();
        }
    }


    /// <inheritdoc/>
    public void WriteToServer(IDataReader reader)
    {
        AssertDisposed();

        try
        {
            ApplyAutoMapping(reader);

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

    /// <inheritdoc/>
    public async Task WriteToServerAsync(IDataReader reader, CancellationToken cancellationToken = default)
    {
        AssertDisposed();

        try
        {
            ApplyAutoMapping(reader);

            await _dataSession.EnsureConnectionAsync(cancellationToken);

            using var bulkCopy = Create();

            await bulkCopy.WriteToServerAsync(reader, cancellationToken);

            bulkCopy.Close();
        }
        finally
        {
            _dataSession.ReleaseConnection();
            Dispose();
        }
    }


    /// <summary>
    /// Applies automatic column mappings based on the <see cref="IDataReader"/> field names.
    /// </summary>
    /// <param name="reader">The data reader containing the field schema.</param>
    private void ApplyAutoMapping(IDataReader reader)
    {
        if (_autoMap != true)
            return;

        for (int i = 0; i < reader.FieldCount; i++)
        {
            var name = reader.GetName(i);
            Mapping(name, name);
        }
    }

    /// <summary>
    /// Applies automatic column mappings based on the <see cref="DataTable"/> column names.
    /// </summary>
    /// <param name="table">The data table containing the column schema.</param>
    private void ApplyAutoMapping(DataTable table)
    {
        if (_autoMap != true)
            return;

        foreach (DataColumn column in table.Columns)
            Mapping(column.ColumnName, column.ColumnName);
    }


    /// <summary>
    /// Creates and configures a <see cref="SqlBulkCopy"/> instance based on the current settings and mappings.
    /// </summary>
    /// <returns>A configured <see cref="SqlBulkCopy"/> instance ready for bulk copy operations.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the underlying connection is not a <see cref="SqlConnection"/>.
    /// </exception>
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
