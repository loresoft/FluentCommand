using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using FluentCommand.Extensions;

namespace FluentCommand.Batch.Validation
{
    public class BatchValidator : IBatchValidator
    {
        private readonly HashSet<int> _duplicateHash;

        public BatchValidator()
        {
            _duplicateHash = new HashSet<int>();
        }


        public virtual void Reset()
        {
            _duplicateHash.Clear();
        }

        public virtual void ValidateRow(BatchJob batchJob, DataRow targetRow)
        {
            // check for DBNull or null
            CheckForNull(batchJob, targetRow);

            // look for duplicate key values
            CheckForDuplicate(batchJob, targetRow);
        }


        protected IList<string> SelectFields(BatchJob batchJob, IEnumerable<string> fields)
        {
            var selectFields = batchJob
                .Fields
                .Where(b => b.Index.HasValue || b.Default.HasValue)
                .Select(b => b.Name)
                .Intersect(fields)
                .ToList();

            return selectFields;
        }


        protected void CheckForNull(BatchJob batchJob, DataRow targetRow)
        {
            var requiredFields = batchJob
                .Fields
                .Where(b => (b.Index.HasValue || b.Default.HasValue) && b.CanBeNull == false)
                .Select(b => b.Name)
                .ToList();

            // first null
            var invalidField = requiredFields
                .FirstOrDefault(targetRow.IsNull);

            if (!invalidField.HasValue())
                return;

            string message = "Field '{0}' can not be null. {1}"
                .FormatWith(invalidField, targetRow.ItemArray.ToDelimitedString());

            throw new ValidationException(message);
        }

        protected void CheckForDuplicate(BatchJob batchJob, DataRow targetRow)
        {
            var keyFields = batchJob
                .Fields
                .Where(b => (b.Index.HasValue || b.Default.HasValue) && b.IsKey)
                .Select(b => b.Name)
                .ToList();

            var keyValues = keyFields
                .Select(k => targetRow[k])
                .ToList();

            // use a hash code of all the key values to check duplicate
            var hashCode = keyValues
                .Aggregate(HashCode.Seed, (hash, value) => hash.Combine(value));

            bool added = _duplicateHash.Add(hashCode);
            if (added)
                return;

            string message = "Duplicate key found.  Field: {0}, Value: {1}"
                .FormatWith(keyFields.ToDelimitedString(), keyValues.ToDelimitedString());

            throw new DuplicateException(message);
        }
    }
}