using System.Data.Common;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace FluentCommand;

/// <summary>
/// Captures SQL Server informational messages such as PRINT output.
/// </summary>
/// <seealso cref="IDataConnectionInterceptor" />
public class MessageInterceptor : IDataConnectionInterceptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageInterceptor"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public MessageInterceptor(ILogger<MessageInterceptor> logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    protected ILogger Logger { get; }

    /// <inheritdoc />
    public virtual void ConnectionOpened(DbConnection connection, IDataSession session)
    {
        if (connection is SqlConnection sqlConnection)
            sqlConnection.InfoMessage += OnInfoMessage;
    }

    /// <inheritdoc />
    public virtual Task ConnectionOpenedAsync(DbConnection connection, IDataSession session, CancellationToken cancellationToken = default)
    {
        ConnectionOpened(connection, session);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual void ConnectionClosing(DbConnection connection, IDataSession session)
    {
        if (connection is SqlConnection sqlConnection)
            sqlConnection.InfoMessage -= OnInfoMessage;
    }

    /// <inheritdoc />
    public virtual Task ConnectionClosingAsync(DbConnection connection, IDataSession session, CancellationToken cancellationToken = default)
    {
        ConnectionClosing(connection, session);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles SQL Server informational messages.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The SQL Server message event data.</param>
    protected virtual void OnInfoMessage(object sender, SqlInfoMessageEventArgs e)
    {
        if (e is null)
            return;

        Logger.LogInformation("SQL Server message: {Message}", e.Message);
    }
}
