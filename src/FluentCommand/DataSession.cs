using System.Data;
using System.Data.Common;

using FluentCommand.Query.Generators;

namespace FluentCommand;

/// <summary>
/// A fluent class for a data session.
/// </summary>
/// <seealso cref="FluentCommand.DisposableBase" />
/// <seealso cref="FluentCommand.IDataSession" />
public class DataSession : DisposableBase, IDataSession
{
    private readonly bool _disposeConnection;

    private bool _openedConnection;
    private int _connectionRequestCount;

    /// <summary>
    /// Gets the underlying <see cref="DbConnection"/> for the session.
    /// </summary>
    public DbConnection Connection { get; }

    /// <summary>
    /// Gets the underlying <see cref="DbTransaction"/> for the session.
    /// </summary>
    public DbTransaction Transaction { get; private set; }

    /// <summary>
    /// Gets the underlying <see cref="IDataCache"/> for the session.
    /// </summary>
    public IDataCache Cache { get; }

    /// <summary>
    /// Gets the query generator provider.
    /// </summary>
    /// <value>
    /// The query generator provider.
    /// </value>
    public IQueryGenerator QueryGenerator { get; }

    /// <summary>
    /// Gets the data command query logger.
    /// </summary>
    /// <value>
    /// The data command query logger.
    /// </value>
    public IDataQueryLogger QueryLogger { get; }


    /// <summary>
    /// Initializes a new instance of the <see cref="DataSession" /> class.
    /// </summary>
    /// <param name="connection">The IDbConnection to use for the session.</param>
    /// <param name="disposeConnection">if set to <c>true</c> dispose connection with this session.</param>
    /// <param name="cache">The <see cref="IDataCache" /> used to cached results of queries.</param>
    /// <param name="queryGenerator">The query generator provider.</param>
    /// <param name="logger">The logger delegate for writing log messages.</param>
    /// <exception cref="System.ArgumentNullException">connection</exception>
    /// <exception cref="System.ArgumentException">Invalid connection string - connection</exception>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is null</exception>
    /// <exception cref="ArgumentException">Invalid connection string on <paramref name="connection" /> instance.</exception>
    public DataSession(DbConnection connection, bool disposeConnection = true, IDataCache cache = null, IQueryGenerator queryGenerator = null, IDataQueryLogger logger = null)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        if (string.IsNullOrEmpty(connection.ConnectionString))
            throw new ArgumentException("Invalid connection string", nameof(connection));

        Connection = connection;
        Cache = cache;
        QueryGenerator = queryGenerator ?? new SqlServerGenerator();
        QueryLogger = logger;

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


        Connection = dataConfiguration.CreateConnection();
        Cache = dataConfiguration.DataCache;
        QueryGenerator = dataConfiguration.QueryGenerator;
        QueryLogger = dataConfiguration.QueryLogger;
        _disposeConnection = true;
    }


    /// <summary>
    /// Starts a database transaction with the specified isolation level.
    /// </summary>
    /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
    /// <returns>
    /// A <see cref="DbTransaction" /> representing the new transaction.
    /// </returns>
    public DbTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
    {
        EnsureConnection();
        Transaction = Connection.BeginTransaction(isolationLevel);

        return Transaction;
    }

#if !NETSTANDARD2_0
    /// <summary>
    /// Starts a database transaction with the specified isolation level.
    /// </summary>
    /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// A <see cref="DbTransaction" /> representing the new transaction.
    /// </returns>
    public async Task<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default)
    {
        await EnsureConnectionAsync();
        Transaction = await Connection.BeginTransactionAsync(isolationLevel, cancellationToken);

        return Transaction;
    }
#endif

    /// <summary>
    /// Uses the specified transaction for this session.
    /// </summary>
    /// <param name="transaction">The transaction to use for session.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to the session.
    /// </returns>
    public IDataSession UseTransaction(DbTransaction transaction)
    {
        Transaction = transaction;
        return this;
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
        var dataCommand = new DataCommand(this, Transaction);
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
        var dataCommand = new DataCommand(this, Transaction);
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
            Connection.Open();
            _openedConnection = true;
        }

        if (_openedConnection)
            _connectionRequestCount++;

        // Check the connection was opened correctly
        if (Connection.State == ConnectionState.Closed || Connection.State == ConnectionState.Broken)
            throw new InvalidOperationException($"Execution of the command requires an open and available connection. The connection's current state is {Connection.State}.");
    }

    /// <summary>
    /// Ensures the connection is open asynchronous.
    /// </summary>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Failed to open connection</exception>
    public async Task EnsureConnectionAsync(CancellationToken cancellationToken = default)
    {
        AssertDisposed();

        if (ConnectionState.Closed == Connection.State)
        {
            await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            _openedConnection = true;
        }

        if (_openedConnection)
            _connectionRequestCount++;

        // Check the connection was opened correctly
        if (Connection.State == ConnectionState.Closed || Connection.State == ConnectionState.Broken)
            throw new InvalidOperationException($"Execution of the command requires an open and available connection. The connection's current state is {Connection.State}.");
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
        Connection.Close();
        _openedConnection = false;
    }

#if !NETSTANDARD2_0
    /// <summary>
    /// Releases the connection.
    /// </summary>
    public async Task ReleaseConnectionAsync()
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
        await Connection.CloseAsync();
        _openedConnection = false;
    }

    /// <summary>
    /// Disposes the managed resources.
    /// </summary>
    protected override async ValueTask DisposeResourcesAsync()
    {
        // Release managed resources here.
        if (Connection != null)
        {
            // Dispose the connection created
            if (_disposeConnection)
                await Connection.DisposeAsync();
        }
    }
#endif

    /// <summary>
    /// Disposes the managed resources.
    /// </summary>
    protected override void DisposeManagedResources()
    {
        // Release managed resources here.
        if (Connection != null)
        {
            // Dispose the connection created
            if (_disposeConnection)
                Connection.Dispose();
        }
    }
}
