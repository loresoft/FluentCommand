
namespace FluentCommand;

/// <summary>
/// Extension methods for <see cref="IDataCommand"/>
/// </summary>
public static class DataCommandExtensions
{
    /// <summary>
    /// Executes the command against the connection and converts the results to dynamic objects.
    /// </summary>
    /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
    /// <returns>
    /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of dynamic objects.
    /// </returns>
    public static IEnumerable<dynamic> Query(this IDataQuery dataQuery)
    {
        return dataQuery.Query(ReaderFactory.DynamicFactory);
    }

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
        return dataQuery.Query(ReaderFactory.EntityFactory<TEntity>);
    }


    /// <summary>
    /// Executes the query and returns the first row in the result as a dynamic object.
    /// </summary>
    /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
    /// <returns>
    /// A instance of a dynamic object if row exists; otherwise null.
    /// </returns>
    public static dynamic QuerySingle(this IDataQuery dataQuery)
    {
        return dataQuery.QuerySingle(ReaderFactory.DynamicFactory);
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
        where TEntity : class, new()
    {
        return dataQuery.QuerySingle(ReaderFactory.EntityFactory<TEntity>);
    }


    /// <summary>
    /// Executes the command against the connection and converts the results to dynamic objects asynchronously.
    /// </summary>
    /// <param name="dataQuery">The <see cref="IDataQueryAsync"/> for this extension method.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of dynamic objects.
    /// </returns>
    public static Task<IEnumerable<dynamic>> QueryAsync(this IDataQueryAsync dataQuery, CancellationToken cancellationToken = default(CancellationToken))
    {
        return dataQuery.QueryAsync(ReaderFactory.DynamicFactory, cancellationToken);
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
    public static Task<IEnumerable<TEntity>> QueryAsync<TEntity>(this IDataQueryAsync dataQuery, CancellationToken cancellationToken = default(CancellationToken))
        where TEntity : class, new()
    {
        return dataQuery.QueryAsync(ReaderFactory.EntityFactory<TEntity>, cancellationToken);
    }


    /// <summary>
    /// Executes the query and returns the first row in the result as a dynamic object asynchronously.
    /// </summary>
    /// <param name="dataQuery">The <see cref="IDataQueryAsync"/> for this extension method.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// A instance of a dynamic object if row exists; otherwise null.
    /// </returns>
    public static Task<dynamic> QuerySingleAsync(this IDataQueryAsync dataQuery, CancellationToken cancellationToken = default(CancellationToken))
    {
        return dataQuery.QuerySingleAsync(ReaderFactory.DynamicFactory, cancellationToken);
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
    public static Task<TEntity> QuerySingleAsync<TEntity>(this IDataQueryAsync dataQuery, CancellationToken cancellationToken = default(CancellationToken))
        where TEntity : class, new()
    {
        return dataQuery.QuerySingleAsync(ReaderFactory.EntityFactory<TEntity>, cancellationToken);
    }
}
