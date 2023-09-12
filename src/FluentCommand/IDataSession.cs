using System.Data;
using System.Data.Common;

using FluentCommand.Query.Generators;

namespace FluentCommand;

/// <summary>
/// An <see langword="interface"/> for data sessions.
/// </summary>
public interface IDataSession
    : IDisposable
#if NETCOREAPP3_0_OR_GREATER
    , IAsyncDisposable
#endif
{
    /// <summary>
    /// Gets the underlying <see cref="DbConnection"/> for the session.
    /// </summary>
    DbConnection Connection { get; }

    /// <summary>
    /// Gets the underlying <see cref="DbTransaction"/> for the session.
    /// </summary>
    DbTransaction Transaction { get; }

    /// <summary>
    /// Gets the underlying <see cref="IDataCache"/> for the session.
    /// </summary>
    IDataCache Cache { get; }

    /// <summary>
    /// Gets the query generator provider.
    /// </summary>
    /// <value>
    /// The query generator provider.
    /// </value>
    IQueryGenerator QueryGenerator { get; }

    /// <summary>
    /// Gets the data command query logger.
    /// </summary>
    /// <value>
    /// The data command query logger.
    /// </value>
    IDataQueryLogger QueryLogger { get; }

    /// <summary>
    /// Starts a database transaction with the specified isolation level.
    /// </summary>
    /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
    /// <returns>A <see cref="DbTransaction"/> representing the new transaction.</returns>
    DbTransaction BeginTransaction(IsolationLevel isolationLevel);

#if NETCOREAPP3_0_OR_GREATER
    /// <summary>
    /// Starts a database transaction with the specified isolation level.
    /// </summary>
    /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// A <see cref="DbTransaction" /> representing the new transaction.
    /// </returns>
    Task<DbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken cancellationToken = default);
#endif


    /// <summary>
    /// Starts a data command with the specified SQL.
    /// </summary>
    /// <param name="sql">The SQL statement.</param>
    /// <returns>A fluent <see langword="interface"/> to a data command.</returns>
    IDataCommand Sql(string sql);

    /// <summary>
    /// Starts a data command with the specified stored procedure name.
    /// </summary>
    /// <param name="storedProcedureName">Name of the stored procedure.</param>
    /// <returns>A fluent <see langword="interface"/> to a data command.</returns>
    IDataCommand StoredProcedure(string storedProcedureName);


    /// <summary>
    /// Ensures the connection is open.
    /// </summary>
    void EnsureConnection();

    /// <summary>
    /// Ensures the connection is open asynchronous.
    /// </summary>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Failed to open connection</exception>
    Task EnsureConnectionAsync(CancellationToken cancellationToken = default);


    /// <summary>
    /// Releases the connection.
    /// </summary>
    void ReleaseConnection();

#if NETCOREAPP3_0_OR_GREATER
    /// <summary>
    /// Releases the connection.
    /// </summary>
    Task ReleaseConnectionAsync();
#endif
}

/// <summary>
/// A fluent interface for a data session by discriminator.  Used to register multiple instances of IDataSession.
/// </summary>
/// <typeparam name="TDiscriminator">The type of the discriminator.</typeparam>
public interface IDataSession<TDiscriminator> : IDataSession
{
    
}
