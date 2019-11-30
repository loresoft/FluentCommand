using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentCommand.Extensions;

namespace FluentCommand
{
    /// <summary>
    /// A fluent class to build a data command.
    /// </summary>
    public class DataCommand : DisposableBase, IDataCommand
    {
        private readonly Queue<DataCallback> _callbacks;
        private readonly IDataSession _dataSession;
        private readonly DbCommand _command;

        private TimeSpan? _slidingExpiration;
        private DateTimeOffset? _absoluteExpiration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCommand" /> class.
        /// </summary>
        /// <param name="dataSession">The data session.</param>
        /// <param name="transaction">The DbTransaction for this DataCommand.</param>
        public DataCommand(IDataSession dataSession, DbTransaction transaction)
        {
            _callbacks = new Queue<DataCallback>();
            _dataSession = dataSession ?? throw new ArgumentNullException(nameof(dataSession));
            _command = dataSession.Connection.CreateCommand();
            _command.Transaction = transaction;
        }

        /// <summary>
        /// Gets the underlying <see cref="DbCommand"/> for this <see cref="DataCommand"/>.
        /// </summary>
        public DbCommand Command => _command;

        /// <summary>
        /// Set the data command with the specified SQL statement.
        /// </summary>
        /// <param name="sql">The SQL statement.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a data command.
        /// </returns>
        public IDataCommand Sql(string sql)
        {
            _command.CommandText = sql;
            _command.CommandType = CommandType.Text;
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
            _command.CommandText = storedProcedure;
            _command.CommandType = CommandType.StoredProcedure;
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
            _command.CommandTimeout = timeout;
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

            _command.Parameters.Add(parameter);
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
            var dataCallback = new DataCallback
            {
                Callback = callback,
                Type = typeof(TParameter),
                Parameter = parameter
            };
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
            string cacheKey = CacheKey<TEntity>();
            if (_dataSession.Cache != null && cacheKey != null)
                _dataSession.Cache.Remove(cacheKey);

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
            where TEntity : class
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            AssertDisposed();

            try
            {
                string cacheKey = CacheKey<TEntity>();
                if (GetCache(cacheKey) is List<TEntity> results)
                    return results;

                results = new List<TEntity>();
                _dataSession.EnsureConnection();

                LogCommand();
                using (var reader = _command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var entity = factory(reader);
                        results.Add(entity);
                    }
                }

                TriggerCallbacks();

                SetCache(cacheKey, results);
                return results;
            }
            finally
            {
                _dataSession.ReleaseConnection();
                Dispose();
            }
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
            where TEntity : class
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            AssertDisposed();

            try
            {
                string cacheKey = CacheKey<TEntity>();
                if (GetCache(cacheKey) is List<TEntity> results)
                    return results;

                results = new List<TEntity>();
                await _dataSession.EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);

                LogCommand();
                using (var reader = await _command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        var entity = factory(reader);
                        results.Add(entity);
                    }
                }

                TriggerCallbacks();

