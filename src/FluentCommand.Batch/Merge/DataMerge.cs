using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FluentCommand;
using FluentCommand.Extensions;

namespace FluentCommand.Merge
{
    /// <summary>
    /// Fluent class for a data merge operation.
    /// </summary>
    public class DataMerge : DisposableBase, IDataMerge
    {
        private readonly IDataSession _dataSession;
        private readonly DataMergeDefinition _mergeDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMerge"/> class.
        /// </summary>
        /// <param name="dataSession">The data session.</param>
        /// <param name="mergeDefinition">The data merge definition.</param>
        public DataMerge(IDataSession dataSession, DataMergeDefinition mergeDefinition)
        {
            _dataSession = dataSession;
            _mergeDefinition = mergeDefinition;
        }

        /// <summary>
        /// Sets a value indicating whether to insert data not found in <see cref="TargetTable" />.
        /// </summary>
        /// <param name="value"><c>true</c> to insert data not found; otherwise, <c>false</c>.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge " /> operation.
        /// </returns>
        public IDataMerge IncludeInsert(bool value = true)
        {
            _mergeDefinition.IncludeInsert = value;
            return this;
        }

        /// <summary>
        /// Sets a value indicating whether to update data found in <see cref="TargetTable" />.
        /// </summary>
        /// <param name="value"><c>true</c> to update data found; otherwise, <c>false</c>.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge " /> operation.
        /// </returns>
        public IDataMerge IncludeUpdate(bool value = true)
        {
            _mergeDefinition.IncludeUpdate = value;
            return this;
        }

        /// <summary>
        /// Sets a value indicating whether to delete data from <see cref="TargetTable" /> not found in source data.
        /// </summary>
        /// <param name="value"><c>true</c> to delete target data not in source data; otherwise, <c>false</c>.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge " /> operation.
        /// </returns>
        public IDataMerge IncludeDelete(bool value = true)
        {
            _mergeDefinition.IncludeDelete = value;
            return this;
        }

        /// <summary>
        /// Sets a value indicating whether to allow identity insert on the <see cref="TargetTable" />.
        /// </summary>
        /// <param name="value"><c>true</c> to allow identity insert; otherwise, <c>false</c>.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge " /> operation.
        /// </returns>
        public IDataMerge IdentityInsert(bool value = true)
        {
            _mergeDefinition.IdentityInsert = value;
            return this;
        }

        /// <summary>
        /// The name of target table to merge data into.
        /// </summary>
        /// <param name="value">The name of the target table.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge " /> operation.
        /// </returns>
        public IDataMerge TargetTable(string value)
        {
            _mergeDefinition.TargetTable = value;
            return this;
        }

        /// <summary>
        /// Start mapping the columns to merge using the fluent <paramref name="builder" />.
        /// </summary>
        /// <param name="builder">The fluent data mapping builder.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge " /> operation.
        /// </returns>
        public IDataMerge Map(Action<DataMergeMapping> builder)
        {
            var dataMapping = new DataMergeMapping(_mergeDefinition);
            builder(dataMapping);

            return this;
        }

        /// <summary>
        /// Start mapping the columns to merge using the strongly typed fluent <paramref name="builder" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="builder">The fluent data mapping builder.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a <see cref="DataMerge " /> operation.
        /// </returns>
        public IDataMerge Map<TEntity>(Action<DataMergeMapping<TEntity>> builder)
        {
            var dataMapping = new DataMergeMapping<TEntity>(_mergeDefinition);
            builder(dataMapping);

            return this;
        }


        /// <summary>
        /// Merges the specified <paramref name="data"/> into the <see cref="TargetTable"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="data">The data to be merged.</param>
        /// <returns>The number of rows processed.</returns>
        public int Merge<TEntity>(IEnumerable<TEntity> data)
        {
            // validate definition
            if (!Validate(_mergeDefinition))
                return 0;

            var ignoreNames = _mergeDefinition.Columns
                .Where(c => c.IsIgnored)
                .Select(c => c.SourceColumn);

            var dataTable = data.ToDataTable(ignoreNames);
            return Merge(dataTable);
        }

        /// <summary>
        /// Merges the specified <paramref name="table"/> into the <see cref="TargetTable"/>.
        /// </summary>
        /// <param name="table">The table data to be merged.</param>
        /// <returns>The number of rows processed.</returns>
        /// <exception cref="System.InvalidOperationException">Bulk-Copy only supported by SQL Server.  Make sure DataSession was create with a valid SqlConnection.</exception>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
        /// TargetTable is require for the merge definition.
        /// or
        /// At least one column is required for the merge definition.
        /// or
        /// At least one column is required to be marked as a key for the merge definition.
        /// or
        /// SourceColumn is require for column merge definition
        /// or
        /// NativeType is require for column merge definition
        /// </exception>
        public int Merge(DataTable table)
        {
            int result = 0;
            Merge(table, command => result = command.ExecuteNonQuery());

            return result;
        }


