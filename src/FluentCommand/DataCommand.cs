using System.Data;
using System.Data.Common;
using System.Diagnostics;

using FluentCommand.Extensions;

using HashCode = FluentCommand.Internal.HashCode;

namespace FluentCommand;

/// <summary>
/// A fluent class to build a data command.
/// </summary>
public class DataCommand : DisposableBase, IDataCommand
{
    private readonly Queue<DataCallback> _callbacks;
    private readonly IDataSession _dataSession;

    private TimeSpan? _slidingExpiration;
    private DateTimeOffset? _absoluteExpiration;
    private object _logState;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommand" /> class.
    /// </summary>
    /// <param name="dataSession">The data session.</param>
    /// <param name="transaction">The DbTransaction for this DataCommand.</param>
    public DataCommand(IDataSession dataSession, DbTransaction transaction)
    {
        _callbacks = new Queue<DataCallback>();
        _dataSession = dataSession ?? throw new ArgumentNullException(nameof(dataSession));
        Command = dataSession.Connection.CreateCommand();
        Command.Transaction = transaction;
    }

    /// <summary>
    /// Gets the underlying <see cref="DbCommand"/> for this <see cref="DataCommand"/>.
    /// </summary>
    public DbCommand Command { get; }


    /// <summary>
    /// Set the data command with the specified SQL statement.
    /// </summary>
    /// <param name="sql">The SQL statement.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a data command.
    /// </returns>
    public IDataCommand Sql(string sql)
    {
        Command.CommandText = sql;
        Command.CommandType = CommandType.Text;
        return this;
    }

    /// <summary>
    /// Set the data command with the specified stored procedure name.
    /// </summary>
    /// <param name="storedProcedure">Name of the stored procedure.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a data command.
    /// </returns>
    public IDataCommand StoredProcedure(string storedProcedure)
    {
        Command.CommandText = storedProcedure;
        Command.CommandType = CommandType.StoredProcedure;
        return this;
    }


    /// <summary>
    /// Sets the wait time before terminating the attempt to execute a command and generating an error.
    /// </summary>
    /// <param name="timeout">The time, in seconds, to wait for the command to execute.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to the data command.
    /// </returns>
    public IDataCommand CommandTimeout(int timeout)
    {
        Command.CommandTimeout = timeout;
        return this;
    }


    /// <summary>
    /// Adds the parameter to the underlying command.
    /// </summary>
    /// <param name="parameter">The <see cref="DbParameter" /> to add.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to the data command.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="parameter"/> is null</exception>
    public IDataCommand Parameter(DbParameter parameter)
    {
        if (parameter == null)
            throw new ArgumentNullException(nameof(parameter));

        Command.Parameters.Add(parameter);
        return this;
    }

    /// <summary>
    /// Register a return value <paramref name="callback" /> for the specified <paramref name="parameter" />.
    /// </summary>
    /// <typeparam name="TParameter">The type of the parameter value.</typeparam>
    /// <param name="parameter">The <see cref="IDbDataParameter" /> to add.</param>
    /// <param name="callback">The callback used to get the out value.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to the data command.
    /// </returns>
    public IDataCommand RegisterCallback<TParameter>(DbParameter parameter, Action<TParameter> callback)
    {
        var dataCallback = new DataCallback(typeof(TParameter), parameter, callback);
        _callbacks.Enqueue(dataCallback);

        return this;
    }


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
    public IDataCommand UseCache(TimeSpan slidingExpiration)
    {
        _slidingExpiration = slidingExpiration;
        if (_slidingExpiration != null && _callbacks.Count > 0)
            throw new InvalidOperationException("A command with Output or Return parameters can not be cached.");

        return this;
    }

