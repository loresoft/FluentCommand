using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentCommand.Batch.Validation;
using FluentCommand.Extensions;
using FluentCommand.Logging;
using FluentCommand.Merge;

namespace FluentCommand.Batch
{
    public class BatchProcessor : IBatchProcessor
    {
        private readonly IBatchFactory _factory;

        public BatchProcessor()
            : this(BatchFactory.Default)
        {
        }

        public BatchProcessor(IBatchFactory factory)
        {
            _factory = factory;
        }


        public BatchJob ExtractFields(BatchJob batchJob, string fileName, string workingFile)
        {
            if (batchJob == null)
                throw new ArgumentNullException("batchJob");
            if (workingFile == null)
                throw new ArgumentNullException("workingFile");
            if (!File.Exists(workingFile))
                throw new ArgumentException("The BatchJob working file '{0}' could not be found.".FormatWith(batchJob.WorkingFile), "workingFile");

            batchJob.FileName = fileName;
            batchJob.WorkingFile = workingFile;

            // add select row
            batchJob.SourceFields.Add(new FieldIndex { Name = "- Select -", Index = -1 });

            var reader = _factory.ResolveReader(batchJob.ReaderType);

            // read headers                       
            var sourceFields = reader.ReadHeader(workingFile);
            batchJob.SourceFields.AddRange(sourceFields);

            // try match
            foreach (var fieldMapping in batchJob.SourceMapping)
            {
                var match = FindField(batchJob, fieldMapping);
                if (match == null)
                    continue;

                // include if match
                fieldMapping.Index = match.Index;
                fieldMapping.IsIncluded = true;

                if (!fieldMapping.CanBeKey)
                    continue;

                // if match and only key, mark as key
                bool isOnlyKey = batchJob.SourceMapping.Count(m => m.CanBeKey) == 1;
                if (isOnlyKey)
                    fieldMapping.IsKey = true;
            }

            return batchJob;
        }

        public IEnumerable<DataMergeOutputRow> Process(BatchJob batchJob, string connectionName)
        {
            if (batchJob == null)
                throw new ArgumentNullException("batchJob");

            Logger.Info()
                .Message("Running batch job with file: '{0}'", batchJob.WorkingFile)
                .Write();

            // reset all cache
            _factory.Reset();

            // load file into DataTable
            var dataTable = LoadData(batchJob);

            // create DataMergeDefinition
            var mergeDefinition = CreateDefinition(batchJob);

            // run DataMerge
            var result = Merge(mergeDefinition, dataTable, connectionName);

            return result;
        }



        private IEnumerable<DataMergeOutputRow> Merge(DataMergeDefinition mergeDefinition, DataTable dataTable, string connectionName)
        {
            Logger.Debug()
                .Message("Executing batch merge to: '{0}'", mergeDefinition.TargetTable)
                .Write();

            List<DataMergeOutputRow> result;
            using (var session = new DataSession(connectionName))
            {
                result = session
                    .MergeData(mergeDefinition)
                    .MergeOutput(dataTable)
                    .ToList();
            }
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
            foreach (var fieldMapping in batchJob.SourceMapping.Where(c => c.IsIncluded))
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

        private DataTable LoadData(BatchJob batchJob)
        {
            if (batchJob == null)
                throw new ArgumentNullException("batchJob");

            if (!File.Exists(batchJob.WorkingFile))
                throw new InvalidOperationException("The BatchJob working file '{0}' could not be found.".FormatWith(batchJob.WorkingFile));


            var mappings = batchJob.SourceMapping
                .Where(m => m.IsIncluded && (m.Index.HasValue || m.Default.HasValue))
                .ToList();


            var targetTable = CreateTargetTable(mappings);

            Logger.Debug()
                .Message("Processing working file: '{0}', Mapped Columns: {1}", batchJob.WorkingFile, targetTable.Columns.Count)
                .Write();

            var reader = _factory.ResolveReader(batchJob.ReaderType);

            // read source data
            var sourceTable = reader.ReadData(batchJob.WorkingFile);

            var validator = _factory.ResolveValidator(batchJob.ValidatorType);
            if (validator != null)
                validator.Reset();

            foreach (DataRow sourceRow in sourceTable.Rows)
                CopyRow(batchJob, sourceRow, targetTable, mappings, validator);

            Logger.Debug()
                .Message("Processed {0} rows from file: '{1}'", targetTable.Rows.Count, batchJob.WorkingFile)
                .Write();

            return targetTable;
        }

        private void CopyRow(BatchJob batchJob, DataRow sourceRow, DataTable targetTable, IEnumerable<FieldMapping> mappings, IBatchValidator validator)
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

                    var sourceValue = mapping.Index.HasValue
                        ? sourceRow[mapping.Index.Value]
                        : GetDefault(batchJob, mapping);

                    var translator = _factory.ResolveTranslator(mapping.TranslatorType);
                    if (translator != null)
                        sourceValue = translator.Translate(mapping.TranslatorSource, sourceValue);

                    targetRow[mapping.Name] = sourceValue ?? DBNull.Value; // null must be set as DBNull
                }

                // ValidateRow should throw exception if can't continue
                if (validator != null)
                    validator.ValidateRow(batchJob, targetRow);

                targetTable.Rows.Add(targetRow);
            }
            catch (DuplicateException dex)
            {
                Logger.Error()
                    .Message(dex.Message)
                    .Exception(dex)
                    .Write();

                batchJob.Duplicates++;

                bool quit = batchJob.DuplicateHandling == BatchError.Quit;
                if (quit)
                    throw;
            }
            catch (Exception ex)
            {
                Logger.Error()
                    .Message(ex.GetBaseException().Message)
                    .Exception(ex)
                    .Write();

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
                return DateTime.Now;

            if (fieldDefault.Value == FieldDefault.UserName)
                return batchJob.UserName;

            if (fieldDefault.Value == FieldDefault.Static)
                return fieldMapping.DefaultValue;

            return null;
        }

        private DataTable CreateTargetTable(IEnumerable<FieldMapping> mappings)
        {
            var targetTable = new DataTable();
            foreach (var mapping in mappings)
            {
                var dataColumn = targetTable.Columns.Add();
                dataColumn.ColumnName = mapping.Name;
                dataColumn.DataType = mapping.DataType.ToType();
            }
            return targetTable;
        }

        private bool IsRowNull(DataRow sourceRow)
        {
            var sourceData = sourceRow.ItemArray;
            return sourceData.All(d => Convert.IsDBNull(d) || string.IsNullOrWhiteSpace(d as string));

        }

        private static FieldIndex FindField(BatchJob batchJob, FieldMapping fieldMapping)
        {
            FieldIndex match;

            //first try match definitions 
            foreach (var matchDefinition in fieldMapping.MatchDefinitions)
            {
                if (matchDefinition.Text.IsNullOrEmpty())
                    continue;

                string text = matchDefinition.Text;

                match = matchDefinition.UseRegex
                    ? batchJob.SourceFields.FirstOrDefault(f => Regex.IsMatch(f.Name, text, RegexOptions.IgnoreCase))
                    : batchJob.SourceFields.FirstOrDefault(f => string.Equals(f.Name, text, StringComparison.OrdinalIgnoreCase));

                if (match == null)
                    continue;

                fieldMapping.TranslatorSource = matchDefinition.TranslatorSource;
                return match;
            }

            // next try name match
            var name = fieldMapping.Name;
            match = batchJob.SourceFields.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));

            return match;
        }
    }
}