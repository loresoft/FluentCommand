using System;
using System.Collections.Generic;
using System.Text;

namespace FluentCommand.Batch;

/// <summary>
/// A class that represents a visitor for <see cref="BatchJob"/>.
/// </summary>
public abstract class BatchJobVisitor
{
    /// <summary>
    /// Visits the specified <see cref="BatchJob"/>.
    /// </summary>
    /// <param name="batchJob">The <see cref="BatchJob"/> to visit.</param>
    public virtual void Visit(BatchJob batchJob)
    {
        foreach (var item in batchJob.Fields)
            VisitFieldMapping(item);

    }

    /// <summary>
    /// Visits the specified <see cref="FieldMapping"/>.
    /// </summary>
    /// <param name="fieldMapping">The <see cref="FieldMapping"/> to visit.</param>
    public virtual void VisitFieldMapping(FieldMapping fieldMapping)
    {
        foreach (var item in fieldMapping.MatchDefinitions)
            VisitFieldMatch(item);

    }

    /// <summary>
    /// Visits the specified <see cref="FieldMatch"/>.
    /// </summary>
    /// <param name="fieldMatch">The <see cref="FieldMatch"/> to visit.</param>
    public virtual void VisitFieldMatch(FieldMatch fieldMatch)
    {
    }
}

