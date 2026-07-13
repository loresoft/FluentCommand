using System.Data;
using System.Text;

using FluentCommand.Extensions;
using FluentCommand.Internal;

using Microsoft.Extensions.Logging;

namespace FluentCommand;

/// <summary>
/// A class for logging queries
/// </summary>
/// <seealso cref="FluentCommand.IDataQueryLogger" />
public partial class DataQueryLogger : IDataQueryLogger
{
    /// <summary>
    /// The default maximum formatted query length to write to the log.
    /// </summary>
    public const int DefaultMaxQueryLength = 32 * 1024;

    /// <summary>
    /// The default maximum parameter value length to write to the log.
    /// </summary>
    public const int DefaultMaxParameterLength = 1024;


    /// <summary>
    /// Initializes a new instance of the <see cref="DataQueryLogger"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public DataQueryLogger(ILogger<DataQueryLogger> logger)
        : this(logger, DefaultMaxQueryLength, DefaultMaxParameterLength)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataQueryLogger"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="maxQueryLength">The maximum formatted query length to write to the log.</param>
    /// <param name="maxParameterLength">The maximum parameter value length to write to the log.</param>
    public DataQueryLogger(ILogger<DataQueryLogger> logger, int maxQueryLength, int maxParameterLength)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxQueryLength, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxParameterLength, 1);

        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        MaxQueryLength = maxQueryLength;
        MaxParameterLength = maxParameterLength;
    }


    /// <summary>
    /// Gets the logger used to write command log messages.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets the maximum formatted query length to write to the log.
    /// </summary>
    protected int MaxQueryLength { get; }

    /// <summary>
    /// Gets the maximum parameter value length to write to the log.
    /// </summary>
    protected int MaxParameterLength { get; }


    /// <summary>
    /// Log the current specified <paramref name="command" />
    /// </summary>
    /// <param name="command">The command to log.</param>
    /// <param name="duration">The execution duration.</param>
    /// <param name="exception">The exception thrown when executing the command.</param>
    /// <param name="state">The state used to control logging.</param>
    /// <exception cref="System.ArgumentNullException">command</exception>
    public virtual void LogCommand(IDbCommand command, TimeSpan duration, Exception? exception = null, object? state = null)
    {
        ArgumentNullException.ThrowIfNull(command);

        var logLevel = exception == null ? LogLevel.Debug : LogLevel.Error;
        if (!Logger.IsEnabled(logLevel))
            return;

        var elapsed = duration.TotalMilliseconds;
        var commandType = command.CommandType;
        var commandTimeout = command.CommandTimeout;
        var commandText = FormatCommand(command);

        if (exception == null)
            LogCommand(Logger, elapsed, commandType, commandTimeout, commandText);
        else
            LogError(Logger, elapsed, commandType, commandTimeout, commandText, exception);
    }


    private string FormatCommand(IDbCommand command)
    {
        var commandText = command.CommandText ?? string.Empty;

        // get start and end of the trimmed command text
        GetTrimmedRange(commandText, out var commandTextStart, out var commandTextLength);

        // don't log really long queries
        if (commandTextLength > MaxQueryLength || commandTextLength == 0)
            return string.Empty;

        var builder = StringBuilderCache.Acquire(commandTextLength + Environment.NewLine.Length);

        // new line to wrap from existing log message
        builder.AppendLine();
        builder.Append(commandText, commandTextStart, commandTextLength);

        AppendParameters(builder, command);

        return StringBuilderCache.ToString(builder);
    }

    private void AppendParameters(StringBuilder builder, IDbCommand command)
    {
        if (command.Parameters == null || command.Parameters.Count == 0)
            return;

        foreach (IDataParameter parameter in command.Parameters)
        {
            int precision = 0;
            int scale = 0;
            int size = 0;

            if (parameter is IDbDataParameter dataParameter)
            {
                precision = dataParameter.Precision;
                scale = dataParameter.Scale;
                size = dataParameter.Size;
            }

            if (builder.Length > 0)
                builder.AppendLine();

            builder
                .Append("-- ")
                .Append(parameter.ParameterName)
                .Append(": ")
                .Append(parameter.Direction)
                .Append(' ')
                .Append(parameter.DbType)
                .Append("(Size=")
                .Append(size)
                .Append("; Precision=")
                .Append(precision)
                .Append("; Scale=")
                .Append(scale)
                .Append(") [")
                .AppendTruncated(parameter.Value, MaxParameterLength)
                .Append(']');
        }
    }


    private static void GetTrimmedRange(string text, out int startIndex, out int length)
    {
        var endIndex = text.Length;
        startIndex = 0;

        while (startIndex < endIndex && IsLineBreak(text[startIndex]))
            startIndex++;

        while (endIndex > startIndex && IsLineBreak(text[endIndex - 1]))
            endIndex--;

        length = endIndex - startIndex;
    }

    private static bool IsLineBreak(char value) => value == '\r' || value == '\n';


    [LoggerMessage(0, LogLevel.Debug, "Executed DbCommand ({Elapsed} ms) [CommandType='{CommandType}', CommandTimeout='{CommandTimeout}']{CommandText}")]
    private static partial void LogCommand(ILogger logger, double elapsed, CommandType commandType, int commandTimeout, string commandText);

    [LoggerMessage(1, LogLevel.Error, "Error Executing DbCommand ({Elapsed} ms) [CommandType='{CommandType}', CommandTimeout='{CommandTimeout}']{CommandText}")]
    private static partial void LogError(ILogger logger, double elapsed, CommandType commandType, int commandTimeout, string commandText, Exception exception);
}
