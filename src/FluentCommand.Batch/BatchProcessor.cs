using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentCommand.Batch.Validation;
using FluentCommand.Extensions;
using FluentCommand.Merge;
using Microsoft.Extensions.Logging;

namespace FluentCommand.Batch
{
    public class BatchProcessor : IBatchProcessor
    {
        private readonly IBatchFactory _factory;
        private readonly ILogger<BatchProcessor> _logger;
        private readonly IDataSession _dataSession;

        public BatchProcessor(ILogger<BatchProcessor> logger, IBatchFactory factory, IDataSession dataSession)
        {
            _factory = factory;
            _logger = logger;
            _dataSession = dataSession;
        }


        public IEnumerable<DataMergeOutputRow> Process(BatchJob batchJob)
        {
            if (batchJob == null)
                throw new ArgumentNullException(nameof(batchJob));

            _logger.LogInformation("Running batch job: {job}", batchJob);

            // load into DataTable
            var dataTable = CreateTable(batchJob);

            // create DataMergeDefinition
            var mergeDefinition = CreateDefinition(batchJob);

            // run DataMerge
            var result = Merge(mergeDefinition, dataTable);

            return result;
        }

        public DataTable CreateTable(BatchJob batchJob)
        {
            var mappings = batchJob.Fields
                .Where(m => m.Index.HasValue || m.Default.HasValue)
                .ToList();

            var targetTable = CreateTable(mappings);

            _logger.LogDebug("Processing Job: '{job}', Mapped Columns: {columns}", batchJob, targetTable.Columns.Count);

            var validator = _factory.ResolveValidator(batchJob.ValidatorType);
            validator?.Reset();

            // skip first row, header
            for (var index = 1; index < batchJob.Data.Length; index++)
            {
                var sourceRow = batchJob.Data[index];
                CopyRow(batchJob, sourceRow, targetTable, mappings, validator);
            }

            _logger.LogInformation("Processed {rows} rows from job: '{job}'", targetTable.Rows.Count, batchJob);

            return targetTable;
        }


        private IEnumerable<DataMergeOutputRow> Merge(DataMergeDefinition mergeDefinition, DataTable dataTable)
        {
            _logger.LogDebug("Executing batch merge to: '{table}'", mergeDefinition.TargetTable);

            var result = _dataSession
                .MergeData(mergeDefinition)
                .ExecuteOutput(dataTable)
                .ToList();

            return result;
        }

        private DataMergeDefinition CreateDefinition(BatchJob batchJob)
        {
            var mergeDefinition = new DataMergeDefinition();
            mergeDefinition.TargetTable = batchJob.TargetTable;
            mergeDefinition.IncludeInsert = batchJob.CanInsert;
            mergeDefinition.IncludeUpdate = batchJob.CanUpdate;

            // fluent builder
            var mergeMapping = new DataMergeMapping(mergeDefinition);

            // map included columns
            foreach (var fieldMapping in batchJob.Fields.Where(m => m.Index.HasValue || m.Default.HasValue))
            {
                mergeMapping
                    .Column(fieldMapping.Name)
                    .Insert(fieldMapping.CanInsert)
                    .Update(fieldMapping.CanUpdate)
                    .NativeType(fieldMapping.NativeType)
                    .Key(fieldMapping.IsKey);
            }

            return mergeDefinition;
        }

        private void CopyRow(BatchJob batchJob, string[] sourceRow, DataTable targetTable, IEnumerable<FieldMapping> mappings, IBatchValidator validator)
        {
            try
            {
                // skip empty rows
                if (IsRowNull(sourceRow))
                    return;

                var targetRow = targetTable.NewRow();

                foreach (var mapping in mappings)
                {
                    if (!(mapping.Index.HasValue || mapping.Default.HasValue))
                        continue;

                    if (mapping.Default.HasValue)
                    {
                        var defaultValue = GetDefault(batchJob, mapping);
                        targetRow[mapping.Name] = defaultValue;
                    }
                    else
                    {
                        var sourceValue = sourceRow[mapping.Index.Value];

                        sourceValue.TryConvert(mapping.DataType, out var value);

                        var translator = _factory.ResolveTranslator(mapping.TranslatorType);
                        if (translator != null)
                            value = translator.Translate(mapping.TranslatorSource, value);

                        targetRow[mapping.Name] = value ?? DBNull.Value; // null must be set as DBNull
                    }
                }

                // ValidateRow should throw exception if can't continue
                validator?.ValidateRow(batchJob, targetRow);

                targetTable.Rows.Add(targetRow);
            }
            catch (DuplicateException dex)
            {
                _logger.LogError(dex, dex.Message);

                batchJob.Duplicates++;

                bool quit = batchJob.DuplicateHandling == BatchError.Quit;
                if (quit)
                    throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.GetBaseException().Message);

                batchJob.Errors++;

                bool quit = batchJob.ErrorHandling == BatchError.Quit ||
                    batchJob.Errors > batchJob.MaxErrors;

                if (quit)
                    throw;
            }
        }

        private object GetDefault(BatchJob batchJob, FieldMapping fieldMapping)
        {
            var fieldDefault = fieldMapping.Default;
            if (!fieldDefault.HasValue)
                return null;

            if (fieldDefault.Value == FieldDefault.CurrentDate)
                return DateTimeOffset.UtcNow;

            if (fieldDefault.Value == FieldDefault.UserName)
                return batchJob.UserName;

            if (fieldDefault.Value == FieldDefault.Static)
                return fieldMapping.DefaultValue;

            return null;
        }

        private DataTable CreateTable(IEnumerable<FieldMapping> mappings)
        {
            var targetTable = new DataTable("#Batch" + DateTime.Now.Ticks);
            foreach (var mapping in mappings)
            {
                var dataType = Nullable.GetUnderlyingType(mapping.DataType) ?? mapping.DataType;
                
                var dataColumn = targetTable.Columns.Add();
                dataColumn.ColumnName = mapping.Name;
                dataColumn.DataType = dataType;
            }
            return targetTable;
        }

        private bool IsRowNull(string[] sourceData)
        {
            return sourceData.All(string.IsNullOrWhiteSpace);
        }
    }
}