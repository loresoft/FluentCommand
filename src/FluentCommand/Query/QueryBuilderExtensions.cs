namespace FluentCommand.Query;

/// <summary>
/// Extension methods for building a query for the data session
/// </summary>
public static class QueryBuilderExtensions
{
    /// <summary>
    /// Set the data command statement using the builder action.
    /// </summary>
    /// <param name="dataSession">The data session.</param>
    /// <param name="builder">The query builder action.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a data command.
    /// </returns>
    public static IDataCommand Sql(this IDataSession dataSession, Action<QueryBuilder> builder)
    {
        var queryParameters = new List<QueryParameter>();
        var queryBuilder = new QueryBuilder(dataSession.QueryGenerator, queryParameters);

        builder(queryBuilder);

        var statement = queryBuilder.BuildStatement();

        var dataCommand = dataSession.Sql(statement?.Statement ?? string.Empty);

        if (statement?.Parameters == null)
            return dataCommand;

        foreach (var parameter in statement.Parameters)
            dataCommand.Parameter(parameter.Name, parameter.Value, parameter.Type);

        return dataCommand;
    }
}
