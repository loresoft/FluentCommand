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

    private readonly IDataInterceptor[] _interceptors;
    private readonly IDataConnectionInterceptor[] _connectionInterceptors;
    private readonly IDataCommandInterceptor[] _commandInterceptors;

    private bool _openedConnection;
    private int _connectionRequestCount;

    /// <summary>
    /// Gets the underlying <see cref="DbConnection"/> for the session.
    /// </summary>
    public DbConnection Connection { get; }

    /// <summary>
    /// Gets the underlying <see cref="DbTransaction"/> for the session.
    /// </summary>
    public DbTransaction? Transaction { get; private set; }

    /// <summary>
    /// Gets the underlying <see cref="IDataCache"/> for the session.
    /// </summary>
    public IDataCache? Cache { get; }

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
    public IDataQueryLogger? QueryLogger { get; }

    /// <summary>
    /// Gets the default command timeout in seconds.
    /// </summary>
    /// <value>
    /// The default command timeout in seconds.
    /// </value>
    public int? CommandTimeout { get; }

    /// <summary>
    /// Gets the interceptors registered for this session.
    /// </summary>
    /// <value>
    /// The list of <see cref="IDataInterceptor"/> instances active for this session.
    /// </value>
    public IReadOnlyList<IDataInterceptor> Interceptors => _interceptors;


    /// <summary>
    /// Initializes a new instance of the <see cref="DataSession" /> class.
    /// </summary>
    /// <param name="connection">The DbConnection to use for the session.</param>
    /// <param name="disposeConnection">if set to <c>true</c> dispose connection with this session.</param>
    /// <param name="cache">The <see cref="IDataCache" /> used to cached results of queries.</param>
    /// <param name="queryGenerator">The query generator provider.</param>
    /// <param name="logger">The logger delegate for writing log messages.</param>
    /// <param name="commandTimeout">The default command timeout in seconds.</param>
    /// <param name="interceptors">The interceptors to apply during this session's lifetime.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is null</exception>
    /// <exception cref="ArgumentException">Invalid connection string on <paramref name="connection" /> instance.</exception>
    public DataSession(
        DbConnection connection,
        bool disposeConnection = true,
        IDataCache? cache = null,
        IQueryGenerator? queryGenerator = null,
        IDataQueryLogger? logger = null,
        IEnumerable<IDataInterceptor>? interceptors = null,
        int? commandTimeout = null)
    {
        if (connection == null)
            throw new ArgumentNullException(nameof(connection));

        if (string.IsNullOrEmpty(connection.ConnectionString))
            throw new ArgumentException("Invalid connection string", nameof(connection));

        Connection = connection;
        Cache = cache;
        QueryGenerator = queryGenerator ?? new SqlServerGenerator();
        QueryLogger = logger;
        CommandTimeout = commandTimeout;

        _interceptors = interceptors is null ? [] : [.. interceptors];
        _connectionInterceptors = [.. _interceptors.OfType<IDataConnectionInterceptor>()];
        _commandInterceptors = [.. _interceptors.OfType<IDataCommandInterceptor>()];

        _disposeConnection = disposeConnection;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSession"/> class.
    /// </summary>
    /// <param name="transaction">The DbTransaction to use for the session.</param>
    /// <param name="disposeConnection">if set to <c>true</c> dispose connection with this session.</param>
    /// <param name="cache">The <see cref="IDataCache" /> used to cached results of queries.</param>
    /// <param name="queryGenerator">The query generator provider.</param>
    /// <param name="logger">The logger delegate for writing log messages.</param>
    /// <param name="commandTimeout">The default command timeout in seconds.</param>
    /// <param name="interceptors">The interceptors to apply during this session's lifetime.</param>
    /// <exception cref="ArgumentNullException"><paramref name="transaction" /> is null</exception>
    /// <exception cref="ArgumentException">Invalid connection string on <paramref name="transaction" /> instance.</exception>
    public DataSession(
        DbTransaction transaction,
        bool disposeConnection = false,
        IDataCache? cache = null,
        IQueryGenerator? queryGenerator = null,
        IDataQueryLogger? logger = null,
        IEnumerable<IDataInterceptor>? interceptors = null,
        int? commandTimeout = null)
        : this(GetTransactionConnection(transaction), disposeConnection, cache, queryGenerator, logger, interceptors, commandTimeout)
    {
        Transaction = transaction;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSession" /> class.
    /// </summary>
    /// <param name="dataConfiguration">The configuration for the session</param>
    /// <exception cref="ArgumentNullException"><paramref name="dataConfiguration"/> is null</exception>
    public DataSession(IDataConfiguration dataConfiguration)
    {
        if (dataConfiguration == null)
            throw new ArgumentNullException(nameof(dataConfiguration));

        Connection = dataConfiguration.CreateConnection();
        Cache = dataConfiguration.DataCache;
        QueryGenerator = dataConfiguration.QueryGenerator;
        QueryLogger = dataConfiguration.QueryLogger;
        CommandTimeout = dataConfiguration.CommandTimeout;

        _interceptors = dataConfiguration.Interceptors is null ? [] : [.. dataConfiguration.Interceptors];
        _connectionInterceptors = [.. _interceptors.OfType<IDataConnectionInterceptor>()];
        _commandInterceptors = [.. _interceptors.OfType<IDataCommandInterceptor>()];

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

#if NETCOREAPP3_0_OR_GREATER
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
        await EnsureConnectionAsync(cancellationToken).ConfigureAwait(false);
        Transaction = await Connection.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);

        return Transaction;
    }
#endif

    /// <summary>
    /// Starts a data command with the specified SQL.
    /// </summary>
    /// <param name="sql">The SQL statement.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a data command.
    /// </returns>
    public IDataCommand Sql(string sql)
    {
        var dataCommand = new DataCommand(this, Transaction, _commandInterceptors, commandTimeout: CommandTimeout);
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
        var dataCommand = new DataCommand(this, Transaction, _commandInterceptors, commandTimeout: CommandTimeout);
        return dataCommand.StoredProcedure(storedProcedureName);
    }


    /// <summary>
    /// Ensures the connection is open.
    /// </summary>
    /// <exception cref="InvalidOperationException">Failed to open connection</exception>
    public void EnsureConnection()
    {
        AssertDisposed();

        bool justOpened = false;
        if (ConnectionState.Closed == Connection.State)
        {
            Connection.Open();
            _openedConnection = true;
            justOpened = true;
        }

        if (_openedConnection)
            _connectionRequestCount++;

        // Check the connection was opened correctly
        if (Connection.State is ConnectionState.Closed or ConnectionState.Broken)
            throw new InvalidOperationException($"Execution of the command requires an open and available connection. The connection's current state is {Connection.State}.");

        // run connection opened interceptors only when the connection was just opened by this context
        if (justOpened)
        {
            foreach (var interceptor in _connectionInterceptors)
                interceptor.ConnectionOpened(Connection, this);
        }
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

        bool justOpened = false;
        if (ConnectionState.Closed == Connection.State)
        {
            await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            _openedConnection = true;
            justOpened = true;
        }

        if (_openedConnection)
            _connectionRequestCount++;

        // Check the connection was opened correctly
        if (Connection.State is ConnectionState.Closed or ConnectionState.Broken)
            throw new InvalidOperationException($"Execution of the command requires an open and available connection. The connection's current state is {Connection.State}.");

        // run connection opened interceptors only when the connection was just opened by this context
        if (justOpened)
        {
            foreach (var interceptor in _connectionInterceptors)
                await interceptor.ConnectionOpenedAsync(Connection, this, cancellationToken).ConfigureAwait(false);
        }
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
        foreach (var interceptor in _connectionInterceptors)
            interceptor.ConnectionClosing(Connection, this);

        Connection.Close();
        _openedConnection = false;
    }

#if NETCOREAPP3_0_OR_GREATER
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
        foreach (var interceptor in _connectionInterceptors)
            await interceptor.ConnectionClosingAsync(Connection, this).ConfigureAwait(false);

        await Connection.CloseAsync().ConfigureAwait(false);
        _openedConnection = false;
    }

    /// <summary>
    /// Disposes the managed resources.
    /// </summary>
    protected override async ValueTask DisposeResourcesAsync()
    {
        if (!_disposeConnection)
            return;

        if (Transaction is not null)
            await Transaction.DisposeAsync().ConfigureAwait(false);

        if (_openedConnection)
        {
            foreach (var interceptor in _connectionInterceptors)
                await interceptor.ConnectionClosingAsync(Connection, this).ConfigureAwait(false);

            _openedConnection = false;
        }

        await Connection.DisposeAsync().ConfigureAwait(false);
    }
