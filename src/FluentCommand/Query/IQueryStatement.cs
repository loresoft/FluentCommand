namespace FluentCommand.Query;

/// <summary>
/// interface for defining a sql query
/// </summary>
public interface IQueryStatement
{
    /// <summary>
    /// Gets the parameters for the query.
    /// </summary>
    /// <value>
    /// The parameters for the query.
    /// </value>
    IReadOnlyCollection<QueryParameter> Parameters { get; }

    /// <summary>
    /// Gets the sql statement.
    /// </summary>
    /// <value>
    /// The sql statement.
    /// </value>
    string Statement { get; }
}
