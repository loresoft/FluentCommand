using System;
using System.Linq;

namespace FluentCommand.Batch.Fluent
{
    public class BatchBuilder
    {
        private readonly BatchJob _batchJob;

        public BatchBuilder(BatchJob batchJob)
        {
            _batchJob = batchJob;
        }

        public BatchBuilder Entity<TEntity>()
            where TEntity : class, new()
        {
            _batchJob.EntityType = typeof(TEntity).FullName;
            return this;
        }

        public BatchBuilder Validator<TValicator>()
            where TValicator : IBatchValidator
        {
            _batchJob.ValidatorType = typeof(TValicator).FullName;
            return this;
        }

        public BatchBuilder Description(string description)
        {
            _batchJob.Description = description;
            return this;
        }

        public BatchBuilder TargetTable(string targetTable)
        {
            _batchJob.TargetTable = targetTable;
            return this;
        }

        public BatchBuilder CanInsert(bool value = true)
        {
            _batchJob.CanInsert = value;
            return this;
        }

        public BatchBuilder CanUpdate(bool value = true)
        {
            _batchJob.CanUpdate = value;
            return this;
        }

        public BatchBuilder CanDelete(bool value = true)
        {
            _batchJob.CanDelete = value;
            return this;
        }


        public BatchBuilder Map(Action<BatchFieldBuilder> builder)
        {
            var fieldMapping = new FieldMapping();

            var fieldBuilder = new BatchFieldBuilder(fieldMapping);
            builder(fieldBuilder);

            _batchJob.SourceMapping.Add(fieldMapping);

            return this;
        }

        public BatchBuilder Map(string name, Action<BatchFieldBuilder> builder)
        {
            var fieldMapping = _batchJob.SourceMapping.FirstOrDefault(m => m.Name == name);
            if (fieldMapping == null)
            {
                fieldMapping = new FieldMapping { Name = name };
                _batchJob.SourceMapping.Add(fieldMapping);
            }

            var fieldBuilder = new BatchFieldBuilder(fieldMapping);
            builder(fieldBuilder);

            return this;
        }

        public BatchFieldBuilder Map(string name)
        {
            var fieldMapping = _batchJob.SourceMapping.FirstOrDefault(m => m.Name == name);
            if (fieldMapping == null)
            {
                fieldMapping = new FieldMapping { Name = name };
                _batchJob.SourceMapping.Add(fieldMapping);
            }

            var fieldBuilder = new BatchFieldBuilder(fieldMapping);
            return fieldBuilder;
        }


        public static BatchJob Build(Action<BatchBuilder> builder)
        {
            var batchJob = new BatchJob();
            var batchBuilder = new BatchBuilder(batchJob);
            builder(batchBuilder);
            return batchJob;
        }
    }
}
