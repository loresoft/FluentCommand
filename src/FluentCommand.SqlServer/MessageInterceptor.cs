using System.Data.Common;

using FluentCommand.Extensions;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace FluentCommand;

/// <summary>
/// Captures SQL Server informational messages such as PRINT output.
/// </summary>
/// <seealso cref="IDataConnectionInterceptor" />
public partial class MessageInterceptor : IDataConnectionInterceptor
{
    /// <summary>
    /// The default maximum length of the rendered SQL Server message.
    /// </summary>
    public const int DefaultMaxMessageLength = 1024;

    private const string FullMessagePropertyName = "FullMessage";

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageInterceptor"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public MessageInterceptor(ILogger<MessageInterceptor> logger)
        : this(logger, DefaultMaxMessageLength)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageInterceptor"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="maxMessageLength">The maximum length of the rendered SQL Server message.</param>
    public MessageInterceptor(ILogger<MessageInterceptor> logger, int maxMessageLength)
    {
        if (maxMessageLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxMessageLength), maxMessageLength, "The maximum message length must be greater than zero.");

        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        MaxMessageLength = maxMessageLength;
    }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets the maximum length of the rendered SQL Server message.
    /// </summary>
    protected int MaxMessageLength { get; }

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

        var fullMessage = e.Message ?? string.Empty;
        var message = fullMessage.Truncate(MaxMessageLength);

        var scope = new[] { new KeyValuePair<string, object?>(FullMessagePropertyName, fullMessage) };

        using (Logger.BeginScope(scope))
            LogInfoMessage(Logger, message);
    }

    [LoggerMessage(0, LogLevel.Information, "SQL Server Message: {Message}")]
    private static partial void LogInfoMessage(ILogger logger, string message);
}