                SetCache(cacheKey, results);
                return results;
            }
            finally
            {
                _dataSession.ReleaseConnection();
                Dispose();
            }
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
            where TEntity : class
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            AssertDisposed();

            try
            {
                string cacheKey = CacheKey<TEntity>();
                if (GetCache(cacheKey) is TEntity result)
                    return result;

                _dataSession.EnsureConnection();

                LogCommand();

                using (var reader = _command.ExecuteReader())
                {
                    result = reader.Read()
                        ? factory(reader)
                        : default;
                }

                TriggerCallbacks();

                SetCache(cacheKey, result);
                return result;
            }
            finally
            {
                _dataSession.ReleaseConnection();
                Dispose();
            }
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
            where TEntity : class
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            AssertDisposed();

            try
            {
                string cacheKey = CacheKey<TEntity>();
                if (GetCache(cacheKey) is TEntity result)
                    return result;

                await _dataSession.EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);

                LogCommand();

                using (var reader = await _command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false)
                        ? factory(reader)
                        : default;
                }

                TriggerCallbacks();

                SetCache(cacheKey, result);
                return result;
            }
            finally
            {
                _dataSession.ReleaseConnection();
                Dispose();
            }
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
            AssertDisposed();

            try
            {
                string cacheKey = CacheKey<TValue>();
                var result = GetCache(cacheKey);
                if (result != null)
                    return result.ConvertValue(convert);

                _dataSession.EnsureConnection();

                LogCommand();

                result = _command.ExecuteScalar();
                var value = result.ConvertValue(convert);

                TriggerCallbacks();

                SetCache(cacheKey, value);
                return value;
            }
            finally
            {
                _dataSession.ReleaseConnection();
                Dispose();
            }
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
            AssertDisposed();

            try
            {
                string cacheKey = CacheKey<TValue>();
                var result = GetCache(cacheKey);
                if (result != null)
                    return result.ConvertValue(convert);

                await _dataSession.EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);

                LogCommand();

                result = await _command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                var value = result.ConvertValue(convert);

                TriggerCallbacks();

                SetCache(cacheKey, value);
                return value;
            }
            finally
            {
                _dataSession.ReleaseConnection();
                Dispose();
            }
        }


        /// <summary>
        /// Executes the command against the connection and converts the results to a <see cref="DataTable" />.
        /// </summary>
        /// <returns>
        /// A <see cref="DataTable" /> of the results.
        /// </returns>
        public DataTable QueryTable()
        {
            AssertDisposed();

            try
            {
                string cacheKey = CacheKey<DataTable>();
                if (GetCache(cacheKey) is DataTable dataTable)
                    return dataTable;

                _dataSession.EnsureConnection();

                LogCommand();

                dataTable = new DataTable();

                using (var reader = _command.ExecuteReader())
                    dataTable.Load(reader);

                TriggerCallbacks();

                SetCache(cacheKey, dataTable);
                return dataTable;
            }
            finally
            {
                _dataSession.ReleaseConnection();
                Dispose();
            }

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
            AssertDisposed();

            try
            {
                string cacheKey = CacheKey<DataTable>();
                if (GetCache(cacheKey) is DataTable dataTable)
                    return dataTable;

                await _dataSession.EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);

                LogCommand();

                dataTable = new DataTable();

                using (var reader = await _command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                    dataTable.Load(reader);

                TriggerCallbacks();

                SetCache(cacheKey, dataTable);
                return dataTable;
            }
            finally
            {
                _dataSession.ReleaseConnection();
                Dispose();
            }

        }


        /// <summary>
        /// Executes the command against the connection and sends the resulting <see cref="IDataQuery" /> for reading multiple results sets.
        /// </summary>
        /// <param name="queryAction">The query action delegate to pass the open <see cref="IDataQuery" /> for reading multiple results.</param>
        public void QueryMultiple(Action<IDataQuery> queryAction)
        {
            if (queryAction == null)
                throw new ArgumentNullException(nameof(queryAction));

            AssertDisposed();

            try
            {
                _dataSession.EnsureConnection();

                LogCommand();


                using (var reader = _command.ExecuteReader())
                {
                    var query = new QueryMultipleResult(reader);
                    queryAction(query);
                }

                TriggerCallbacks();
            }
            finally
            {
                _dataSession.ReleaseConnection();
                Dispose();
            }
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

            AssertDisposed();

            try
            {
                await _dataSession.EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);

                LogCommand();


                using (var reader = await _command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {
                    var query = new QueryMultipleResult(reader);
                    queryAction(query);
                }

                TriggerCallbacks();
            }
            finally
            {
                _dataSession.ReleaseConnection();
                Dispose();
            }
        }


        /// <summary>
        /// Executes the command against a connection.
        /// </summary>
        /// <returns>
        /// The number of rows affected.
        /// </returns>
        public int Execute()
        {
            AssertDisposed();

            try
            {
                _dataSession.EnsureConnection();

                LogCommand();

                int result = _command.ExecuteNonQuery();

                TriggerCallbacks();
                return result;
            }
            finally
            {
                _dataSession.ReleaseConnection();
                Dispose();
            }
        }

        /// <summary>
        /// Executes the command against a connection asynchronously.
        /// </summary>
        /// <returns>
        /// The number of rows affected.
        /// </returns>
        public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            AssertDisposed();

            try
            {
                await _dataSession.EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);

                LogCommand();

                int result = await _command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                TriggerCallbacks();
                return result;
            }
            finally
            {
                _dataSession.ReleaseConnection();
                Dispose();
            }
        }


        /// <summary>
        /// Executes the command against the connection and sends the resulting <see cref="IDataReader" /> to the readAction delegate.
        /// </summary>
        /// <param name="readAction">The read action delegate to pass the open <see cref="IDataReader" />.</param>
        public void Read(Action<IDataReader> readAction)
        {
            AssertDisposed();

            try
            {
                _dataSession.EnsureConnection();

                LogCommand();

                using (var reader = _command.ExecuteReader())
                    readAction(reader);

                TriggerCallbacks();
            }
            finally
            {
                _dataSession.ReleaseConnection();
                Dispose();
            }
        }

        /// <summary>
        /// Executes the command against the connection and sends the resulting <see cref="IDataReader" /> to the readAction delegate.
        /// </summary>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <param name="readAction">The read action delegate to pass the open <see cref="IDataReader" />.</param>
        public async Task ReadAsync(Action<IDataReader> readAction, CancellationToken cancellationToken = default)
        {
            AssertDisposed();

            try
            {
                await _dataSession.EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);

                LogCommand();

                using (var reader = await _command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                    readAction(reader);

                TriggerCallbacks();
            }
            finally
            {
                _dataSession.ReleaseConnection();
                Dispose();
            }
        }


        /// <summary>
        /// Disposes the managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            _command?.Dispose();
        }


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


        private string CacheKey<T>()
        {
            if (_dataSession.Cache == null)
                return null;

            if (_slidingExpiration == null && _absoluteExpiration == null)
                return null;

            int hashCode;
            string connectionString = _command.Connection.ConnectionString;
            string commandText = _command.CommandText;
            var commandType = _command.CommandType;
            var type = typeof(T);

            unchecked
            {
                hashCode = 17; // seed with prime
                hashCode = hashCode * 23 + (connectionString?.GetHashCode() ?? 0);
                hashCode = hashCode * 23 + commandType.GetHashCode();
                hashCode = hashCode * 23 + (commandText?.GetHashCode() ?? 0);
                hashCode = hashCode * 23 + type.GetHashCode();

                foreach (IDbDataParameter parameter in _command.Parameters)
                {
                    if (parameter.Direction == ParameterDirection.InputOutput
                        || parameter.Direction == ParameterDirection.Output
                        || parameter.Direction == ParameterDirection.ReturnValue)
                        throw new InvalidOperationException("A command with Output or Return parameters can not be cached.");

                    string name = parameter.ParameterName;
                    object value = parameter.Value;

                    hashCode = hashCode * 23 + (name?.GetHashCode() ?? 0);
                    hashCode = hashCode * 23 + (value?.GetHashCode() ?? 0);
                    hashCode = hashCode * 23 + parameter.DbType.GetHashCode();
                }
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

            object cacheValue = cache.Get(key);

            if (cacheValue != null)
                _dataSession.WriteLog("Cache Hit: " + key);

            return cacheValue;
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

            _dataSession.WriteLog("Cache Set: " + key);

            if (_absoluteExpiration.HasValue)
                cache.Set(key, value, _absoluteExpiration.Value);
            else
                cache.Set(key, value, _slidingExpiration.Value);
        }


        private void LogCommand()
        {
            LogCommand(_dataSession.WriteLog, _command);
        }

        private static void LogCommand(Action<string> writer, IDbCommand command)
        {
            if (writer == null)
                return;

            var buffer = new StringBuilder(command.CommandText);
            buffer.AppendLine();

            const string parameterFormat = "-- {0}: {1} {2} (Size = {3}; Precision = {4}; Scale = {5}) [{6}]";
            foreach (IDataParameter parameter in command.Parameters)
            {
                int precision = 0;
                int scale = 0;
                int size = 0;

                if (parameter is IDbDataParameter dataParameter)
                {
                    precision = dataParameter.Precision;
                    scale = dataParameter.Scale;
                    size = dataParameter.Size;
                }

                buffer.AppendFormat(parameterFormat,
                    parameter.ParameterName,
                    parameter.Direction,
                    parameter.DbType,
                    size,
                    precision,
                    scale,
                    parameter.Value);

                buffer.AppendLine();
            }

            writer(buffer.ToString());
        }
    }
}