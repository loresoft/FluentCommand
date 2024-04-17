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
    /// Initializes a new instance of the <see cref="DataQueryLogger"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public DataQueryLogger(ILogger<DataQueryLogger> logger)
    {
        Logger = logger;
    }

    protected ILogger Logger { get; }

    /// <summary>
    /// Log the current specified <paramref name="command" />
    /// </summary>
    /// <param name="command">The command to log.</param>
    /// <param name="duration">The execution duration.</param>
    /// <param name="exception">The exception thrown when executing the command.</param>
    /// <param name="state">The state used to control logging.</param>
    /// <exception cref="System.ArgumentNullException">command</exception>
    public virtual void LogCommand(IDbCommand command, TimeSpan duration, Exception exception = null, object state = null)
    {
        if (Logger == null)
            return;

        if (command is null)
            throw new ArgumentNullException(nameof(command));

        var elapsed = duration.TotalMilliseconds;
        var commandType = command.CommandType;
        var commandTimeout = command.CommandTimeout;
        var commandText = command.CommandText;
        var parameterText = FormatParameters(command);

        if (exception == null)
            LogCommand(Logger, elapsed, commandType, commandTimeout, commandText, parameterText);
        else
            LogError(Logger, elapsed, commandType, commandTimeout, commandText, parameterText, exception);
    }

    protected static string FormatParameters(IDbCommand command)
    {
        if (command is null || command.Parameters == null || command.Parameters.Count == 0)
            return string.Empty;

        var parameterText = StringBuilderCache.Acquire();

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

            parameterText
                .AppendLineIf(() => parameterText.Length > 0)
                .Append("-- ")
                .Append(parameter.ParameterName)
                .Append(": ")
                .Append(parameter.Direction)
                .Append(" ")
                .Append(parameter.DbType)
                .Append("(Size=")
                .Append(size)
                .Append("; Precision=")
                .Append(precision)
                .Append("; Scale=")
                .Append(scale)
                .Append(") [")
                .Append(parameter.Value)
                .Append("]");
        }

        return parameterText.ToString();
    }

    [LoggerMessage(0, LogLevel.Debug, "Executed DbCommand ({Elapsed} ms) [CommandType='{CommandType}', CommandTimeout='{CommandTimeout}']\r\n{CommandText}\r\n{ParameterText}")]
    protected static partial void LogCommand(ILogger logger, double elapsed, CommandType commandType, int commandTimeout, string commandText, string parameterText);

    [LoggerMessage(1, LogLevel.Error, "Error Executing DbCommand ({Elapsed} ms) [CommandType='{CommandType}', CommandTimeout='{CommandTimeout}']\r\n{CommandText}\r\n{ParameterText}")]
    protected static partial void LogError(ILogger logger, double elapsed, CommandType commandType, int commandTimeout, string commandText, string parameterText, Exception exception);
}