    /// <summary>
    /// Uses cache to insert and retrieve cached results for the command with the specified <paramref name="absoluteExpiration" />.
    /// </summary>
    /// <param name="absoluteExpiration">A value that indicates whether a cache entry should be evicted after a specified duration.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to the data command.
    /// </returns>
    /// <exception cref="InvalidOperationException">A command with Output or Return parameters can not be cached.</exception>
    public IDataCommand UseCache(DateTimeOffset absoluteExpiration)
    {
        _absoluteExpiration = absoluteExpiration;
        if (_absoluteExpiration != null && _callbacks.Count > 0)
            throw new InvalidOperationException("A command with Output or Return parameters can not be cached.");

        return this;
    }


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
    public IDataCommand ExpireCache<TEntity>()
    {
        string cacheKey = CacheKey<TEntity>(true);
        if (_dataSession.Cache != null && cacheKey != null)
            _dataSession.Cache.Remove(cacheKey);

        return this;
    }

    /// <summary>
    /// Use to pass a state to the <see cref="IDataQueryLogger" />.
    /// </summary>
    /// <param name="state">The state to pass to the logger.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to the data command.
    /// </returns>
    /// <remarks>
    /// Use the state to help control what is logged.
    /// </remarks>
    public IDataCommand LogState(object state)
    {
        _logState = state;
        return this;
    }

