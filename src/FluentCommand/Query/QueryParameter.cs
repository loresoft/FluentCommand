using System;

namespace FluentCommand;

public class QueryParameter
{
    public QueryParameter(string name, object value, Type type)
    {
        Name = name;
        Value = value;
        Type = type;
    }

    public string Name { get; }

    public object Value { get; }

    public Type Type { get; }
}
