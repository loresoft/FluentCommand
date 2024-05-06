using System.Data;

using FluentCommand.Extensions;
using FluentCommand.Merge;

namespace FluentCommand.Import;

/// <summary>
/// A data import processor
/// </summary>
public class ImportProcessor : IImportProcessor
{
    private readonly IDataSession _dataSession;
    private readonly ImportFactory _importFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportProcessor" /> class.
    /// </summary>
    /// <param name="dataSession">The data session.</param>
    /// <param name="importFactory">The service provider factory.</param>
    public ImportProcessor(IDataSession dataSession, ImportFactory importFactory)
    {
        _dataSession = dataSession;
        _importFactory = importFactory;
    }


    /// <summary>
    /// Import data using the specified <paramref name="importDefinition" /> and <paramref name="importData" />.
    /// </summary>
    /// <param name="importDefinition">The import definition.</param>
    /// <param name="importData">The import data.</param>
    /// <param name="username">The name of the user importing the data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The results of the import</returns>
    /// <exception cref="ArgumentNullException"><paramref name="importData" /> or <paramref name="importDefinition" /> is null</exception>
    public virtual async Task<ImportResult> ImportAsync(ImportDefinition importDefinition, ImportData importData, string username, CancellationToken cancellationToken = default)
    {
        if (importData == null)
            throw new ArgumentNullException(nameof(importData));
        if (importDefinition == null)
            throw new ArgumentNullException(nameof(importDefinition));

        var context = new ImportProcessContext(importDefinition, importData, username, _importFactory);

        var dataTable = CreateTable(context);
        await PopulateTable(context, dataTable);

        if (dataTable.Rows.Count == 0)
            return new ImportResult { Processed = 0, Errors = context.Errors };

        var mergeDefinition = CreateMergeDefinition(context);

        var result = await _dataSession
            .MergeData(mergeDefinition)
            .ExecuteAsync(dataTable, cancellationToken);

        return new ImportResult { Processed = result, Errors = context.Errors };
    }


    /// <summary>
    /// Create a <see cref="DataTable" /> instance using the specified <paramref name="importContext" />.
    /// </summary>
    /// <param name="importContext">The import context to create DataTable from.</param>
    /// <returns>
    /// An instance of <see cref="DataTable" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">importContext is null</exception>
    protected virtual DataTable CreateTable(ImportProcessContext importContext)
    {
        if (importContext == null)
            throw new ArgumentNullException(nameof(importContext));

        var dataTable = new DataTable("#Import" + DateTime.Now.Ticks);

        foreach (var field in importContext.MappedFields)
        {
            var dataType = Nullable.GetUnderlyingType(field.Definition.DataType)
                           ?? field.Definition.DataType;

            var dataColumn = new DataColumn
            {
                ColumnName = field.Definition.Name,
                DataType = dataType
            };

            dataTable.Columns.Add(dataColumn);
        }

        return dataTable;
    }

    /// <summary>
    /// Populates the <see cref="DataTable" /> with the specified <paramref name="importContext" />.
    /// </summary>
    /// <param name="importContext">The import context.</param>
    /// <param name="dataTable">The data table to populate.</param>
    /// <returns>
    /// The <see cref="DataTable" /> with the populated data.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// dataTable or importContext is null
    /// </exception>
    protected virtual async Task<DataTable> PopulateTable(ImportProcessContext importContext, DataTable dataTable)
    {
        if (dataTable == null)
            throw new ArgumentNullException(nameof(dataTable));

        if (importContext == null)
            throw new ArgumentNullException(nameof(importContext));

        var data = importContext.ImportData.Data;
        if (data == null || data.Length == 0)
            return dataTable;

        var rows = data.Length;
        var startIndex = importContext.ImportData.HasHeader ? 1 : 0;

        for (var index = startIndex; index < rows; index++)
        {
            var row = data[index];

            // skip empty row
            if (row.All(string.IsNullOrWhiteSpace))
                continue;

            var dataRow = dataTable.NewRow();

            var valid = await PopulateRow(importContext, dataRow, row);
            if (valid)
                dataTable.Rows.Add(dataRow);
        }

        return dataTable;
    }

