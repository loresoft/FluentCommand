using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentCommand.Extensions;
using FluentCommand.Merge;

namespace FluentCommand.Import
{
    /// <summary>
    /// A data import processor
    /// </summary>
    public class ImportProcessor : IImportProcessor
    {
        private readonly IDataSession _dataSession;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportProcessor"/> class.
        /// </summary>
        /// <param name="dataSession">The data session.</param>
        public ImportProcessor(IDataSession dataSession)
        {
            _dataSession = dataSession;
        }


        /// <summary>
        /// Create a <see cref="DataTable"/> instance using the specified <paramref name="importDefinition" />.
        /// </summary>
        /// <param name="importDefinition">The import definition to create DataTable from.</param>
        /// <returns>An instance of <see cref="DataTable"/>.</returns>
        public virtual DataTable CreateTable(ImportDefinition importDefinition)
        {
            if (importDefinition == null)
                throw new ArgumentNullException(nameof(importDefinition));

            var dataTable = new DataTable("#Import" + DateTime.Now.Ticks);

            foreach (var definitionColumn in importDefinition.Fields)
            {
                var dataType = Nullable.GetUnderlyingType(definitionColumn.DataType)
                    ?? definitionColumn.DataType;

                var dataColumn = new DataColumn
                {
                    ColumnName = definitionColumn.Name,
                    DataType = dataType
                };

                dataTable.Columns.Add(dataColumn);
            }

            return dataTable;
        }

        /// <summary>
        /// Create and populates a <see cref="DataTable" /> instance using the specified <paramref name="importDefinition" /> and <paramref name="importData" />.
        /// </summary>
        /// <param name="importDefinition">The import definition to create DataTable from.</param>
        /// <param name="importData">The import data.</param>
        /// <returns>An instance of <see cref="DataTable" />.</returns>
        public DataTable CreateTable(ImportDefinition importDefinition, ImportData importData)
        {
            if (importDefinition == null)
                throw new ArgumentNullException(nameof(importDefinition));

            if (importData == null)
                throw new ArgumentNullException(nameof(importData));

            var dataTable = CreateTable(importDefinition);
            return PopulateTable(dataTable, importDefinition, importData);
        }

        /// <summary>
        /// Merge data using the specified <paramref name="importDefinition" /> and <paramref name="importData"/>.
        /// </summary>
        /// <param name="importDefinition">The import definition.</param>
        /// <param name="importData">The import data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="importData" /> or <paramref name="importDefinition" /> is null 
        /// </exception>
        public async Task<ImportResult> MergeDataAsync(ImportDefinition importDefinition, ImportData importData, CancellationToken cancellationToken = default)
        {
            if (importData == null)
                throw new ArgumentNullException(nameof(importData));
            if (importDefinition == null)
                throw new ArgumentNullException(nameof(importDefinition));

            var dataTable = CreateTable(importDefinition, importData);

            var result = await MergeDataAsync(dataTable, importDefinition, cancellationToken);

            return result;
        }

