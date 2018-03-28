using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Runtime.Caching;
using System.Text;
using System.Transactions;
using FluentCommand.Extensions;

namespace FluentCommand
{
    /// <summary>
    /// A fluent class for a data session.
    /// </summary>
    public class DataSession : DisposableBase, IDataSession
    {
        private DbConnection _connection;
        private bool _createdConnection;
        private bool _openedConnection;
        private int _connectionRequestCount;
        private Transaction _lastTransaction;
        private ObjectCache _cache;
        private DbTransaction _dbTransaction;

        /// <summary>
        /// Gets the underlying <see cref="DbConnection"/> for the session.
        /// </summary>
        public DbConnection Connection
        {
            get { return _connection; }
        }

        /// <summary>
        /// Gets the underlying <see cref="DbTransaction"/> for the session.
        /// </summary>
        public DbTransaction Transaction
        {
            get { return _dbTransaction; }
        }

        /// <summary>
        /// Gets the underlying <see cref="ObjectCache"/> for the session.
        /// </summary>
        public ObjectCache DataCache
        {
            get { return _cache; }
        }

        /// <summary>
        /// Gets or sets the command logger.
        /// </summary>
        internal Action<string> Logger { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSession" /> class.
        /// </summary>
        /// <param name="connectionName">Name of the connection.</param>
        public DataSession(string connectionName)
            : this()
        {
            Initialize(connectionName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSession" /> class.
        /// </summary>
        /// <param name="connection">The DbConnection to use for the session.</param>
        public DataSession(DbConnection connection)
            : this()
        {
            _connection = connection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSession" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string use with ths session.</param>
        /// <param name="providerName">Name of the <see cref="DbProviderFactory" /> to use with the session.</param>
        public DataSession(string connectionString, string providerName)
            : this()
        {
            Initialize(connectionString, providerName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSession" /> class.
        /// </summary>
        protected DataSession()
        {
            _cache = MemoryCache.Default;
        }

        /// <summary>
        /// Writes log messages to the logger <see langword="delegate"/>.
        /// </summary>
        /// <param name="logger">The logger <see langword="delegate"/> to write messages to.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> to a data session.
        /// </returns>
        public IDataSession Log(Action<string> logger)
        {
            Logger = logger;
            return this;
        }

        /// <summary>
        /// Set the underlying <see cref="ObjectCache"/> used to store cached result.
        /// </summary>
        /// <param name="cache">The <see cref="ObjectCache"/> used to store cached results.</param>
        /// A fluent <see langword="interface" /> to a data session.
        /// <exception cref="System.ArgumentNullException">cache</exception>
        public IDataSession Cache(ObjectCache cache)
        {
            if (cache == null)
                throw new ArgumentNullException("cache");

            _cache = cache;
            return this;
        }

        /// <summary>
        /// Starts a database transaction with the specified isolation level.
        /// </summary>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
        /// <returns>
        /// An <see cref="IDbTransaction" /> representing the new transaction.
        /// </returns>
        public IDbTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.Unspecified)
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
        public void EnsureConnection()
        {
            AssertDisposed();

            // wire up info messages
            var sqlConnection = Connection as SqlConnection;
            if (sqlConnection != null)
                sqlConnection.InfoMessage += InfoMessage;

            if (ConnectionState.Closed == Connection.State)
            {
                Connection.Open();
                _openedConnection = true;
            }

            if (_openedConnection)
                _connectionRequestCount++;

            // Check the connection was opened correctly
            if (Connection.State == ConnectionState.Closed || Connection.State == ConnectionState.Broken)
                throw new InvalidOperationException("Execution of the command requires an open and available connection. The connection's current state is {0}."
                                                        .FormatWith(Connection.State));

            try
            {
                var currentTransaction = System.Transactions.Transaction.Current;

                var transactionHasChanged = (null != currentTransaction && !currentTransaction.Equals(_lastTransaction)) ||
                                            (null != _lastTransaction && !_lastTransaction.Equals(currentTransaction));

                if (transactionHasChanged)
                {
                    if (!_openedConnection)
                    {
                        if (currentTransaction != null)
                        {
                            Connection.EnlistTransaction(currentTransaction);
                        }
                    }
                    else if (_connectionRequestCount > 1)
                    {
                        if (null == _lastTransaction)
                        {
                            Connection.EnlistTransaction(currentTransaction);
                        }
                        else
                        {
                            Connection.Close();
                            Connection.Open();
                            _openedConnection = true;
                            _connectionRequestCount++;
                        }
                    }
                }

                _lastTransaction = currentTransaction;
            }
            catch (Exception)
            {
                ReleaseConnection();
                throw;
            }

        }

        /// <summary>
        /// Releases the connection.
        /// </summary>
        public void ReleaseConnection()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(null,
                    "The DataSession instance has been disposed and can no longer be used for operations that require a connection.");

            if (!_openedConnection)
                return;

            if (_connectionRequestCount > 0)
            {
                _connectionRequestCount--;
            }

            // When no operation is using the connection and the context had opened the connection
            // the connection can be closed
            if (_connectionRequestCount == 0)
            {
                Connection.Close();
                _openedConnection = false;
            }

            // unwire info messages
            var sqlConnection = Connection as SqlConnection;
            if (sqlConnection != null)
                sqlConnection.InfoMessage -= InfoMessage;

        }

        /// <summary>
        /// Writes the log message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteLog(string message)
        {
            if (Logger == null)
                return;

            Logger(message);
        }

        /// <summary>
        /// Disposes the managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            // Release managed resources here.
            if (_connection != null)
            {
                // Dispose the connection the ObjectContext created
                if (_createdConnection)
                {
                    _connection.Dispose();
                }
            }
            _connection = null; // Marks this object as disposed.
        }

        private void InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            if (Logger == null)
                return;

            foreach (SqlError error in e.Errors)
                WriteLog("SQL Message '{0}',  Number: {1}, Procedure: '{2}', Line: {3}".FormatWith(
                    error.Message, error.Number, error.Procedure, error.LineNumber));
        }

        private void Initialize(string connectionName)
        {
            var settings = ConfigurationManager.ConnectionStrings[connectionName];
            if (settings == null)
                throw new ConfigurationErrorsException(
                    "No connection string named '{0}' could be found in the application config file.".FormatWith(
                        connectionName));

            string connectionString = settings.ConnectionString;
            if (connectionString.IsNullOrEmpty())
                throw new ConfigurationErrorsException(
                    "The connection string '{0}' in the application's configuration file does not contain the required connectionString attribute.".FormatWith(
                        connectionName));

            string providerName = settings.ProviderName;
            if (providerName.IsNullOrEmpty())
                throw new ConfigurationErrorsException(
                    "The connection string '{0}' in the application's configuration file does not contain the required providerName attribute.".FormatWith(
                        connectionName));

            var providerFactory = DbProviderFactories.GetFactory(providerName);
            _connection = providerFactory.CreateConnection();
            if (_connection == null)
                throw new InvalidOperationException("The provider factory returned a null connection.");

            _connection.ConnectionString = connectionString;
            _createdConnection = true;
        }

        private void Initialize(string connectionString, string providerName)
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");

            if (providerName == null)
                throw new ArgumentNullException("providerName");

            DbProviderFactory providerFactory = DbProviderFactories.GetFactory(providerName);
            _connection = providerFactory.CreateConnection();
            if (_connection == null)
                throw new InvalidOperationException("The provider factory returned a null connection.");

            _connection.ConnectionString = connectionString;
            _createdConnection = true;
        }
    }
}
