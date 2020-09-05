using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentCommand.Extensions;

namespace FluentCommand.Import
{
    /// <summary>
    /// An import data validator
    /// </summary>
    public class ImportValidator : IImportValidator
    {
        private readonly HashSet<int> _duplicateHash;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportValidator"/> class.
        /// </summary>
        public ImportValidator()
        {
            _duplicateHash = new HashSet<int>();
        }


        /// <summary>
        /// Validates the specified <paramref name="targetRow" />.
        /// </summary>
        /// <param name="importDefinition">The import definition.</param>
        /// <param name="targetRow">The target row.</param>
        /// <returns></returns>
        public virtual Task ValidateRow(ImportDefinition importDefinition, DataRow targetRow)
        {
            // check for DBNull or null
            CheckForNull(importDefinition, targetRow);

            // look for duplicate key values
            CheckForDuplicate(importDefinition, targetRow);

            return Task.CompletedTask;
        }


        /// <summary>
        /// Checks specified <paramref name="targetRow" /> for null values.
        /// </summary>
        /// <param name="importDefinition">The import definition.</param>
        /// <param name="targetRow">The target row.</param>
        /// <exception cref="ValidationException">When a required field is null</exception>
        protected virtual void CheckForNull(ImportDefinition importDefinition, DataRow targetRow)
        {
            var requiredFields = importDefinition
                .Fields
                .Where(b => b.IsRequired || b.IsKey)
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

        /// <summary>
        /// Checks specified <paramref name="targetRow" /> for key duplicates.
        /// </summary>
        /// <param name="importDefinition">The import definition.</param>
        /// <param name="targetRow">The target row.</param>
        /// <exception cref="ValidationException">When key is duplicate</exception>
        protected virtual void CheckForDuplicate(ImportDefinition importDefinition, DataRow targetRow)
        {
            var keyFields = importDefinition
                .Fields
                .Where(b => b.IsKey)
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

            throw new ValidationException(message);
        }
    }
}