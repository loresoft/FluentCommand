namespace FluentCommand.Query;

public static class QueryBuilderExtensions
{
    public static IDataCommand Sql(this IDataSession dataSession, Action<QueryBuilder> builder)
    {
        var queryParameters = new List<QueryParameter>();
        var queryBuilder = new QueryBuilder(dataSession.QueryGenerator, queryParameters);

        builder(queryBuilder);

        var statement = queryBuilder.BuildStatement();

        var dataCommand = dataSession.Sql(statement?.Statement ?? string.Empty);

        foreach (var parameter in statement?.Parameters)
            dataCommand.Parameter(parameter.Name, parameter.Value, parameter.Type);

        return dataCommand;
    }
}
