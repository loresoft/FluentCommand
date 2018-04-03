using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace FluentCommand
{
    /// <summary>
    /// A fluent class for a data session.
    /// </summary>
    public class DataSession : DisposableBase, IDataSession
    {
        private readonly DbConnection _connection;
        private readonly IDataCache _cache;
        private readonly Action<string> _logger;
        private readonly bool _disposeConnection;

        private bool _openedConnection;
        private int _connectionRequestCount;
        private DbTransaction _dbTransaction;

        /// <summary>
        /// Gets the underlying <see cref="DbConnection"/> for the session.
        /// </summary>
        public DbConnection Connection => _connection;

        /// <summary>
        /// Gets the underlying <see cref="DbTransaction"/> for the session.
        /// </summary>
        public DbTransaction Transaction => _dbTransaction;

        /// <summary>
        /// Gets the underlying <see cref="IDataCache"/> for the session.
        /// </summary>
        public IDataCache Cache => _cache;


        /// <summary>
        /// Initializes a new instance of the <see cref="DataSession" /> class.
        /// </summary>
        /// <param name="connection">The IDbConnection to use for the session.</param>
        /// <param name="disposeConnection">if set to <c>true</c> dispose connection with this session.</param>
        /// <param name="cache">The <see cref="IDataCache"/> used to cached results of queries.</param>
        /// <param name="logger">The logger delegate for writing log messages.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is null</exception>
        /// <exception cref="ArgumentException">Invalid connection string on <paramref name="connection"/> instance.</exception>
        public DataSession(DbConnection connection, bool disposeConnection = true, IDataCache cache = null, Action<string> logger = null)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            if (string.IsNullOrEmpty(connection.ConnectionString))
                throw new ArgumentException("Invalid connection string", nameof(connection));
            
            _connection = connection;
            _cache = cache;
            _logger = logger;
            _disposeConnection = disposeConnection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSession" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="dataConfiguration"/> is null</exception>
        public DataSession(IDataConfiguration dataConfiguration)
        {
            if (dataConfiguration == null)
                throw new ArgumentNullException(nameof(dataConfiguration));


            _connection = dataConfiguration.CreateConnection();
            _cache = dataConfiguration.DataCache;
            _logger = dataConfiguration.Logger;
            _disposeConnection = true;
        }


        /// <summary>
        /// Starts a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
        /// <returns>
        /// A <see cref="DbTransaction" /> representing the new transaction.
        /// </returns>
        public DbTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.Unspecified)
        {
            EnsureConnection();
            _dbTransaction = Connection.BeginTransaction(isolationLevel);

            return _dbTransaction;
        }

        /// <summary>
        /// Starts a data command with the specified SQL.
        /// </summary>
        /// <param name="sql">The SQL statement.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a data command.
        /// </returns>
        public IDataCommand Sql(string sql)
        {
            var dataCommand = new DataCommand(this, _dbTransaction);
            return dataCommand.Sql(sql);
        }

        /// <summary>
        /// Starts a data command with the specified stored procedure name.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a data command.
        /// </returns>
        public IDataCommand StoredProcedure(string storedProcedureName)
        {
            var dataCommand = new DataCommand(this, _dbTransaction);
            return dataCommand.StoredProcedure(storedProcedureName);
        }


        /// <summary>
        /// Ensures the connection is open.
        /// </summary>
        /// <exception cref="InvalidOperationException">Failed to open connection</exception>
        public void EnsureConnection()
        {
            AssertDisposed();

            if (ConnectionState.Closed == Connection.State)
            {
                _connection.Open();
                _openedConnection = true;
            }

            if (_openedConnection)
                _connectionRequestCount++;

            // Check the connection was opened correctly
            if (_connection.State == ConnectionState.Closed || _connection.State == ConnectionState.Broken)
                throw new InvalidOperationException($"Execution of the command requires an open and available connection. The connection's current state is {_connection.State}.");
        }

        /// <summary>
        /// Ensures the connection is open asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation instruction.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Failed to open connection</exception>
        public async Task EnsureConnectionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            AssertDisposed();

            if (ConnectionState.Closed == Connection.State)
            {
                await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                _openedConnection = true;
            }

            if (_openedConnection)
                _connectionRequestCount++;

            // Check the connection was opened correctly
            if (_connection.State == ConnectionState.Closed || _connection.State == ConnectionState.Broken)
                throw new InvalidOperationException($"Execution of the command requires an open and available connection. The connection's current state is {_connection.State}.");
        }

        /// <summary>
        /// Releases the connection.
        /// </summary>
        public void ReleaseConnection()
        {
            AssertDisposed();

            if (!_openedConnection)
                return;

            if (_connectionRequestCount > 0)
                _connectionRequestCount--;

            if (_connectionRequestCount != 0)
                return;
            
            // When no operation is using the connection and the context had opened the connection
            // the connection can be closed
            _connection.Close();
            _openedConnection = false;
        }


        /// <summary>
        /// Writes the log message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteLog(string message)
        {
            _logger?.Invoke(message);
        }


        /// <summary>
        /// Disposes the managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            // Release managed resources here.
            if (_connection != null)
            {
                // Dispose the connection created
                if (_disposeConnection)
                    _connection.Dispose();
            }
        }
    }
}
