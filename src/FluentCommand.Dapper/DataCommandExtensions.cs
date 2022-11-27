using System.Data;
using System.Data.Common;

using Dapper;

using static Dapper.SqlMapper;

namespace FluentCommand;

/// <summary>
/// Extension methods for <see cref="IDataCommand"/>
/// </summary>
public static class DataCommandExtensions
{

    /// <summary>
    /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
    /// <returns>
    /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
    /// </returns>
    public static IEnumerable<TEntity> Query<TEntity>(this IDataQuery dataQuery)
        where TEntity : class, new()
    {
        var results = new List<TEntity>();

        dataQuery.Read(reader =>
        {
            var parser = reader.GetRowParser<TEntity>();

            while (reader.Read())
            {
                var entity = parser(reader);
                results.Add(entity);
            }
        }, CommandBehavior.SequentialAccess | CommandBehavior.SingleResult);

        return results;
    }

    /// <summary>
    /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
    /// <returns>
    /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
    /// </returns>
    public static TEntity QuerySingle<TEntity>(this IDataQuery dataQuery)
        where TEntity : class
    {
        TEntity result = default;

        dataQuery.Read(reader =>
        {
            var parser = reader.GetRowParser<TEntity>();
            if (reader.Read())
                result = parser(reader);

        }, CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);

        return result;
    }


    /// <summary>
    /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="dataQuery">The <see cref="IDataQueryAsync"/> for this extension method.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
    /// </returns>
    public static async Task<IEnumerable<TEntity>> QueryAsync<TEntity>(this IDataQueryAsync dataQuery, CancellationToken cancellationToken = default(CancellationToken))
        where TEntity : class
    {
        var results = new List<TEntity>();

        await dataQuery.ReadAsync(async (reader, token) =>
        {
            var parser = reader.GetRowParser<TEntity>();

            if (reader is DbDataReader dataReader)
            {
                while (await dataReader.ReadAsync(token))
                {
                    var entity = parser(reader);
                    results.Add(entity);
                }
            }
            else
            {
                while (reader.Read())
                {
                    var entity = parser(reader);
                    results.Add(entity);
                }
            }
        }, CommandBehavior.SequentialAccess | CommandBehavior.SingleResult, cancellationToken);

        return results;
    }

    /// <summary>
    /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="dataQuery">The <see cref="IDataQueryAsync"/> for this extension method.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
    /// </returns>
    public static async Task<TEntity> QuerySingleAsync<TEntity>(this IDataQueryAsync dataQuery, CancellationToken cancellationToken = default(CancellationToken))
        where TEntity : class
    {
        TEntity result = default;

        await dataQuery.ReadAsync(async (reader, token) =>
        {
            var parser = reader.GetRowParser<TEntity>();

            if (reader is DbDataReader dataReader)
            {
                if (await dataReader.ReadAsync(token))
                    result = parser(reader);
            }
            else
            {
                if (reader.Read())
                    result = parser(reader);
            }


        }, CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);

        return result;
    }
}
