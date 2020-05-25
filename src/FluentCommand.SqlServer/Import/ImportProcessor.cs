using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentCommand.Extensions;

namespace FluentCommand.Import
{
    /// <summary>
    /// A data import processor
    /// </summary>
    public class ImportProcessor : IImportProcessor
    {

        /// <summary>
        /// Create a <see cref="DataTable"/> instance using the specified <paramref name="importDefinition" />.
        /// </summary>
        /// <param name="importDefinition">The import definition to create DataTable from.</param>
        /// <returns>An instance of <see cref="DataTable"/>.</returns>
        public virtual DataTable CreateTable(ImportDefinition importDefinition)
        {
            var dataTable = new DataTable("#Import" + DateTime.Now.Ticks);

            foreach (var definitionColumn in importDefinition.Fields)
            {
                var dataType = Nullable.GetUnderlyingType(definitionColumn.DataType) 
                    ?? definitionColumn.DataType;

                var dataColumn = new DataColumn
                {
                    ColumnName = definitionColumn.FieldName,
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
            var dataTable = CreateTable(importDefinition);
            return PopulateTable(dataTable, importDefinition, importData);
        }

        /// <summary>
        /// Populates the <see cref="DataTable"/> with the specified <paramref name="importData" />.
        /// </summary>
        /// <param name="dataTable">The data table to populate.</param>
        /// <param name="importDefinition">The import definition.</param>
        /// <param name="importData">The import data.</param>
        /// <returns></returns>
        public virtual DataTable PopulateTable(DataTable dataTable, ImportDefinition importDefinition, ImportData importData)
        {
            var rows = importData.Data.Length;
            if (rows == 0)
                return dataTable;

            var fields = importDefinition.Fields;
            var mappings = importData.Mappings;

            foreach (var row in importData.Data)
            {
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

                var input = row[index.Value];
                input.TryConvert(field.DataType, out var value);

                dataRow[field.FieldName] = value ?? DBNull.Value;
            }

            return dataRow;
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
        protected int? GetIndex(FieldDefinition fieldDefinition, List<FieldMap> mappings, int columns)
        {
            var name = fieldDefinition.FieldName;
            var field = mappings.FirstOrDefault(m => m.Name == name);

            if (field == null)
            {
                if (fieldDefinition.Required)
                    throw new InvalidOperationException($"Missing import index map for '{name}'");

                return null;
            }

            var index = field.Index;
            if (index >= columns)
                throw new IndexOutOfRangeException($"The import mapped index '{index}' for field '{name}' is out of range of '{columns}'");

            return index;
        }

    }
}