#endif

    /// <summary>
    /// Disposes the managed resources.
    /// </summary>
    protected override void DisposeManagedResources()
    {
        if (!_disposeConnection)
            return;

        Transaction?.Dispose();

        if (_openedConnection)
        {
            foreach (var interceptor in _connectionInterceptors)
                interceptor.ConnectionClosing(Connection, this);

            _openedConnection = false;
        }

        Connection.Dispose();
    }

    private static DbConnection GetTransactionConnection(DbTransaction transaction)
    {
        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction));

        return transaction.Connection
            ?? throw new ArgumentException("Transaction has no associated connection.", nameof(transaction));
    }
}

/// <summary>
/// A fluent class for a data session by discriminator.  Used to register multiple instances of IDataSession.
/// </summary>
/// <typeparam name="TDiscriminator">The type of the discriminator.</typeparam>
/// <seealso cref="FluentCommand.DisposableBase" />
/// <seealso cref="FluentCommand.IDataSession" />
public class DataSession<TDiscriminator> : DataSession, IDataSession<TDiscriminator>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataSession" /> class.
    /// </summary>
    /// <param name="connection">The DbConnection to use for the session.</param>
    /// <param name="disposeConnection">if set to <c>true</c> dispose connection with this session.</param>
    /// <param name="cache">The <see cref="IDataCache" /> used to cached results of queries.</param>
    /// <param name="queryGenerator">The query generator provider.</param>
    /// <param name="logger">The logger delegate for writing log messages.</param>
    /// <param name="commandTimeout">The default command timeout in seconds.</param>
    /// <param name="interceptors">The interceptors to apply during this session's lifetime.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connection" /> is null</exception>
    /// <exception cref="ArgumentException">Invalid connection string on <paramref name="connection" /> instance.</exception>
    public DataSession(
        DbConnection connection,
        bool disposeConnection = true,
        IDataCache? cache = null,
        IQueryGenerator? queryGenerator = null,
        IDataQueryLogger? logger = null,
        IEnumerable<IDataInterceptor>? interceptors = null,
        int? commandTimeout = null)
        : base(connection, disposeConnection, cache, queryGenerator, logger, interceptors, commandTimeout)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSession"/> class.
    /// </summary>
    /// <param name="transaction">The DbTransaction to use for the session.</param>
    /// <param name="disposeConnection">if set to <c>true</c> dispose connection with this session.</param>
    /// <param name="cache">The <see cref="IDataCache" /> used to cached results of queries.</param>
    /// <param name="queryGenerator">The query generator provider.</param>
    /// <param name="logger">The logger delegate for writing log messages.</param>
    /// <param name="commandTimeout">The default command timeout in seconds.</param>
    /// <param name="interceptors">The interceptors to apply during this session's lifetime.</param>
    /// <exception cref="ArgumentNullException"><paramref name="transaction" /> is null</exception>
    /// <exception cref="ArgumentException">Invalid connection string on <paramref name="transaction" /> instance.</exception>
    public DataSession(
        DbTransaction transaction,
        bool disposeConnection = false,
        IDataCache? cache = null,
        IQueryGenerator? queryGenerator = null,
        IDataQueryLogger? logger = null,
        IEnumerable<IDataInterceptor>? interceptors = null,
        int? commandTimeout = null)
        : base(transaction, disposeConnection, cache, queryGenerator, logger, interceptors, commandTimeout)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSession" /> class.
    /// </summary>
    /// <param name="dataConfiguration">The configuration for the session</param>
    /// <exception cref="ArgumentNullException"><paramref name="dataConfiguration" /> is null</exception>
    public DataSession(IDataConfiguration<TDiscriminator> dataConfiguration)
        : base(dataConfiguration)
    {
    }
}
