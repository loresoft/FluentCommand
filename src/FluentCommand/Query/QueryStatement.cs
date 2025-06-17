namespace FluentCommand.Query;

/// <summary>
/// Represents a SQL query statement, including the SQL text and its associated parameters.
/// </summary>
/// <seealso cref="FluentCommand.Query.IQueryStatement" />
public class QueryStatement : IQueryStatement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryStatement"/> class with the specified SQL statement and parameters.
    /// </summary>
    /// <param name="statement">The SQL statement text to be executed.</param>
    /// <param name="parameters">The collection of <see cref="QueryParameter"/> objects used in the query.</param>
    /// <exception cref="System.ArgumentException">
    /// Thrown if <paramref name="statement"/> is null or whitespace.
    /// </exception>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown if <paramref name="parameters"/> is <c>null</c>.
    /// </exception>
    public QueryStatement(string statement, IReadOnlyCollection<QueryParameter> parameters)
    {
        if (string.IsNullOrWhiteSpace(statement))
            throw new ArgumentException($"'{nameof(statement)}' cannot be null or whitespace.", nameof(statement));

        if (parameters is null)
            throw new ArgumentNullException(nameof(parameters));

        Statement = statement;
        Parameters = parameters;
    }

    /// <summary>
    /// Gets the SQL statement text to be executed.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the SQL query statement.
    /// </value>
    public string Statement { get; }

    /// <summary>
    /// Gets the collection of <see cref="QueryParameter"/> objects used in the query.
    /// </summary>
    /// <value>
    /// An <see cref="IReadOnlyCollection{T}"/> of <see cref="QueryParameter"/> representing the parameters and their values for the SQL statement.
    /// </value>
    public IReadOnlyCollection<QueryParameter> Parameters { get; }
}
