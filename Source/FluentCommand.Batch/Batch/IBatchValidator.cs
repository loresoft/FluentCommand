using System;
using System.Data;

namespace FluentCommand.Batch
{
    public interface IBatchValidator
    {
        void Reset();

        void ValidateRow(BatchJob batchJob, DataRow targetRow);
    }
}