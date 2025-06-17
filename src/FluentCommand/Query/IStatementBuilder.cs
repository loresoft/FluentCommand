namespace FluentCommand.Query;

/// <summary>
/// Defines a builder for constructing SQL query statements.
/// </summary>
public interface IStatementBuilder
{
    /// <summary>
    /// Builds and returns a <see cref="QueryStatement"/> representing the SQL query and its parameters.
    /// </summary>
    /// <returns>
    /// A <see cref="QueryStatement"/> containing the SQL statement and associated <see cref="QueryParameter"/> values.
    /// </returns>
    QueryStatement BuildStatement();
}
