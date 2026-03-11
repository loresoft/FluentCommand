using System.Data.Common;

namespace FluentCommand;

/// <summary>
/// An interface for intercepting database command execution events.
/// </summary>
/// <remarks>
/// Use this interceptor to inspect or modify a <see cref="DbCommand"/> before it executes,
/// for example to inject additional parameters or rewrite command text.
/// </remarks>
public interface IDataCommandInterceptor : IDataInterceptor
{
    /// <summary>
    /// Called immediately before a database command is executed.
    /// </summary>
    /// <param name="command">The <see cref="DbCommand"/> that is about to execute.</param>
    /// <param name="session">The <see cref="IDataSession"/> executing the command.</param>
    void CommandExecuting(DbCommand command, IDataSession session);

    /// <summary>
    /// Called immediately before a database command is executed asynchronously.
    /// </summary>
    /// <param name="command">The <see cref="DbCommand"/> that is about to execute.</param>
    /// <param name="session">The <see cref="IDataSession"/> executing the command.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CommandExecutingAsync(DbCommand command, IDataSession session, CancellationToken cancellationToken = default);
}
