using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FluentCommand.Batch;

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
    /// <exception cref="ValidationException">
    /// Missing key column.  Please select a column to be the key.
    /// or
    /// Missing column selection.  Please select a column to be included.
    /// </exception>
    public override void Visit(BatchJob batchJob)
    {
        // must have key
        var keyColumn = batchJob.Fields.FirstOrDefault(m => m.IsKey);
        if (keyColumn == null)
            throw new ValidationException("Missing key field.  Please select a field to be the key.");

        // must have column
        bool hasColumn = batchJob.Fields.Any(m => (m.Index.HasValue || m.Default.HasValue) && !m.IsKey);
        if (!hasColumn)
            throw new ValidationException("Missing field selection.  Please select a field to be included.");

        base.Visit(batchJob);
    }

    /// <summary>
    /// Visits the specified <see cref="FieldMapping" />.
    /// </summary>
    /// <param name="fieldMapping">The <see cref="FieldMapping" /> to visit.</param>
    /// <exception cref="ValidationException">Missing source column mapping.  Please select a source column.</exception>
    public override void VisitFieldMapping(FieldMapping fieldMapping)
    {
        var c = fieldMapping;

        if (c.Required && (c.Index == null || c.Index == -1))
            throw new ValidationException("Missing required field mapping.  Please select a source field for '" + c.DisplayName + "'.");

        base.VisitFieldMapping(fieldMapping);
    }
}