using System;
using System.Collections.Generic;
using System.Data;

namespace FluentCommand.Batch
{
    public interface IBatchReader
    {
        DataTable ReadData(string filePath);

        IEnumerable<FieldIndex> ReadHeader(string filePath);
    }
}