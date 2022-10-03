namespace FluentCommand.Query;

public class QueryStatement : IQueryStatement
{
    public QueryStatement(string statement, IReadOnlyCollection<QueryParameter> parameters)
    {
        if (string.IsNullOrWhiteSpace(statement))
            throw new ArgumentException($"'{nameof(statement)}' cannot be null or whitespace.", nameof(statement));

        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        Statement = statement;
        Parameters = parameters;
    }

    public string Statement { get; }

    public IReadOnlyCollection<QueryParameter> Parameters { get; }
}
