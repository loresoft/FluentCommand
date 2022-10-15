using System;

namespace FluentCommand.Batch.Fluent;

public class BatchMatchBuilder
{
    private readonly FieldMatch _fieldMatch;

    public BatchMatchBuilder(FieldMatch fieldMatch)
    {
        _fieldMatch = fieldMatch;
    }

    public BatchMatchBuilder Text(string value)
    {
        _fieldMatch.Text = value;
        return this;
    }

    public BatchMatchBuilder UseRegex(bool value = true)
    {
        _fieldMatch.UseRegex = value;
        return this;
    }

    public BatchMatchBuilder TranslatorSource(string value)
    {
        _fieldMatch.TranslatorSource = value;
        return this;
    }
}