    /// <summary>
    /// Populates the <see cref="DataRow" /> using the specified <paramref name="row" />.
    /// </summary>
    /// <param name="importContext">The import context.</param>
    /// <param name="dataRow">The data row to populate.</param>
    /// <param name="row">The imported source data row.</param>
    /// <returns>
    /// The <see cref="DataRow" /> with the populated data.
    /// </returns>
    protected virtual async Task<bool> PopulateRow(ImportProcessContext importContext, DataRow dataRow, string[] row)
    {
        try
        {
            foreach (var field in importContext.MappedFields)
            {
                if (field.Definition.Default.HasValue)
                {
                    dataRow[field.Definition.Name] = GetDefault(field.Definition, importContext.UserName);
                    continue;
                }

                var index = field.FieldMap.Index;
                if (!index.HasValue)
                    continue;

                var value = row[index.Value];

                var convertValue = await ConvertValue(importContext, field.Definition, value);

                dataRow[field.Definition.Name] = convertValue ?? DBNull.Value;
            }

            if (importContext.Definition.Validator == null)
                return true;

            var validator = importContext.GetService(importContext.Definition.Validator) as IImportValidator;
            if (validator == null)
                throw new InvalidOperationException($"Failed to create data row validator '{importContext.Definition.Validator}'");

            await validator.ValidateRow(importContext.Definition, dataRow);

            return true;
        }
        catch (Exception ex)
        {
            importContext.Errors.Add(ex);

            if (importContext.Errors.Count > importContext.Definition.MaxErrors)
                throw;

            return false;
        }
    }

    /// <summary>
    /// Converts the source string value into the correct data type using specified <paramref name="field" /> definition.
    /// </summary>
    /// <param name="importContext">The import context.</param>
    /// <param name="field">The field definition.</param>
    /// <param name="value">The source value.</param>
    /// <returns>
    /// The convert value.
    /// </returns>
    /// <exception cref="InvalidOperationException">Failed to create translator for field '{field.Name}'</exception>
    protected virtual async Task<object> ConvertValue(ImportProcessContext importContext, FieldDefinition field, string value)
    {
        if (field.Translator == null)
        {
            return ConvertExtensions.SafeConvert(field.DataType, value);
        }

        var translator = importContext.GetService(field.Translator) as IFieldTranslator;
        if (translator == null)
            throw new InvalidOperationException($"Failed to create translator for field '{field.Name}'");


        var translatedValue = await translator.Translate(value);
        return translatedValue;
    }

    /// <summary>
    /// Gets the default value for the specified <paramref name="fieldDefinition"/>.
    /// </summary>
    /// <param name="fieldDefinition">The field definition.</param>
    /// <param name="username">The username.</param>
    /// <returns></returns>
    protected virtual object GetDefault(FieldDefinition fieldDefinition, string username)
    {
        var fieldDefault = fieldDefinition?.Default;
        if (!fieldDefault.HasValue)
            return null;

        if (fieldDefault.Value == FieldDefault.CurrentDate)
            return DateTimeOffset.UtcNow;

        if (fieldDefault.Value == FieldDefault.Static)
            return fieldDefinition.DefaultValue;

        if (fieldDefault.Value == FieldDefault.UserName)
            return username;

        return null;
    }

    /// <summary>
    /// Creates a <see cref="DataMergeDefinition" /> from the specified <paramref name="importContext" />.
    /// </summary>
    /// <param name="importContext">The import context.</param>
    /// <returns>
    /// An instance of <see cref="DataMergeDefinition" />
    /// </returns>
    /// <exception cref="InvalidOperationException">Could not find matching field definition for data column</exception>
    protected virtual DataMergeDefinition CreateMergeDefinition(ImportProcessContext importContext)
    {
        var importDefinition = importContext.Definition;

        var mergeDefinition = new DataMergeDefinition();
        mergeDefinition.TargetTable = importDefinition.TargetTable;
        mergeDefinition.IncludeInsert = importDefinition.CanInsert;
        mergeDefinition.IncludeUpdate = importDefinition.CanUpdate;

        // fluent builder
        var mergeMapping = new DataMergeMapping(mergeDefinition);

        // map included columns
        foreach (var fieldMapping in importContext.MappedFields)
        {
            var fieldDefinition = fieldMapping.Definition;
            var nativeType = SqlTypeMapping.NativeType(fieldDefinition.DataType);

            mergeMapping
                .Column(fieldDefinition.Name)
                .Insert(fieldDefinition.CanInsert)
                .Update(fieldDefinition.CanUpdate)
                .Key(fieldDefinition.IsKey)
                .NativeType(nativeType);
        }

        return mergeDefinition;
    }
}
