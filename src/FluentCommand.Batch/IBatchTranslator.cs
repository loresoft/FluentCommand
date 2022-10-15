using System;

namespace FluentCommand.Batch;

public interface IBatchTranslator
{
    string[] Sources { get; }

    object Translate(string source, object original);
}