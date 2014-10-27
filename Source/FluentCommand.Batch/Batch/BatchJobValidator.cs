using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FluentCommand.Batch
{
    /// <summary>
    /// A class to validate an instance of <see cref="T:BatchJob"/>
    /// </summary>
    public class BatchJobValidator : BatchJobVisitor
    {
        /// <summary>
        /// Validates the specified <see cref="T:BatchJob"/>.
        /// </summary>
        /// <param name="batchJob">The <see cref="T:BatchJob"/> to validate.</param>
        /// <returns></returns>
        public virtual bool Validate(BatchJob batchJob)
        {            
            Visit(batchJob);

            return true;
        }

        /// <summary>
        /// Visits the specified <see cref="BatchJob" />.
        /// </summary>
        /// <param name="batchJob">The <see cref="BatchJob" /> to visit.</param>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
        /// Missing key column.  Please select a column to be the key.
        /// or
        /// Missing column selection.  Please select a column to be included.
        /// </exception>
        public override void Visit(BatchJob batchJob)
        {
            // must have key
            var keyColumn = batchJob.SourceMapping.FirstOrDefault(m => m.IsKey);
            if (keyColumn == null || keyColumn.IsIncluded == false)
                throw new ValidationException("Missing key column.  Please select a column to be the key.");

            // must have column
            bool hasColumn = batchJob.SourceMapping.Any(m => m.IsIncluded && !m.IsKey);
            if (!hasColumn)
                throw new ValidationException("Missing column selection.  Please select a column to be included.");

            base.Visit(batchJob);
        }

        /// <summary>
        /// Visits the specified <see cref="FieldMapping" />.
        /// </summary>
        /// <param name="fieldMapping">The <see cref="FieldMapping" /> to visit.</param>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">Missing source column mapping.  Please select a source column.</exception>
        public override void VisitFieldMapping(FieldMapping fieldMapping)
        {
            var c = fieldMapping;

            if (c.IsIncluded && (c.Index == null || c.Index == -1))
                throw new ValidationException("Missing source column mapping.  Please select a source column for '" + c.DisplayName + "'.");

            base.VisitFieldMapping(fieldMapping);
        }
    }
}