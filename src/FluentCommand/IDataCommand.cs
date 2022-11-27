using System.Data;
using System.Data.Common;

namespace FluentCommand;

/// <summary>
/// An <see langword="interface"/> defining a data command.
/// </summary>
public interface IDataCommand : IDataQuery, IDataQueryAsync
{
    /// <summary>
    /// Gets the underlying <see cref="DbCommand"/> for this <see cref="DataCommand"/>.
    /// </summary>
    DbCommand Command { get; }


    /// <summary>
    /// Set the data command with the specified SQL.
    /// </summary>
    /// <param name="sql">The SQL statement.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a data command.
    /// </returns>
    IDataCommand Sql(string sql);

    /// <summary>
    /// Set the data command with the specified stored procedure name.
    /// </summary>
    /// <param name="storedProcedure">Name of the stored procedure.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a data command.
    /// </returns>
    IDataCommand StoredProcedure(string storedProcedure);


    /// <summary>
    /// Sets the wait time before terminating the attempt to execute a command and generating an error.
    /// </summary>
    /// <param name="timeout">TThe time in seconds to wait for the command to execute.</param>
    /// <returns>A fluent <see langword="interface"/> to the data command.</returns>
    IDataCommand CommandTimeout(int timeout);


    /// <summary>
    /// Adds the parameter to the underlying command.
    /// </summary>
    /// <returns>A fluent <see langword="interface"/> to the data command.</returns>
    IDataCommand Parameter(DbParameter parameter);

    /// <summary>
    /// Register a return value <paramref name="callback" /> for the specified <paramref name="parameter"/>.
    /// </summary>
    /// <typeparam name="TParameter">The type of the parameter value.</typeparam>
    /// <param name="parameter">The <see cref="IDbDataParameter"/> to add.</param>
    /// <param name="callback">The callback used to get the out value.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to the data command.
    /// </returns>
    IDataCommand RegisterCallback<TParameter>(DbParameter parameter, Action<TParameter> callback);


    /// <summary>
    /// Uses cache to insert and retrieve cached results for the command with the specified <paramref name="slidingExpiration" />.
    /// </summary>
    /// <param name="slidingExpiration">
    /// A value that indicates whether a cache entry should be evicted if it has not been accessed in a given span of time.
    /// </param>
    /// <returns>
    /// A fluent <see langword="interface" /> to the data command.
    /// </returns>
    /// <exception cref="InvalidOperationException">A command with Output or Return parameters can not be cached.</exception>
    IDataCommand UseCache(TimeSpan slidingExpiration);

    /// <summary>
    /// Uses cache to insert and retrieve cached results for the command with the specified <paramref name="absoluteExpiration" />.
    /// </summary>
    /// <param name="absoluteExpiration">A value that indicates whether a cache entry should be evicted after a specified duration.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to the data command.
    /// </returns>
    /// <exception cref="InvalidOperationException">A command with Output or Return parameters can not be cached.</exception>
    IDataCommand UseCache(DateTimeOffset absoluteExpiration);


    /// <summary>
    /// Expires cached items that have been cached using the current DataCommand.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <returns>
    /// A fluent <see langword="interface" /> to the data command.
    /// </returns>
    /// <remarks>
    /// Cached keys are created using the current DataCommand state.  When any Query opertion is
    /// executed with a cache policy, the results are cached.  Use this method with the same parameters
    /// to expire the cached item.
    /// </remarks>
    IDataCommand ExpireCache<TEntity>();

    /// <summary>
    /// Use to pass a state to the <see cref="IDataQueryLogger"/>.
    /// </summary>
    /// <param name="state">The state to pass to the logger.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to the data command.
    /// </returns>
    /// <remarks>
    /// Use the state to help control what is logged.
    /// </remarks>
    IDataCommand LogState(object state);

    /// <summary>
    /// Executes the command against the connection and sends the resulting <see cref="IDataQuery"/> for reading multiple results sets.
    /// </summary>
    /// <param name="queryAction">The query action delegate to pass the open <see cref="IDataQuery"/> for reading multiple results.</param>
    void QueryMultiple(Action<IDataQuery> queryAction);

    /// <summary>
    /// Executes the command against the connection and sends the resulting <see cref="IDataQueryAsync" /> for reading multiple results sets.
    /// </summary>
    /// <param name="queryAction">The query action delegate to pass the open <see cref="IDataQueryAsync" /> for reading multiple results.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    Task QueryMultipleAsync(Action<IDataQueryAsync> queryAction, CancellationToken cancellationToken = default);


    /// <summary>
    /// Executes the command against a connection.
    /// </summary>
    /// <returns>The number of rows affected.</returns>
    int Execute();

    /// <summary>
    /// Executes the command against a connection asynchronously.
    /// </summary>
    /// <returns>
    /// The number of rows affected.
    /// </returns>
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);



    /// <summary>
    /// Converts the specified <paramref name="value"/> before assigning to <seealso cref="DbParameter.Value"/>
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value</returns>
    object ConvertParameterValue<TValue>(TValue value);
}
