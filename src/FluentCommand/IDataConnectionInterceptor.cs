using System.Data.Common;

namespace FluentCommand;

/// <summary>
/// An interface for intercepting database connection lifecycle events.
/// </summary>
/// <remarks>
/// Use this interceptor to execute logic immediately after a connection is opened, such as
/// setting SQL Server session context for row-level security via <c>sp_set_session_context</c>.
/// </remarks>
public interface IDataConnectionInterceptor : IDataInterceptor
{
    /// <summary>
    /// Called immediately after a database connection is opened.
    /// </summary>
    /// <param name="connection">The <see cref="DbConnection"/> that was just opened.</param>
    /// <param name="session">The <see cref="IDataSession"/> that opened the connection.</param>
    void ConnectionOpened(DbConnection connection, IDataSession session);

    /// <summary>
    /// Called immediately after a database connection is opened asynchronously.
    /// </summary>
    /// <param name="connection">The <see cref="DbConnection"/> that was just opened.</param>
    /// <param name="session">The <see cref="IDataSession"/> that opened the connection.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ConnectionOpenedAsync(DbConnection connection, IDataSession session, CancellationToken cancellationToken = default);
}
