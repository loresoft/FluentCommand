using System;
using System.Data;

using FluentCommand.Extensions;

namespace FluentCommand;

/// <summary>
/// A class to log queries to string delegate
/// </summary>
/// <seealso cref="FluentCommand.IDataQueryLogger" />
public class DataQueryLogger : IDataQueryLogger
{
    private readonly Action<string> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataQueryLogger"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public DataQueryLogger(Action<string> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Log the current specified <paramref name="command" />
    /// </summary>
    /// <param name="command">The command to log.</param>
    /// <param name="duration">The execution duration.</param>
    /// <param name="exception">The exception thrown when executing the command.</param>
    /// <exception cref="System.NotImplementedException"></exception>
    public void LogCommand(IDbCommand command, TimeSpan duration, Exception exception = null)
    {
        if (_logger == null)
            return;

        if (command is null)
            throw new ArgumentNullException(nameof(command));
                
        var elapsed = duration.TotalMilliseconds;
        var commandType = command.CommandType;
        var commandTimeout = command.CommandTimeout;
        var resultText = exception == null ? "Executed" : "Error Executing";

        var buffer = StringBuilderCache.Acquire();
        buffer
            .AppendLine($"{resultText} DbCommand ({elapsed}ms) [CommandType='{commandType}', CommandTimeout='{commandTimeout}']")
            .AppendLine(command.CommandText);

        const string parameterFormat = "-- {0}: {1} {2} (Size = {3}; Precision = {4}; Scale = {5}) [{6}]";
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

            buffer.AppendFormat(parameterFormat,
                parameter.ParameterName,
                parameter.Direction,
                parameter.DbType,
                size,
                precision,
                scale,
                parameter.Value);

            buffer.AppendLine();
        }

        var output = StringBuilderCache.ToString(buffer);
        _logger(output);
    }
}
