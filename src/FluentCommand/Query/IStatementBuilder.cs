namespace FluentCommand.Query;

/// <summary>
/// interface defining sql statement builder
/// </summary>
public interface IStatementBuilder
{
    /// <summary>
    /// Builds the sql statement from this builder.
    /// </summary>
    /// <returns>The sql query statement</returns>
    QueryStatement BuildStatement();
}
