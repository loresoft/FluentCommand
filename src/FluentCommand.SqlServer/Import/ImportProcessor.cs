using System;
using System.Data;

using FluentCommand.Extensions;
using FluentCommand.Merge;

using Microsoft.Extensions.DependencyInjection;

namespace FluentCommand.Import;

/// <summary>
/// Processes data imports by transforming, validating, and merging imported data into a target data store.
/// </summary>
public class ImportProcessor : IImportProcessor
{
    private readonly IDataSession _dataSession;
    private readonly IServiceProvider _serviceProvider;

    private IImportValidator _importValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportProcessor"/> class.
    /// </summary>
    /// <param name="dataSession">The data session used for database operations.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies such as translators and validators.</param>
    public ImportProcessor(IDataSession dataSession, IServiceProvider serviceProvider)
    {
        _dataSession = dataSession;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Imports data using the specified import definition and import data.
    /// Transforms, validates, and merges the data into the target table as defined by the import configuration.
    /// </summary>
    /// <param name="importDefinition">The <see cref="ImportDefinition"/> describing the import configuration and rules.</param>
    /// <param name="importData">The <see cref="ImportData"/> containing the data and field mappings to be imported.</param>
    /// <param name="username">The name of the user performing the import operation.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// An <see cref="ImportResult"/> containing the number of processed rows and any errors encountered.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="importData"/> or <paramref name="importDefinition"/> is <c>null</c>.</exception>
    public virtual async Task<ImportResult> ImportAsync(ImportDefinition importDefinition, ImportData importData, string username, CancellationToken cancellationToken = default)
    {
        if (importData == null)
            throw new ArgumentNullException(nameof(importData));
        if (importDefinition == null)
            throw new ArgumentNullException(nameof(importDefinition));

        // validator is shared for entire import process
        _importValidator = GetValidator(importDefinition);

        var context = new ImportProcessContext(_serviceProvider, importDefinition, importData, username);

        var dataTable = CreateTable(context);
        await PopulateTable(context, dataTable);

        if (dataTable.Rows.Count == 0)
            return new ImportResult { Processed = 0, Errors = context.Errors?.ConvertAll(e => e.Message) };

        var mergeDefinition = CreateMergeDefinition(context);

        var result = await _dataSession
            .MergeData(mergeDefinition)
            .ExecuteAsync(dataTable, cancellationToken);

        return new ImportResult { Processed = result, Errors = context.Errors?.ConvertAll(e => e.Message) };
    }

    /// <summary>
    /// Creates a <see cref="DataTable"/> instance based on the mapped fields in the specified import context.
    /// The table schema is generated according to the field definitions and their data types.
    /// </summary>
    /// <param name="importContext">The <see cref="ImportProcessContext"/> containing field mappings and definitions.</param>
    /// <returns>
    /// A <see cref="DataTable"/> with columns corresponding to the mapped fields.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="importContext"/> is <c>null</c>.</exception>
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
    /// Populates the specified <see cref="DataTable"/> with data from the import context.
    /// Each row is transformed and validated according to the field definitions and mappings.
    /// </summary>
    /// <param name="importContext">The <see cref="ImportProcessContext"/> containing the import data and mappings.</param>
    /// <param name="dataTable">The <see cref="DataTable"/> to populate with imported data.</param>
    /// <returns>
    /// The populated <see cref="DataTable"/> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dataTable"/> or <paramref name="importContext"/> is <c>null</c>.</exception>
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
    /// Populates a <see cref="DataRow"/> with values from the specified source row, applying field transformations and validation as needed.
    /// </summary>
    /// <param name="importContext">The <see cref="ImportProcessContext"/> providing field mappings and validation logic.</param>
    /// <param name="dataRow">The <see cref="DataRow"/> to populate.</param>
    /// <param name="row">The source data row as an array of strings.</param>
    /// <returns>
    /// <c>true</c> if the row is valid and should be included; otherwise, <c>false</c>.
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

            if (_importValidator != null)
                await _importValidator.ValidateRow(importContext.Definition, dataRow);

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
    /// Converts the source string value into the correct data type for the specified field definition.
    /// If a translator is configured, it is used to transform the value; otherwise, a safe conversion is performed.
    /// </summary>
    /// <param name="importContext">The <see cref="ImportProcessContext"/> for resolving translators.</param>
    /// <param name="field">The <see cref="FieldDefinition"/> describing the field and its transformation options.</param>
    /// <param name="value">The source value as a string.</param>
    /// <returns>
    /// The converted value, or the result of the translator if configured.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if a configured translator cannot be resolved.</exception>
    protected virtual async Task<object> ConvertValue(ImportProcessContext importContext, FieldDefinition field, string value)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        if (field.Translator != null)
        {
            if (_serviceProvider.GetService(field.Translator) is not IFieldTranslator translator)
                throw new InvalidOperationException($"Failed to create translator '{field.Translator}' for field '{field.Name}'");

            return await translator.Translate(value);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        if (field.TranslatorKey != null)
        {
            var translator = _serviceProvider.GetKeyedService<IFieldTranslator>(field.TranslatorKey);
            if (translator == null)
                throw new InvalidOperationException($"Failed to create translator with service key '{field.TranslatorKey}' for field '{field.Name}'");

            return await translator.Translate(value);
        }

        return ConvertExtensions.SafeConvert(field.DataType, value);
    }

    /// <summary>
    /// Gets the default value for the specified field definition, based on the configured <see cref="FieldDefault"/> option.
    /// </summary>
    /// <param name="fieldDefinition">The <see cref="FieldDefinition"/> for which to get the default value.</param>
    /// <param name="username">The username to use if the default is <see cref="FieldDefault.UserName"/>.</param>
    /// <returns>
    /// The default value for the field, or <c>null</c> if no default is configured.
    /// </returns>
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
    /// Creates a <see cref="DataMergeDefinition"/> for the import operation, mapping the import fields to the target table columns.
    /// </summary>
    /// <param name="importContext">The <see cref="ImportProcessContext"/> containing the import definition and mapped fields.</param>
    /// <returns>
    /// A configured <see cref="DataMergeDefinition"/> instance for the merge operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if a field definition cannot be mapped to a data column.</exception>
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

    protected virtual IImportValidator GetValidator(ImportDefinition importDefinition)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        if (importDefinition.Validator != null)
        {
            var validator = _serviceProvider.GetService(importDefinition.Validator);
            if (validator is not IImportValidator importValidator)
                throw new InvalidOperationException($"Failed to create data row validator '{importDefinition.Validator}'");

            return importValidator;
        }
#pragma warning restore CS0618 // Type or member is obsolete

        if (importDefinition.ValidatorKey != null)
        {
            var validator = _serviceProvider.GetKeyedService<IImportValidator>(importDefinition.ValidatorKey);
            if (validator == null)
                throw new InvalidOperationException($"Failed to create data row validator with service key '{importDefinition.ValidatorKey}'");

            return validator;
        }

        return null;
    }
}
