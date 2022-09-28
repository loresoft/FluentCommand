using System;

namespace FluentCommand;

public static class QueryBuilderExtensions
{
    public static IDataCommand Sql(this IDataSession dataSession, Action<QueryBuilder> builder)
    {
        var queryBuilder = new QueryBuilder(dataSession.QueryGenerator);
        builder(queryBuilder);

        var statement = queryBuilder.BuildStatement();

        var dataCommand = dataSession.Sql(statement?.Statement ?? string.Empty);

        foreach (var parameter in statement?.Parameters)
            dataCommand.Parameter(parameter.Key, parameter.Value);

        return dataCommand;
    }
}
