using System.Data;

using FluentCommand.Internal;

using Microsoft.Extensions.Logging;

namespace FluentCommand;

/// <summary>
/// A class to log queries to string delegate
/// </summary>
/// <seealso cref="FluentCommand.IDataQueryLogger" />
public partial class DataQueryLogger : IDataQueryLogger
{
    private readonly ILogger<DataQueryLogger> _logger;
    private readonly IDataQueryFormatter _formatter;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataQueryLogger"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="formatter">The formatter for the data command</param>
    public DataQueryLogger(ILogger<DataQueryLogger> logger, IDataQueryFormatter formatter)
    {
        _logger = logger;
        _formatter = formatter;
    }

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
        if (_logger == null)
            return;

        if (command is null)
            throw new ArgumentNullException(nameof(command));

        var output = _formatter.FormatCommand(command, duration, exception);

        if (exception == null)
            LogCommand(output);
        else
            LogError(output, exception);
    }

    [LoggerMessage(0, LogLevel.Debug, "{output}")]
    public partial void LogCommand(string output);

    [LoggerMessage(1, LogLevel.Error, "{output}")]
    public partial void LogError(string output, Exception exception);
}
