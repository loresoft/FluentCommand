namespace FluentCommand.Query;

/// <summary>
/// Defines an interface for a SQL query statement, including the SQL text and its parameters.
/// </summary>
public interface IQueryStatement
{
    /// <summary>
    /// Gets the collection of <see cref="QueryParameter"/> objects used in the query.
    /// </summary>
    /// <value>
    /// An <see cref="IReadOnlyCollection{T}"/> of <see cref="QueryParameter"/> representing the parameters and their values for the SQL statement.
    /// </value>
    IReadOnlyCollection<QueryParameter> Parameters { get; }

    /// <summary>
    /// Gets the SQL statement text to be executed.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing the SQL query statement.
    /// </value>
    string Statement { get; }
}
