using System.Collections.Generic;

namespace FluentCommand;

public class QueryStatement
{
    public QueryStatement(string statement, IReadOnlyCollection<QueryParameter> parameters)
    {
        Statement = statement;
        Parameters = parameters;
    }

    public string Statement { get; }

    public IReadOnlyCollection<QueryParameter> Parameters { get; }
}
