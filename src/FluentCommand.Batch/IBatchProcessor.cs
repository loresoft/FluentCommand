using System;
using System.Collections.Generic;
using System.Data;

using FluentCommand.Merge;

namespace FluentCommand.Batch;

public interface IBatchProcessor
{
    IEnumerable<DataMergeOutputRow> Process(BatchJob batchJob);
    DataTable CreateTable(BatchJob batchJob);
}