        /// <summary>
        /// Merges the specified <paramref name="dataTable" /> using the <paramref name="importDefinition" />.
        /// </summary>
        /// <param name="dataTable">The data table to merge.</param>
        /// <param name="importDefinition">The import definition.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dataTable" /> or <paramref name="importDefinition" /> is null 
        /// </exception>
        public virtual async Task<ImportResult> MergeDataAsync(DataTable dataTable, ImportDefinition importDefinition, CancellationToken cancellationToken = default)
        {
            if (dataTable == null) 
                throw new ArgumentNullException(nameof(dataTable));
            if (importDefinition == null) 
                throw new ArgumentNullException(nameof(importDefinition));

            var mergeDefinition = CreateMergeDefinition(dataTable, importDefinition);

            var result = await _dataSession
                .MergeData(mergeDefinition)
                .ExecuteAsync(dataTable, cancellationToken);

            return new ImportResult { Processed = result };
        }

        
        /// <summary>
        /// Populates the <see cref="DataTable"/> with the specified <paramref name="importData" />.
        /// </summary>
        /// <param name="dataTable">The data table to populate.</param>
        /// <param name="importDefinition">The import definition.</param>
        /// <param name="importData">The import data.</param>
        /// <returns></returns>
        protected virtual DataTable PopulateTable(DataTable dataTable, ImportDefinition importDefinition, ImportData importData)
        {
            if (dataTable == null)
                throw new ArgumentNullException(nameof(dataTable));

            if (importDefinition == null)
                throw new ArgumentNullException(nameof(importDefinition));

            if (importData == null)
                throw new ArgumentNullException(nameof(importData));

            if (importData.Data == null || importData.Data.Length == 0)
                return dataTable;

            var rows = importData.Data.Length;
            var fields = importDefinition.Fields;
            var mappings = importData.Mappings;
            var startIndex = importData.HasHeader ? 1 : 0;

            for (var index = startIndex; index < rows; index++)
            {
                var row = importData.Data[index];

                // skip empty row
                if (row.All(string.IsNullOrWhiteSpace))
                    continue;

                var dataRow = dataTable.NewRow();

                PopulateRow(dataRow, fields, mappings, row);

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <summary>
        /// Populates the <see cref="DataRow"/> using the specified <paramref name="mappings"/>.
        /// </summary>
        /// <param name="dataRow">The data row to populate.</param>
        /// <param name="fields">The list of field definitions.</param>
        /// <param name="mappings">The field mappings.</param>
        /// <param name="row">The imported data row.</param>
        /// <returns></returns>
        protected virtual DataRow PopulateRow(DataRow dataRow, List<FieldDefinition> fields, List<FieldMap> mappings, string[] row)
        {
            var columns = row.Length;

            foreach (var field in fields)
            {
                var index = GetIndex(field, mappings, columns);
                if (!index.HasValue)
                    continue;

                var value = row[index.Value];

                var convertValue = ConvertValue(field, value);

                dataRow[field.Name] = convertValue ?? DBNull.Value;
            }

            return dataRow;
        }

        /// <summary>
        /// Converts the source string value into the correct data type using specified <paramref name="field"/> definition.
        /// </summary>
        /// <param name="field">The field definition.</param>
        /// <param name="value">The source value.</param>
        /// <returns>The convert value.</returns>
        protected virtual object ConvertValue(FieldDefinition field, string value)
        {
            value.TryConvert(field.DataType, out var convertValue);

            return convertValue;
        }

        /// <summary>
        /// Gets the field index for the specified field.
        /// </summary>
        /// <param name="fieldDefinition">The field definition.</param>
        /// <param name="mappings">The list of field mappings.</param>
        /// <param name="columns">The number of columns in the import file.</param>
        /// <returns>The field index</returns>
        /// <exception cref="InvalidOperationException">Missing import index map for field</exception>
        /// <exception cref="IndexOutOfRangeException">The import mapped index for a field is out of range</exception>
        protected virtual int? GetIndex(FieldDefinition fieldDefinition, List<FieldMap> mappings, int columns)
        {
            var name = fieldDefinition.Name;
            var field = mappings.FirstOrDefault(m => m.Name == name);

            if (field == null)
            {
                if (fieldDefinition.IsRequired)
                    throw new InvalidOperationException($"Missing import index map for '{name}'");

                return null;
            }

            var index = field.Index;
            if (index >= columns)
                throw new IndexOutOfRangeException($"The import mapped index '{index}' for field '{name}' is out of range of '{columns}'");

            return index;
        }

        /// <summary>
        /// Creates a <see cref="DataMergeDefinition"/> from the specified <paramref name="dataTable"/> and <paramref name="importDefinition"/>.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <param name="importDefinition">The import definition.</param>
        /// <returns>An instance of <see cref="DataMergeDefinition"/></returns>
        /// <exception cref="InvalidOperationException">Could not find matching field definition for data column</exception>
        protected virtual DataMergeDefinition CreateMergeDefinition(DataTable dataTable, ImportDefinition importDefinition)
        {
            var mergeDefinition = new DataMergeDefinition();
            mergeDefinition.TargetTable = importDefinition.TargetTable;
            mergeDefinition.IncludeInsert = importDefinition.CanInsert;
            mergeDefinition.IncludeUpdate = importDefinition.CanUpdate;

            // fluent builder
            var mergeMapping = new DataMergeMapping(mergeDefinition);

            // map included columns
            foreach (DataColumn column in dataTable.Columns)
            {
                var name = column.ColumnName;
                var fieldDefinition = importDefinition.Fields.FirstOrDefault(m => m.Name == name);
                if (fieldDefinition == null)
                    throw new InvalidOperationException($"Could not find matching field definition for column '{name}'");

                mergeMapping
                    .Column(name)
                    .Insert(fieldDefinition.CanInsert)
                    .Update(fieldDefinition.CanUpdate)
                    .Key(fieldDefinition.IsKey);
            }

            return mergeDefinition;
        }
    }
}
