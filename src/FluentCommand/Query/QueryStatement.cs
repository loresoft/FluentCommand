namespace FluentCommand.Query;

/// <summary>
/// class defining a sql query
/// </summary>
/// <seealso cref="FluentCommand.Query.IQueryStatement" />
public class QueryStatement : IQueryStatement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryStatement"/> class.
    /// </summary>
    /// <param name="statement">The sql statement.</param>
    /// <param name="parameters">The query parameters.</param>
    /// <exception cref="System.ArgumentException">statement cannot be null or whitespace. - statement</exception>
    /// <exception cref="System.ArgumentNullException">parameters cannot be null</exception>
    public QueryStatement(string statement, IReadOnlyCollection<QueryParameter> parameters)
    {
        if (string.IsNullOrWhiteSpace(statement))
            throw new ArgumentException($"'{nameof(statement)}' cannot be null or whitespace.", nameof(statement));

        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters));

        Statement = statement;
        Parameters = parameters;
    }

    /// <inheritdoc />
    public string Statement { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<QueryParameter> Parameters { get; }
}
