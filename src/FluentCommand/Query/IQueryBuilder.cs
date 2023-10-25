using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Interface for query building
/// </summary>
public interface IQueryBuilder
{
    /// <summary>
    /// Gets the query generator.
    /// </summary>
    /// <value>
    /// The query generator.
    /// </value>
    IQueryGenerator QueryGenerator { get; }

    /// <summary>
    /// Gets the query parameters.
    /// </summary>
    /// <value>
    /// The query parameters.
    /// </value>
    List<QueryParameter> Parameters { get; }
}
