using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;

using FluentCommand.Extensions;

using Microsoft.Data.SqlClient;

namespace FluentCommand.Merge;

/// <summary>
/// Provides a fluent API for configuring and executing SQL Server data merge operations, supporting insert, update, delete, and output of changes.
/// </summary>
public class DataMerge : DisposableBase, IDataMerge
{
    private readonly IDataSession _dataSession;
    private readonly DataMergeDefinition _mergeDefinition;

    private int _commandTimeout = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataMerge"/> class.
    /// </summary>
    /// <param name="dataSession">The data session used for the merge operation.</param>
    /// <param name="mergeDefinition">The data merge definition containing mapping and configuration.</param>
    public DataMerge(IDataSession dataSession, DataMergeDefinition mergeDefinition)
    {
        _dataSession = dataSession;
        _mergeDefinition = mergeDefinition;
    }

    /// <inheritdoc/>
    public IDataMerge IncludeInsert(bool value = true)
    {
        _mergeDefinition.IncludeInsert = value;
        return this;
    }

    /// <inheritdoc/>
    public IDataMerge IncludeUpdate(bool value = true)
    {
        _mergeDefinition.IncludeUpdate = value;
        return this;
    }

    /// <inheritdoc/>
    public IDataMerge IncludeDelete(bool value = true)
    {
        _mergeDefinition.IncludeDelete = value;
        return this;
    }

    /// <inheritdoc/>
    public IDataMerge IdentityInsert(bool value = true)
    {
        _mergeDefinition.IdentityInsert = value;
        return this;
    }

    /// <inheritdoc/>
    public IDataMerge TargetTable(string value)
    {
        _mergeDefinition.TargetTable = value;
        return this;
    }

    /// <inheritdoc/>
    public IDataMerge Map(Action<DataMergeMapping> builder)
    {
        var dataMapping = new DataMergeMapping(_mergeDefinition);
        builder(dataMapping);

        return this;
    }

    /// <inheritdoc/>
    public IDataMerge Map<TEntity>(Action<DataMergeMapping<TEntity>> builder)
        where TEntity : class
    {
        var dataMapping = new DataMergeMapping<TEntity>(_mergeDefinition);
        builder(dataMapping);

        return this;
    }

    /// <inheritdoc/>
    public IDataMerge Mode(DataMergeMode mergeMode)
    {
        _mergeDefinition.Mode = mergeMode;
        return this;
    }

    /// <inheritdoc/>
    public IDataMerge CommandTimeout(int timeout)
    {
        _commandTimeout = timeout;
        return this;
    }

    /// <inheritdoc/>
    public int Execute<TEntity>(IEnumerable<TEntity> data)
        where TEntity : class
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var ignoreNames = _mergeDefinition.Columns
            .Where(c => c.IsIgnored)
            .Select(c => c.SourceColumn);

        var rows = data.Count();
        using var listDataReader = new ListDataReader<TEntity>(data, ignoreNames);

        int result = 0;
        Merge(listDataReader, rows, command => result = command.ExecuteNonQuery());

