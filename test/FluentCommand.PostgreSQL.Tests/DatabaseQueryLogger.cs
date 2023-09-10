using System;
using System.Data;

using Microsoft.Extensions.Logging;

namespace FluentCommand.PostgreSQL.Tests;

public class DatabaseQueryLogger : DataQueryLogger
{
    private readonly ILogger<DatabaseQueryLogger> _logger;

    public DatabaseQueryLogger(ILogger<DatabaseQueryLogger> logger)
    {
        _logger = logger;
    }

    public override void LogCommand(IDbCommand command, TimeSpan duration, Exception exception = null, object state = null)
    {
        var message = FormatCommand(command, duration, exception);
        _logger.LogInformation(exception, message);
    }
}
