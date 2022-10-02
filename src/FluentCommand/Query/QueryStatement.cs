namespace FluentCommand.Query;

public class QueryStatement : IQueryStatement
{
    public QueryStatement(string statement, IReadOnlyCollection<QueryParameter> parameters)
    {
        Statement = statement;
        Parameters = parameters;
    }

    public string Statement { get; }

    public IReadOnlyCollection<QueryParameter> Parameters { get; }
}