    /// <summary>
    /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
    /// <returns>
    /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
    /// </returns>
    public IEnumerable<TEntity> Query<TEntity>(Func<IDataReader, TEntity> factory)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        return QueryFactory(() =>
        {
            var results = new List<TEntity>();

            using var reader = Command.ExecuteReader(CommandBehavior.SingleResult);
            while (reader.Read())
            {
                var entity = factory(reader);
                results.Add(entity);
            }

            return results;
        }, true);
    }

    /// <summary>
    /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="factory"/> is null</exception>
    public async Task<IEnumerable<TEntity>> QueryAsync<TEntity>(Func<IDataReader, TEntity> factory, CancellationToken cancellationToken = default)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        return await QueryFactoryAsync(async (token) =>
        {
            var results = new List<TEntity>();

            using var reader = await Command.ExecuteReaderAsync(CommandBehavior.SingleResult, token);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var entity = factory(reader);
                results.Add(entity);
            }

            return results;

        }, true, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
    /// <returns>
    /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="factory"/> is null</exception>
    public TEntity QuerySingle<TEntity>(Func<IDataReader, TEntity> factory)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        return QueryFactory(() =>
        {
            using var reader = Command.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SingleRow);
            var result = reader.Read()
                ? factory(reader)
                : default;

            return result;
        }, true);
    }

    /// <summary>
    /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object asynchronously.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="factory"/> is null</exception>
    public async Task<TEntity> QuerySingleAsync<TEntity>(Func<IDataReader, TEntity> factory, CancellationToken cancellationToken = default)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        return await QueryFactoryAsync(async (token) =>
        {
            using var reader = await Command.ExecuteReaderAsync(CommandBehavior.SingleResult | CommandBehavior.SingleRow, token).ConfigureAwait(false);
            var result = await reader.ReadAsync(token).ConfigureAwait(false)
               ? factory(reader)
               : default;

            return result;
        }, true, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="convert">The <see langword="delegate" /> to convert the value..</param>
    /// <returns>
    /// The value of the first column of the first row in the result set.
    /// </returns>
    public TValue QueryValue<TValue>(Func<object, TValue> convert)
    {
        return QueryFactory(() =>
        {
            var result = Command.ExecuteScalar();
            var value = result.ConvertValue(convert);

            return value;
        }, true);
    }

    /// <summary>
    /// Executes the query and returns the first column of the first row in the result set returned by the query asynchronously. All other columns and rows are ignored.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="convert">The <see langword="delegate" /> to convert the value..</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// The value of the first column of the first row in the result set.
    /// </returns>
    public async Task<TValue> QueryValueAsync<TValue>(Func<object, TValue> convert, CancellationToken cancellationToken = default)
    {
        return await QueryFactoryAsync(async (token) =>
        {
            var result = await Command.ExecuteScalarAsync(token).ConfigureAwait(false);
            var value = result.ConvertValue(convert);

            return value;
        }, true, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Executes the command against the connection and converts the results to a <see cref="DataTable" />.
    /// </summary>
    /// <returns>
    /// A <see cref="DataTable" /> of the results.
    /// </returns>
    public DataTable QueryTable()
    {
        return QueryFactory(() =>
        {
            var dataTable = new DataTable();

            using var reader = Command.ExecuteReader();
            dataTable.Load(reader);

            return dataTable;
        }, true);
    }

    /// <summary>
    /// Executes the command against the connection and converts the results to a <see cref="DataTable" /> asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// A <see cref="DataTable" /> of the results.
    /// </returns>
    public async Task<DataTable> QueryTableAsync(CancellationToken cancellationToken = default)
    {
        return await QueryFactoryAsync(async (token) =>
        {
            var dataTable = new DataTable();

            using var reader = await Command.ExecuteReaderAsync(token).ConfigureAwait(false);
            dataTable.Load(reader);

            return dataTable;

        }, true, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Executes the command against the connection and sends the resulting <see cref="IDataQuery" /> for reading multiple results sets.
    /// </summary>
    /// <param name="queryAction">The query action delegate to pass the open <see cref="IDataQuery" /> for reading multiple results.</param>
    public void QueryMultiple(Action<IDataQuery> queryAction)
    {
        if (queryAction == null)
            throw new ArgumentNullException(nameof(queryAction));

        QueryFactory(() =>
        {
            using var reader = Command.ExecuteReader();
            var query = new QueryMultipleResult(reader);
            queryAction(query);

            return true;
        }, false);

    }

    /// <summary>
    /// Executes the command against the connection and sends the resulting <see cref="IDataQueryAsync" /> for reading multiple results sets.
    /// </summary>
    /// <param name="queryAction">The query action delegate to pass the open <see cref="IDataQueryAsync" /> for reading multiple results.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    public async Task QueryMultipleAsync(Action<IDataQueryAsync> queryAction, CancellationToken cancellationToken = default)
    {
        if (queryAction == null)
            throw new ArgumentNullException(nameof(queryAction));

        await QueryFactoryAsync(async (token) =>
        {
            using var reader = await Command.ExecuteReaderAsync(token).ConfigureAwait(false);
            var query = new QueryMultipleResult(reader);
            queryAction(query);

            return true;
        }, false, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Executes the command against a connection.
    /// </summary>
    /// <returns>
    /// The number of rows affected.
    /// </returns>
    public int Execute()
    {
        return QueryFactory(() =>
        {
            int result = Command.ExecuteNonQuery();
            return result;
        }, false);
    }

    /// <summary>
    /// Executes the command against a connection asynchronously.
    /// </summary>
    /// <returns>
    /// The number of rows affected.
    /// </returns>
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return await QueryFactoryAsync(async (token) =>
        {
            int result = await Command.ExecuteNonQueryAsync(token).ConfigureAwait(false);

            return result;
        }, false, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Executes the command against the connection and sends the resulting <see cref="IDataReader" /> to the readAction delegate.
    /// </summary>
    /// <param name="readAction">The read action delegate to pass the open <see cref="IDataReader" />.</param>
    /// <param name="commandBehavior">Provides a description of the results of the query and its effect on the database.</param>
    public void Read(Action<IDataReader> readAction, CommandBehavior commandBehavior = CommandBehavior.Default)
    {
        QueryFactory(() =>
        {
            using var reader = Command.ExecuteReader(commandBehavior);
            readAction(reader);

            return true;
        }, false);
    }

    /// <summary>
    /// Executes the command against the connection and sends the resulting <see cref="IDataReader" /> to the readAction delegate.
    /// </summary>
    /// <param name="readAction">The read action delegate to pass the open <see cref="IDataReader" />.</param>
    /// <param name="commandBehavior">Provides a description of the results of the query and its effect on the database.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    public async Task ReadAsync(Func<IDataReader, CancellationToken, Task> readAction, CommandBehavior commandBehavior = CommandBehavior.Default, CancellationToken cancellationToken = default)
    {
        await QueryFactoryAsync(async (token) =>
        {
            using var reader = await Command.ExecuteReaderAsync(commandBehavior, token).ConfigureAwait(false);
            await readAction(reader, cancellationToken);

            return true;
        }, false, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Disposes the managed resources.
    /// </summary>
    protected override void DisposeManagedResources()
    {
        Command?.Dispose();
    }

#if !NETSTANDARD2_0
    /// <summary>
    /// Disposes the managed resources.
    /// </summary>
    protected override async ValueTask DisposeResourcesAsync()
    {
        if (Command != null)
            await Command.DisposeAsync();
    }
#endif

    internal void TriggerCallbacks()
    {
        if (_callbacks.Count == 0)
            return;

        while (_callbacks.Count > 0)
        {
            var dataCallback = _callbacks.Dequeue();
            dataCallback.Invoke();
        }
    }


    private TResult QueryFactory<TResult>(Func<TResult> query, bool supportCache)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        AssertDisposed();

        var watch = Stopwatch.StartNew();
        try
        {
            var cacheKey = CacheKey<TResult>(supportCache);
            if (GetCache(cacheKey) is TResult results)
                return results;

            _dataSession.EnsureConnection();

            results = query();

            TriggerCallbacks();

            SetCache(cacheKey, results);

            return results;
        }
        catch (Exception ex)
        {
            watch.Stop();
            LogCommand(watch.Elapsed, ex);

            throw;
        }
        finally
        {
            // if catch block didn't already log
            if (watch.IsRunning)
            {
                watch.Stop();
                LogCommand(watch.Elapsed);
            }

            _dataSession.ReleaseConnection();
            Dispose();
        }
    }

    private async Task<TResult> QueryFactoryAsync<TResult>(Func<CancellationToken, Task<TResult>> query, bool supportCache, CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        AssertDisposed();

        var watch = Stopwatch.StartNew();
        try
        {
            var cacheKey = CacheKey<TResult>(supportCache);
            if (GetCache(cacheKey) is TResult results)
                return results;

            await _dataSession.EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);

            results = await query(cancellationToken).ConfigureAwait(false);

            TriggerCallbacks();

            SetCache(cacheKey, results);

            return results;
        }
        catch (Exception ex)
        {
            watch.Stop();
            LogCommand(watch.Elapsed, ex);

            throw;
        }
        finally
        {
            // if catch block didn't already log
            if (watch.IsRunning)
            {
                watch.Stop();
                LogCommand(watch.Elapsed);
            }

            _dataSession.ReleaseConnection();
            Dispose();
        }
    }


    private string CacheKey<T>(bool supportCache)
    {
        if (!supportCache)
            return null;

        if (_dataSession.Cache == null)
            return null;

        if (_slidingExpiration == null && _absoluteExpiration == null)
            return null;


        var connectionString = Command.Connection?.ConnectionString;
        var commandText = Command.CommandText;
        var commandType = Command.CommandType;
        var type = typeof(T);

        var hashCode = HashCode.Seed
            .Combine(connectionString)
            .Combine(commandType)
            .Combine(commandText)
            .Combine(type);

        foreach (IDbDataParameter parameter in Command.Parameters)
        {
            if (parameter.Direction is ParameterDirection.InputOutput or ParameterDirection.Output or ParameterDirection.ReturnValue)
                throw new InvalidOperationException("A command with Output or Return parameters can not be cached.");

            hashCode = hashCode
                .Combine(parameter.ParameterName)
                .Combine(parameter.Value)
                .Combine(parameter.DbType);
        }

        return $"global:data:{hashCode}";
    }

    private object GetCache(string key)
    {
        if (_slidingExpiration == null && _absoluteExpiration == null)
            return null;

        if (key == null)
            return null;

        var cache = _dataSession.Cache;
        if (cache == null)
            return null;

        return cache.Get(key);
    }

    private void SetCache(string key, object value)
    {
        if (_slidingExpiration == null && _absoluteExpiration == null)
            return;

        if (key == null || value == null)
            return;

        var cache = _dataSession.Cache;
        if (cache == null)
            return;

        if (_absoluteExpiration.HasValue)
            cache.Set(key, value, _absoluteExpiration.Value);
        else if (_slidingExpiration.HasValue)
            cache.Set(key, value, _slidingExpiration.Value);
    }


    private void LogCommand(TimeSpan duration, Exception exception = null)
    {
        _dataSession.QueryLogger?.LogCommand(Command, duration, exception, _logState);
    }
}
