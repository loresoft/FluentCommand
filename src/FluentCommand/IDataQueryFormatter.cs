using System.Data;

namespace FluentCommand;

/// <summary>
/// A interface for formatting an <see cref="IDbCommand"/> for logging
/// </summary>
public interface IDataQueryFormatter
{
    /// <summary>
    /// Formats the command for logging.
    /// </summary>
    /// <param name="command">The command to log.</param>
    /// <param name="duration">The execution duration.</param>
    /// <param name="exception">The exception thrown when executing the command.</param>
    /// <returns>The command formatted as a string</returns>
    string FormatCommand(IDbCommand command, TimeSpan duration, Exception exception);
}
