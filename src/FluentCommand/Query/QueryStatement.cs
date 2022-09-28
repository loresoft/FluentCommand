using System.Collections.Generic;

namespace FluentCommand;

public class QueryStatement
{
    public QueryStatement(string statement, IReadOnlyDictionary<string, object> parameters)
    {
        Statement = statement;
        Parameters = parameters;
    }

    public string Statement { get; }

    public IReadOnlyDictionary<string, object> Parameters { get; }
}