        /// <summary>
        /// Merges the specified <paramref name="data" /> into the <see cref="TargetTable" /> capturing the changes in the result.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="data">The data to be merged.</param>
        /// <returns>
        /// A collection of <see cref="T:DataMergeOutput`1" /> instances.
        /// </returns>
        public IEnumerable<DataMergeOutputRow> MergeOutput<TEntity>(IEnumerable<TEntity> data)
            where TEntity : class
        {
            // validate definition
            if (!Validate(_mergeDefinition))
                return null;

            var ignoreNames = _mergeDefinition.Columns
                .Where(c => c.IsIgnored)
                .Select(c => c.SourceColumn);

            var dataTable = data.ToDataTable(ignoreNames);
            return MergeOutput(dataTable);
        }

        /// <summary>
        /// Merges the specified <paramref name="table" /> into the <see cref="TargetTable" /> capturing the changes in the result.
        /// </summary>
        /// <param name="table">The table data to be merged.</param>
        /// <returns>A collection of the merge output in a dictionary.</returns>
        /// <exception cref="System.InvalidOperationException">Bulk-Copy only supported by SQL Server.  Make sure DataSession was create with a valid SqlConnection.</exception>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">TargetTable is require for the merge definition.
        /// or
        /// At least one column is required for the merge definition.
        /// or
        /// At least one column is required to be marked as a key for the merge definition.
        /// or
        /// SourceColumn is require for column merge definition
        /// or
        /// NativeType is require for column merge definition</exception>
        public IEnumerable<DataMergeOutputRow> MergeOutput(DataTable table)
        {
            // update definition to include output
            _mergeDefinition.IncludeOutput = true;

            // validate definition
            if (!Validate(_mergeDefinition))
                return null;

            var results = new List<DataMergeOutputRow>();
            var columns = _mergeDefinition.Columns
                .Where(c => c.IsIgnored == false)
                .ToList();

            // run merge capturing output
            Merge(table, command =>
            {
                using (IDataReader reader = command.ExecuteReader())
                {
                    var originalReader = new DataReaderWrapper(reader, DataMergeGenerator.OriginalPrefix);
                    var currentReader = new DataReaderWrapper(reader, DataMergeGenerator.CurrentPrefix);

                    while (reader.Read())
                    {
                        var output = new DataMergeOutputRow();

                        string action = reader.GetString("Action");
                        output.Action = action;

                        // copy to dictionary
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
                }
            });

            return results;
        }


        private void Merge(DataTable table, Action<IDbCommand> executeFactory)
        {
            // Step 1, validate definition
            if (!Validate(_mergeDefinition))
                return;

            try
            {
                _dataSession.EnsureConnection();

                var sqlConnection = _dataSession.Connection as SqlConnection;
                if (sqlConnection == null)
                    throw new InvalidOperationException(
                        "Bulk-Copy only supported by SQL Server.  Make sure DataSession was create with a valid SqlConnection.");

                var sqlTransaction = _dataSession.Transaction as SqlTransaction;

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

                    bulkCopy.WriteToServer(table);
                }

                // Step 4, run merge sql
                string mergeSql = DataMergeGenerator.BuildMerge(_mergeDefinition);
                using (var mergeCommand = _dataSession.Connection.CreateCommand())
                {
                    mergeCommand.CommandText = mergeSql;
                    mergeCommand.CommandType = CommandType.Text;
                    mergeCommand.Transaction = sqlTransaction;

                    // run merge with factory
                    executeFactory(mergeCommand);
                }
            }
            finally
            {
                _dataSession.ReleaseConnection();
            }
        }


        /// <summary>
        /// Validates the specified merge definition.
        /// </summary>
        /// <param name="mergeDefinition">The merge definition.</param>
        /// <returns></returns>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
        /// TargetTable is require for the merge definition.
        /// or
        /// At least one column is required for the merge definition.
        /// or
        /// At least one column is required to be marked as a key for the merge definition.
        /// or
        /// SourceColumn is require for column merge definition.
        /// or
        /// NativeType is require for column merge definition.
        /// </exception>
        public static bool Validate(DataMergeDefinition mergeDefinition)
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

                if (column.NativeType.IsNullOrEmpty())
                    throw new ValidationException("NativeType is require for column '{0}' merge definition.".FormatWith(column.SourceColumn));
            }

            return true;
        }
    }
}