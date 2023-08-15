using System.Data;
using System.Dynamic;

using FluentCommand.Extensions;
using FluentCommand.Reflection;

namespace FluentCommand;

/// <summary>
/// Extension methods for <see cref="IDataQuery"/>
/// </summary>
public static class DataQueryExtensions
{
    /// <summary>
    /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
    /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="factory"/> is null</exception>
    public static Task<IEnumerable<TEntity>> QueryAsync<TEntity>(
        this IDataQueryAsync dataQuery,
        Func<IDataReader, TEntity> factory,
        CancellationToken cancellationToken = default)
    {
        return dataQuery.QueryAsync(factory, CommandBehavior.SingleResult, cancellationToken);
    }

    /// <summary>
    /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
    /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="factory"/> is null</exception>
    public static Task<TEntity> QuerySingleAsync<TEntity>(
        this IDataQueryAsync dataQuery,
        Func<IDataReader, TEntity> factory,
        CancellationToken cancellationToken = default)
    {
        return dataQuery.QuerySingleAsync(factory, CommandBehavior.SingleResult | CommandBehavior.SingleRow, cancellationToken);

    }


    /// <summary>
    /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
    /// <returns>
    /// The value of the first column of the first row in the result set.
    /// </returns>
    public static TValue QueryValue<TValue>(this IDataQuery dataQuery)
    {
        return dataQuery.QueryValue<TValue>(null);
    }

    /// <summary>
    /// Executes the query and returns the first column of the first row in the result set returned by the query asynchronously. All other columns and rows are ignored.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// The value of the first column of the first row in the result set.
    /// </returns>
    public static Task<TValue> QueryValueAsync<TValue>(
        this IDataQueryAsync dataQuery,
        CancellationToken cancellationToken = default)
    {
        return dataQuery.QueryValueAsync<TValue>(null, cancellationToken);
    }

    /// <summary>
    /// Executes the query and returns the first column values in the result set returned by the query. All other columns and rows are ignored.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
    /// <returns>
    /// The value of the first column values in the result set.
    /// </returns>
    public static IEnumerable<TValue> QueryValues<TValue>(this IDataQuery dataQuery)
    {
        return dataQuery.Query(r => r.GetValue<TValue>(0));
    }

    /// <summary>
    /// Executes the query and returns the first column values in the result set returned by the query. All other columns and rows are ignored.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dataQuery">The <see cref="IDataQuery"/> for this extension method.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// The value of the first column values in the result set.
    /// </returns>
    public static async Task<IEnumerable<TValue>> QueryValuesAsync<TValue>(
        this IDataQueryAsync dataQuery,
        CancellationToken cancellationToken = default)
    {
        return await dataQuery.QueryAsync(r => r.GetValue<TValue>(0), cancellationToken);
    }


    // reflection extensions

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
        return dataQuery.Query(
            factory: DataReaderExtensions.EntityFactory<TEntity>,
            commandBehavior: CommandBehavior.SequentialAccess | CommandBehavior.SingleResult);
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
        return dataQuery.QuerySingle(
            factory: DataReaderExtensions.EntityFactory<TEntity>,
            commandBehavior: CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
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
        return dataQuery.QueryAsync(
            factory: DataReaderExtensions.EntityFactory<TEntity>,
            commandBehavior: CommandBehavior.SequentialAccess | CommandBehavior.SingleResult,
            cancellationToken: cancellationToken);
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
        return dataQuery.QuerySingleAsync(
            factory: DataReaderExtensions.EntityFactory<TEntity>,
            commandBehavior: CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow,
            cancellationToken: cancellationToken);
    }

}
