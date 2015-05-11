using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Runtime.Caching;
using System.Text;
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
        private CacheItemPolicy _cachePolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCommand" /> class.
        /// </summary>
        /// <param name="dataSession">The data session.</param>
        /// <param name="transaction">The DbTransaction for this DataCommand.</param>
        internal DataCommand(IDataSession dataSession, DbTransaction transaction)
        {
            _callbacks = new Queue<DataCallback>();
            _dataSession = dataSession;
            _command = dataSession.Connection.CreateCommand();
            _command.Transaction = transaction;
        }

        /// <summary>
        /// Gets the underlying <see cref="DbCommand"/> for this <see cref="DataCommand"/>.
        /// </summary>
        public DbCommand Command
        {
            get { return _command; }
        }

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
        /// Adds the parameters to the underlying command.
        /// </summary>
        /// <param name="parameters">The <see cref="T:IEnumerable`1"/> of <see cref="T:DbParameter"/>.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public IDataCommand Parameter(IEnumerable<DbParameter> parameters)
        {
            foreach (var parameter in parameters)
                _command.Parameters.Add(parameter);

            return this;
        }

        /// <summary>
        /// Adds the parameter to the underlying command.
        /// </summary>
        /// <param name="parameter">The <see cref="DbParameter"/> to add.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public IDataCommand Parameter(DbParameter parameter)
        {
            _command.Parameters.Add(parameter);
            return this;
        }

        /// <summary>
        /// Adds a new parameter with the <see cref="IDataParameter" /> fluent object.
        /// </summary>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="configurator">The <see langword="delegate" />  to configurator the <see cref="IDataParameter" />.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public IDataCommand Parameter<TParameter>(Action<IDataParameter<TParameter>> configurator)
        {
            var parameter = _command.CreateParameter();

            configurator(new DataParameter<TParameter>(this, parameter));

            return Parameter(parameter);
        }

        /// <summary>
        /// Adds a new parameter with the specified <paramref name="name" /> and <paramref name="value" />.
        /// </summary>
        /// <typeparam name="TParameter">The type of the parameter value.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value to be added.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public IDataCommand Parameter<TParameter>(string name, TParameter value)
        {
            // convert to object
            object innerValue = value;

            // handle value type by using actual value
            Type valueType = value != null ? value.GetType() : typeof(TParameter);

            DbParameter parameter = _command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = innerValue ?? DBNull.Value;
            parameter.DbType = valueType.GetUnderlyingType().ToDbType();
            parameter.Direction = ParameterDirection.Input;

            return Parameter(parameter);
        }

        /// <summary>
        /// Adds a new out parameter with the specified <paramref name="name" /> and <paramref name="callback" />.
        /// </summary>
        /// <typeparam name="TParameter">The type of the parameter value.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="callback">The callback used to get the out value.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public IDataCommand ParameterOut<TParameter>(string name, Action<TParameter> callback)
        {
            var parameter = _command.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = typeof(TParameter).GetUnderlyingType().ToDbType();
            parameter.Direction = ParameterDirection.Output;
            // output parameters must have a size, default to MAX
            parameter.Size = -1;

            RegisterCallback(parameter, callback);

            return Parameter(parameter);
        }

        /// <summary>
        /// Adds a new out parameter with the specified <paramref name="name" />, <paramref name="value" /> and <paramref name="callback" />.
        /// </summary>
        /// <typeparam name="TParameter">The type of the parameter value.</typeparam>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value to be added.</param>
        /// <param name="callback">The callback used to get the out value.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public IDataCommand ParameterOut<TParameter>(string name, TParameter value, Action<TParameter> callback)
        {
            object innerValue = value;

            var parameter = _command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = innerValue ?? DBNull.Value;
            parameter.DbType = typeof(TParameter).GetUnderlyingType().ToDbType();
            parameter.Direction = ParameterDirection.InputOutput;

            RegisterCallback(parameter, callback);

            return Parameter(parameter);
        }


        /// <summary>
        /// Adds a new return parameter with the specified <paramref name="callback" />.
        /// </summary>
        /// <typeparam name="TParameter">The type of the parameter value.</typeparam>
        /// <param name="callback">The callback used to get the return value.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        public IDataCommand Return<TParameter>(Action<TParameter> callback)
        {
            const string parameterName = "@ReturnValue";

            var parameter = _command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = typeof(TParameter).GetUnderlyingType().ToDbType();
            parameter.Direction = ParameterDirection.ReturnValue;

            RegisterCallback(parameter, callback);

            return Parameter(parameter);
        }


        /// <summary>
        /// Uses <see cref="MemoryCache" /> to insert and retrieve cached results for the command.
        /// </summary>
        /// <param name="policy">A <see cref="CacheItemPolicy" /> that contains eviction details for the cache entry..</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to the data command.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">A command with Output or Return parameters can not be cached.</exception>
        public IDataCommand UseCache(CacheItemPolicy policy)
        {
            _cachePolicy = policy;
            if (_cachePolicy != null && _callbacks.Count > 0)
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
            if (_dataSession.DataCache != null && cacheKey != null)
                _dataSession.DataCache.Remove(cacheKey);

            return this;
        }


        /// <summary>
        /// Executes the command against the connection and converts the results to dynamic objects.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of dynamic objects.
        /// </returns>
        public IEnumerable<dynamic> Query()
        {
            return Query(DataFactory.DynamicFactory);
        }

        /// <summary>
        /// Executes the command against the connection and converts the results to <typeparamref name="TEntity" /> objects.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>
        /// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <typeparamref name="TEntity" /> objects.
        /// </returns>
        public IEnumerable<TEntity> Query<TEntity>()
            where TEntity : class, new()
        {
            return Query(DataFactory.EntityFactory<TEntity>);
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
                throw new ArgumentNullException("factory");

            AssertDisposed();

            try
            {
                string cacheKey = CacheKey<TEntity>();
                var results = GetCache(cacheKey) as List<TEntity>;
                if (results != null)
                    return results;

                results = new List<TEntity>();
                _dataSession.EnsureConnection();

                LogCommand();
                using (var reader = _command.ExecuteReader())
                {
                    while (reader.Read())
                        results.Add(factory(reader));
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
        /// Executes the query and returns the first row in the result as a dynamic object.
        /// </summary>
        /// <returns>
        /// A instance of a dynamic object if row exists; otherwise null.
        /// </returns>
        public dynamic QuerySingle()
        {
            return QuerySingle(DataFactory.DynamicFactory);
        }

        /// <summary>
        /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>
        /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
        /// </returns>
        public TEntity QuerySingle<TEntity>()
            where TEntity : class, new()
        {
            return QuerySingle(DataFactory.EntityFactory<TEntity>);
        }

        /// <summary>
        /// Executes the query and returns the first row in the result as a <typeparamref name="TEntity" /> object.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="factory">The <see langword="delegate" /> factory to convert the <see cref="T:System.Data.IDataReader" /> to <typeparamref name="TEntity" />.</param>
        /// <returns>
        /// A instance of <typeparamref name="TEntity" /> if row exists; otherwise null.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">factory</exception>
        public TEntity QuerySingle<TEntity>(Func<IDataReader, TEntity> factory)
            where TEntity : class
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            AssertDisposed();

            try
            {
                string cacheKey = CacheKey<TEntity>();
                var result = GetCache(cacheKey) as TEntity;
                if (result != null)
                    return result;

                _dataSession.EnsureConnection();

                LogCommand();

                using (var reader = _command.ExecuteReader())
                {
                    result = reader.Read()
                        ? factory(reader)
                        : default(TEntity);
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
        /// <returns>
        /// The value of the first column of the first row in the result set.
        /// </returns>
        public TValue QueryValue<TValue>()
        {
            return QueryValue<TValue>(null);
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
                    return DataFactory.ConvertValue(result, convert);

                _dataSession.EnsureConnection();

                LogCommand();

                result = _command.ExecuteScalar();
                var value = DataFactory.ConvertValue(result, convert);

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
                var dataTable = GetCache(cacheKey) as DataTable;
                if (dataTable != null)
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
        /// Executes the command against the connection and sends the resulting <see cref="IDataQuery" /> for reading multiple results sets.
        /// </summary>
        /// <param name="queryAction">The query action delegate to pass the open <see cref="IDataQuery" /> for reading multiple results.</param>
        public void QueryMultiple(Action<IDataQuery> queryAction)
        {
            if (queryAction == null)
                throw new ArgumentNullException("queryAction");

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
        /// Disposes the managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            if (_command != null)
                _command.Dispose();
        }


        internal void RegisterCallback<TParameter>(DbParameter parameter, Action<TParameter> callback)
        {
            var dataCallback = new DataCallback
            {
                Callback = callback,
                Type = typeof(TParameter),
                Parameter = parameter
            };
            _callbacks.Enqueue(dataCallback);
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
            if (_cachePolicy == null)
                return null;

            int hashCode;
            string connectionString = _command.Connection.ConnectionString;
            string commandText = _command.CommandText;
            var commandType = _command.CommandType;
            Type type = typeof(T);

            unchecked
            {
                hashCode = 17; // seed with prime
                hashCode = hashCode * 23 + (connectionString == null ? 0 : connectionString.GetHashCode());
                hashCode = hashCode * 23 + commandType.GetHashCode();
                hashCode = hashCode * 23 + (commandText == null ? 0 : commandText.GetHashCode());
                hashCode = hashCode * 23 + type.GetHashCode();

                foreach (DbParameter parameter in _command.Parameters)
                {
                    if (parameter.Direction == ParameterDirection.InputOutput
                        || parameter.Direction == ParameterDirection.Output
                        || parameter.Direction == ParameterDirection.ReturnValue)
                        throw new InvalidOperationException("A command with Output or Return parameters can not be cached.");

                    string name = parameter.ParameterName;
                    object value = parameter.Value;

                    hashCode = hashCode * 23 + (name == null ? 0 : name.GetHashCode());
                    hashCode = hashCode * 23 + (value == null ? 0 : value.GetHashCode());
                    hashCode = hashCode * 23 + parameter.DbType.GetHashCode();
                }
            }

            return string.Format("global:novus:data:{0}", hashCode);
        }

        private object GetCache(string key)
        {
            if (_cachePolicy == null || key == null)
                return null;

            object cacheValue = _dataSession.DataCache.Get(key);

            if (cacheValue != null)
                _dataSession.WriteLog("Cache Hit: " + key);

            return cacheValue;
        }

        private void SetCache(string key, object value)
        {
            if (_cachePolicy == null || key == null || value == null)
                return;

            var item = new CacheItem(key, value);

            _dataSession.WriteLog("Cache Set: " + key);

            _dataSession.DataCache.Set(item, _cachePolicy);
        }

        private void LogCommand()
        {
            LogCommand(_dataSession.WriteLog, _command);
        }

        private static void LogCommand(Action<string> writer, DbCommand command)
        {
            if (writer == null)
                return;

            var buffer = new StringBuilder(command.CommandText);
            buffer.AppendLine();

            const string parameterFormat = "-- {0}: {1} {2} (Size = {3}; Precision = {4}; Scale = {5}) [{6}]";
            foreach (DbParameter parameter in command.Parameters)
            {
                int precision = 0;
                int scale = 0;

                var dataParameter = parameter as IDbDataParameter;
                if (dataParameter != null)
                {
                    precision = dataParameter.Precision;
                    scale = dataParameter.Scale;
                }

                buffer.AppendFormat(parameterFormat,
                    parameter.ParameterName,
                    parameter.Direction,
                    parameter.DbType,
                    parameter.Size,
                    precision,
                    scale,
                    parameter.Value);

                buffer.AppendLine();
            }

            writer(buffer.ToString());
        }
    }
}