        return result;
    }

    /// <inheritdoc/>
    public int Execute(DataTable tableData)
    {
        if (tableData == null)
            throw new ArgumentNullException(nameof(tableData));

        var rows = tableData.Rows.Count;
        var reader = tableData.CreateDataReader();

        int result = 0;
        Merge(reader, rows, command => result = command.ExecuteNonQuery());

        return result;
    }

    /// <inheritdoc/>
    public async Task<int> ExecuteAsync<TEntity>(IEnumerable<TEntity> data, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var ignoreNames = _mergeDefinition.Columns
            .Where(c => c.IsIgnored)
            .Select(c => c.SourceColumn);

        var rows = data.Count();
        using var listDataReader = new ListDataReader<TEntity>(data, ignoreNames);

        int result = 0;
        await MergeAsync(listDataReader, rows, cancellationToken, async (command, token) =>
        {
            result = await command
                .ExecuteNonQueryAsync(token)
                .ConfigureAwait(false);

        }).ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc/>
    public async Task<int> ExecuteAsync(DataTable tableData, CancellationToken cancellationToken = default)
    {
        var rows = tableData.Rows.Count;
        var reader = tableData.CreateDataReader();

        int result = 0;
        await MergeAsync(reader, rows, cancellationToken, async (command, token) =>
            {
                result = await command
                    .ExecuteNonQueryAsync(token)
                    .ConfigureAwait(false);

            }).ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc/>
    public IEnumerable<DataMergeOutputRow> ExecuteOutput<TEntity>(IEnumerable<TEntity> data)
        where TEntity : class
    {
        // update definition to include output
        _mergeDefinition.IncludeOutput = true;

        var results = Enumerable.Empty<DataMergeOutputRow>();

        var ignoreNames = _mergeDefinition.Columns
            .Where(c => c.IsIgnored)
            .Select(c => c.SourceColumn);

        var columns = _mergeDefinition.Columns
            .Where(c => c.IsIgnored == false)
            .ToList();

        var rows = data.Count();
        using var listDataReader = new ListDataReader<TEntity>(data, ignoreNames);

        // run merge capturing output
        Merge(listDataReader, rows, command =>
        {
            using var reader = command.ExecuteReader();
            results = CaptureOutput(reader, columns);
        });

        return results;
    }

    /// <inheritdoc/>
    public IEnumerable<DataMergeOutputRow> ExecuteOutput(DataTable table)
    {
        // update definition to include output
        _mergeDefinition.IncludeOutput = true;

        var results = Enumerable.Empty<DataMergeOutputRow>();
        var columns = _mergeDefinition.Columns
            .Where(c => c.IsIgnored == false)
            .ToList();

        var rows = table.Rows.Count;
        var dataTableReader = table.CreateDataReader();

        // run merge capturing output
        Merge(dataTableReader, rows, command =>
        {
            using var reader = command.ExecuteReader();
            results = CaptureOutput(reader, columns);
        });

        return results;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DataMergeOutputRow>> ExecuteOutputAsync<TEntity>(IEnumerable<TEntity> data, CancellationToken cancellationToken = default)
        where TEntity : class
    {
        // update definition to include output
        _mergeDefinition.IncludeOutput = true;

        var results = Enumerable.Empty<DataMergeOutputRow>();

        var ignoreNames = _mergeDefinition.Columns
            .Where(c => c.IsIgnored)
            .Select(c => c.SourceColumn);

        var columns = _mergeDefinition.Columns
            .Where(c => c.IsIgnored == false)
            .ToList();

        var rows = data.Count();
        using var listDataReader = new ListDataReader<TEntity>(data, ignoreNames);

        // run merge capturing output
        await MergeAsync(listDataReader, rows, cancellationToken, async (command, token) =>
        {
            using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
            results = await CaptureOutputAsync(reader, columns, cancellationToken);
        });

        return results;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DataMergeOutputRow>> ExecuteOutputAsync(DataTable table, CancellationToken cancellationToken = default)
    {
        // update definition to include output
        _mergeDefinition.IncludeOutput = true;

        var results = Enumerable.Empty<DataMergeOutputRow>();
        var columns = _mergeDefinition.Columns
            .Where(c => c.IsIgnored == false)
            .ToList();

        var rows = table.Rows.Count;
        var dataTableReader = table.CreateDataReader();

        // run merge capturing output
        await MergeAsync(dataTableReader, rows, cancellationToken, async (command, token) =>
        {
            using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
            results = await CaptureOutputAsync(reader, columns, cancellationToken);
        });

        return results;
    }

    /// <summary>
    /// Validates the specified merge definition for correctness and required configuration.
    /// </summary>
    /// <param name="mergeDefinition">The merge definition to validate.</param>
    /// <param name="isBulk"><c>true</c> if the merge mode is bulk copy; otherwise, <c>false</c>.</param>
    /// <returns><c>true</c> if the definition is valid; otherwise, an exception is thrown.</returns>
    /// <exception cref="ValidationException">
    /// Thrown if required properties are missing or invalid in the merge definition.
    /// </exception>
    public static bool Validate(DataMergeDefinition mergeDefinition, bool isBulk)
    {
        if (mergeDefinition.TargetTable.IsNullOrEmpty())
            throw new ValidationException("TargetTable is require for the merge definition.");

        // generate temporary name if not set
        if (mergeDefinition.TemporaryTable.IsNullOrEmpty())
            mergeDefinition.TemporaryTable = "#Merge" + DateTime.Now.Ticks;

        // make sure it starts with #
        if (!mergeDefinition.TemporaryTable.StartsWith("#"))
            mergeDefinition.TemporaryTable = "#" + mergeDefinition.TemporaryTable;

        // filter ignored columns
        var mergeColumns = mergeDefinition.Columns
            .Where(c => !c.IsIgnored)
            .ToList();

        if (mergeColumns.Count == 0)
            throw new ValidationException("At least one column is required for the merge definition.");

        if (mergeColumns.Count(c => c.IsKey) == 0)
            throw new ValidationException("At least one column is required to be marked as a key for the merge definition.");

        for (int i = 0; i < mergeColumns.Count; i++)
        {
            var column = mergeColumns[i];

            if (column.SourceColumn.IsNullOrEmpty())
                throw new ValidationException("SourceColumn is require for column index {0} merge definition.".FormatWith(i));

            // use source if no target
            if (column.TargetColumn.IsNullOrEmpty())
                column.TargetColumn = column.SourceColumn;

            if (isBulk && column.NativeType.IsNullOrEmpty())
                throw new ValidationException("NativeType is require for column '{0}' merge definition.".FormatWith(column.SourceColumn));
        }

        return true;
    }

    // Private helpers (not part of public API, so no XML docs needed)
    private void Merge(IDataReader reader, int rows, Action<DbCommand> executeFactory)
    {
        var isBulk = _mergeDefinition.Mode == DataMergeMode.BulkCopy
                     || (_mergeDefinition.Mode == DataMergeMode.Auto && rows > 1000);

        // Step 1, validate definition
        if (!Validate(_mergeDefinition, isBulk))
            return;

        try
        {
            _dataSession.EnsureConnection();

            var sqlConnection = _dataSession.Connection as SqlConnection;
            if (sqlConnection == null)
                throw new InvalidOperationException(
                    "Bulk-Copy only supported by SQL Server.  Make sure DataSession was create with a valid SqlConnection.");

            var sqlTransaction = _dataSession.Transaction as SqlTransaction;
            string mergeSql;

            if (isBulk)
            {
                // Step 2, create temp table
                string tableSql = DataMergeGenerator.BuildTable(_mergeDefinition);
                using (var tableCommand = _dataSession.Connection.CreateCommand())
                {
                    tableCommand.CommandText = tableSql;
                    tableCommand.CommandType = CommandType.Text;
                    tableCommand.Transaction = sqlTransaction;

                    tableCommand.ExecuteNonQuery();
                }

                // Step 3, bulk copy into temp table
                using (var bulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.Default, sqlTransaction))
                {
                    bulkCopy.DestinationTableName = _mergeDefinition.TemporaryTable;
                    bulkCopy.BatchSize = 1000;
                    foreach (var mergeColumn in _mergeDefinition.Columns.Where(c => !c.IsIgnored && c.CanBulkCopy))
                        bulkCopy.ColumnMappings.Add(mergeColumn.SourceColumn, mergeColumn.SourceColumn);

                    bulkCopy.WriteToServer(reader);
                }

                // Step 4, merge sql
                mergeSql = DataMergeGenerator.BuildMerge(_mergeDefinition);
            }
            else
            {
                // build merge from data
                mergeSql = DataMergeGenerator.BuildMerge(_mergeDefinition, reader);
            }

            // run merge statement
            using var mergeCommand = _dataSession.Connection.CreateCommand();

            mergeCommand.CommandText = mergeSql;
            mergeCommand.CommandType = CommandType.Text;
            mergeCommand.Transaction = sqlTransaction;

            if (_commandTimeout > 0)
                mergeCommand.CommandTimeout = _commandTimeout;

            // run merge with factory
            executeFactory(mergeCommand);
        }
        finally
        {
            _dataSession.ReleaseConnection();
        }
    }

    private async Task MergeAsync(IDataReader reader, int rows, CancellationToken cancellationToken, Func<DbCommand, CancellationToken, Task> executeFactory)
    {
        var isBulk = _mergeDefinition.Mode == DataMergeMode.BulkCopy
                     || (_mergeDefinition.Mode == DataMergeMode.Auto && rows > 1000);

        // Step 1, validate definition
        if (!Validate(_mergeDefinition, isBulk))
            return;

        try
        {
            await _dataSession
                .EnsureConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

            var sqlConnection = _dataSession.Connection as SqlConnection;
            if (sqlConnection == null)
                throw new InvalidOperationException(
                    "Bulk-Copy only supported by SQL Server.  Make sure DataSession was create with a valid SqlConnection.");

            var sqlTransaction = _dataSession.Transaction as SqlTransaction;
            string mergeSql;

            if (isBulk)
            {
                // Step 2, create temp table
                string tableSql = DataMergeGenerator.BuildTable(_mergeDefinition);
                using (var tableCommand = _dataSession.Connection.CreateCommand())
                {
                    tableCommand.CommandText = tableSql;
                    tableCommand.CommandType = CommandType.Text;
                    tableCommand.Transaction = sqlTransaction;

                    await tableCommand
                        .ExecuteNonQueryAsync(cancellationToken)
                        .ConfigureAwait(false);
                }

                // Step 3, bulk copy into temp table
                using (var bulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.Default, sqlTransaction))
                {
                    bulkCopy.DestinationTableName = _mergeDefinition.TemporaryTable;
                    bulkCopy.BatchSize = 1000;
                    foreach (var mergeColumn in _mergeDefinition.Columns.Where(c => !c.IsIgnored && c.CanBulkCopy))
                        bulkCopy.ColumnMappings.Add(mergeColumn.SourceColumn, mergeColumn.SourceColumn);

                    await bulkCopy
                        .WriteToServerAsync(reader, cancellationToken)
                        .ConfigureAwait(false);
                }

                // Step 4, merge sql
                mergeSql = DataMergeGenerator.BuildMerge(_mergeDefinition);
            }
            else
            {
                // build merge from data
                mergeSql = DataMergeGenerator.BuildMerge(_mergeDefinition, reader);
            }

            // run merge statement
            using var mergeCommand = _dataSession.Connection.CreateCommand();

            mergeCommand.CommandText = mergeSql;
            mergeCommand.CommandType = CommandType.Text;
            mergeCommand.Transaction = sqlTransaction;

            if (_commandTimeout > 0)
                mergeCommand.CommandTimeout = _commandTimeout;

            // run merge with factory
            await executeFactory(mergeCommand, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
#if NETCOREAPP3_0_OR_GREATER
            await _dataSession.ReleaseConnectionAsync();
#else
            _dataSession.ReleaseConnection();
#endif
        }
    }

    private static IEnumerable<DataMergeOutputRow> CaptureOutput(IDataReader reader, List<DataMergeColumn> columns)
    {
        List<DataMergeOutputRow> results = new();

        var originalReader = new DataReaderWrapper(reader, DataMergeGenerator.OriginalPrefix);
        var currentReader = new DataReaderWrapper(reader, DataMergeGenerator.CurrentPrefix);

        while (reader.Read())
        {
            var output = new DataMergeOutputRow();

            var action = reader.GetString("Action");
            output.Action = action;

            foreach (var column in columns)
            {
                var name = column.SourceColumn;

                var outputColumn = new DataMergeOutputColumn();
                outputColumn.Name = name;
                outputColumn.Original = originalReader.GetValue(name);
                outputColumn.Current = currentReader.GetValue(name);
                outputColumn.Type = currentReader.GetFieldType(name);

                output.Columns.Add(outputColumn);
            }

            results.Add(output);
        }

        return results;
    }

    private static async Task<IEnumerable<DataMergeOutputRow>> CaptureOutputAsync(DbDataReader reader, List<DataMergeColumn> columns, CancellationToken cancellationToken)
    {
        List<DataMergeOutputRow> results = new();

        var originalReader = new DataReaderWrapper(reader, DataMergeGenerator.OriginalPrefix);
        var currentReader = new DataReaderWrapper(reader, DataMergeGenerator.CurrentPrefix);

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            var output = new DataMergeOutputRow();

            string action = reader.GetString("Action");
            output.Action = action;

            foreach (var column in columns)
            {
                string name = column.SourceColumn;

                var outputColumn = new DataMergeOutputColumn();
                outputColumn.Name = name;
                outputColumn.Original = originalReader.GetValue(name);
                outputColumn.Current = currentReader.GetValue(name);
                outputColumn.Type = currentReader.GetFieldType(name);

                output.Columns.Add(outputColumn);
            }

            results.Add(output);
        }

        return results;
    }
}
