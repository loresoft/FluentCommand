// Ignore Spelling: Sql

using FluentCommand.Query;
using FluentCommand.Query.Generators;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace FluentCommand;

/// <summary>
/// Provides extension methods for building SQL queries using a <see cref="QueryBuilder"/> within a data session.
/// </summary>
public static class QueryBuilderExtensions
{
    /// <summary>
    /// Configures and sets the SQL command for the specified <see cref="IDataSession"/> using a fluent <see cref="QueryBuilder"/>.
    /// </summary>
    /// <param name="dataSession">The data session used to execute the query.</param>
    /// <param name="builder">
    /// An action that configures the <see cref="QueryBuilder"/> to build the desired SQL statement and parameters.
    /// </param>
    /// <returns>
    /// An <see cref="IDataCommand"/> representing the configured SQL command, ready for execution or further configuration.
    /// </returns>
    /// <remarks>
    /// This method creates a new <see cref="QueryBuilder"/> using the session's <see cref="IQueryGenerator"/> and a fresh parameter list.
    /// The <paramref name="builder"/> action is invoked to configure the query. The resulting SQL and parameters are then applied to the session's data command.
    /// </remarks>
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
