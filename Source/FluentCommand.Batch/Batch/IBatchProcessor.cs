using System;
using System.Collections.Generic;
using FluentCommand.Merge;

namespace FluentCommand.Batch
{
    public interface IBatchProcessor
    {
        BatchJob ExtractFields(BatchJob batchJob, string fileName, string workingFile);
        IEnumerable<DataMergeOutputRow> Process(BatchJob batchJob, string connectionName);
    }
}