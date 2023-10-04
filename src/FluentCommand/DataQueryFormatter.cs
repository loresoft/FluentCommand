using System.Data;

using FluentCommand.Internal;

namespace FluentCommand;

/// <summary>
/// A class to format an <see cref="IDbCommand"/> for logging
/// </summary>
public class DataQueryFormatter
    : IDataQueryFormatter
{
    /// <summary>
    /// Formats the command for logging.
    /// </summary>
    /// <param name="command">The command to log.</param>
    /// <param name="duration">The execution duration.</param>
    /// <param name="exception">The exception thrown when executing the command.</param>
    /// <returns>The command formatted as a string</returns>
    /// <exception cref="System.ArgumentNullException">command</exception>
    public string FormatCommand(IDbCommand command, TimeSpan duration, Exception exception)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        var elapsed = duration.TotalMilliseconds;
        var commandType = command.CommandType;
        var commandTimeout = command.CommandTimeout;
        var resultText = exception == null ? "Executed" : "Error Executing";

        var buffer = StringBuilderCache.Acquire();

        buffer
            .Append(resultText)
            .Append(" DbCommand (")
            .Append(elapsed)
            .Append("ms) [CommandType='")
            .Append(commandType)
            .Append("', CommandTimeout='")
            .Append(commandTimeout)
            .Append("']")
            .AppendLine()
            .AppendLine(command.CommandText);

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

            buffer
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
                .Append("]")
                .AppendLine();
        }

        return StringBuilderCache.ToString(buffer);
    }

}
