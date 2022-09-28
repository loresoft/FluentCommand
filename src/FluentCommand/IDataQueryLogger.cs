using System;
using System.Data;

namespace FluentCommand;

/// <summary>
/// An interface for logging queries
/// </summary>
public interface IDataQueryLogger
{
    /// <summary>
    /// Log the current specified <paramref name="command" />
    /// </summary>
    /// <param name="command">The command to log.</param>
    /// <param name="duration">The execution duration.</param>
    /// <param name="exception">The exception thrown when executing the command.</param>
    /// <exception cref="System.NotImplementedException"></exception>
    void LogCommand(IDbCommand command, TimeSpan duration, Exception exception = null);
}
