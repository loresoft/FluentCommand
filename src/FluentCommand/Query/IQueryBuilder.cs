using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Defines a builder interface for constructing SQL queries, including access to the query generator and parameters.
/// </summary>
public interface IQueryBuilder
{
    /// <summary>
    /// Gets the <see cref="IQueryGenerator"/> used to generate SQL statements for the query.
    /// </summary>
    /// <value>
    /// An instance of <see cref="IQueryGenerator"/> responsible for building SQL expressions and statements.
    /// </value>
    IQueryGenerator QueryGenerator { get; }

    /// <summary>
    /// Gets the collection of <see cref="QueryParameter"/> objects used in the query.
    /// </summary>
    /// <value>
    /// A list of <see cref="QueryParameter"/> representing the parameters and their values for the SQL query.
    /// </value>
    List<QueryParameter> Parameters { get; }
}
