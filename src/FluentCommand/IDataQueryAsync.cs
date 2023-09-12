using System.Data;

namespace FluentCommand;

/// <summary>
/// An <see langword="interface"/> defining a data query operations asynchronously.
/// </summary>
public interface IDataQueryAsync
#if NETCOREAPP3_0_OR_GREATER
    : IAsyncDisposable
#endif

{
    /// <summary>
    /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
    /// <param name="commandBehavior">Provides a description of the results of the query and its effect on the database.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="factory"/> is null</exception>
    Task<IEnumerable<TEntity>> QueryAsync<TEntity>(
        Func<IDataReader, TEntity> factory,
        CommandBehavior commandBehavior = CommandBehavior.SingleResult,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
    /// <param name="commandBehavior">Provides a description of the results of the query and its effect on the database.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="factory"/> is null</exception>
    Task<TEntity> QuerySingleAsync<TEntity>(
        Func<IDataReader, TEntity> factory,
        CommandBehavior commandBehavior = CommandBehavior.SingleResult | CommandBehavior.SingleRow,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the query and returns the first column of the first row in the result set returned by the query asynchronously. All other columns and rows are ignored.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="convert">The <see langword="delegate" /> to convert the value..</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// The value of the first column of the first row in the result set.
    /// </returns>
    Task<TValue> QueryValueAsync<TValue>(
        Func<object, TValue> convert,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the command against the connection and converts the results to a <see cref="DataTable" /> asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// A <see cref="DataTable" /> of the results.
    /// </returns>
    Task<DataTable> QueryTableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the command against the connection and sends the resulting <see cref="IDataReader" /> to the readAction delegate.
    /// </summary>
    /// <param name="readAction">The read action delegate to pass the open <see cref="IDataReader" />.</param>
    /// <param name="commandBehavior">Provides a description of the results of the query and its effect on the database.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    Task ReadAsync(
        Func<IDataReader, CancellationToken, Task> readAction,
        CommandBehavior commandBehavior = CommandBehavior.Default,
        CancellationToken cancellationToken = default);
}
