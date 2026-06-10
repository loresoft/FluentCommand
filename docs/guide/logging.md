# Logging

FluentCommand logs executed commands through `IDataQueryLogger`. A query logger receives the underlying `IDbCommand`, the elapsed duration, any exception thrown during execution, and optional command state.

When using dependency injection, `DataQueryLogger` is registered as the default `IDataQueryLogger`. It writes command details through `Microsoft.Extensions.Logging` at `Debug` level for successful commands and `Error` level for failed commands.

## Configure query logging

Configure logging with the standard .NET logging services.

```csharp
services.AddLogging(builder => builder
    .AddConsole()
    .SetMinimumLevel(LogLevel.Debug)
);

services.AddFluentCommand(builder => builder
    .UseConnectionString(ConnectionString)
    .UseSqlServer()
);
```

`DataQueryLogger` logs the command type, command timeout, command text, elapsed time, and parameters.

```text
Executed DbCommand (12.3 ms) [CommandType='Text', CommandTimeout='30']
select * from [User] where [EmailAddress] = @EmailAddress
-- @EmailAddress: Input String(Size=0; Precision=0; Scale=0) [kara.thrace@battlestar.com]
```

## Configure a custom query logger

Implement `IDataQueryLogger` when you need to redact values, send command telemetry to another system, or change the log format.

```csharp
public sealed class RedactingQueryLogger : IDataQueryLogger
{
    private readonly ILogger<RedactingQueryLogger> _logger;

    public RedactingQueryLogger(ILogger<RedactingQueryLogger> logger)
    {
        _logger = logger;
    }

    public void LogCommand(IDbCommand command, TimeSpan duration, Exception? exception = null, object? state = null)
    {
        if (exception is null)
        {
            _logger.LogDebug(
                "Executed {CommandType} command in {ElapsedMilliseconds} ms",
                command.CommandType,
                duration.TotalMilliseconds);
        }
        else
        {
            _logger.LogError(
                exception,
                "Error executing {CommandType} command after {ElapsedMilliseconds} ms",
                command.CommandType,
                duration.TotalMilliseconds);
        }
    }
}
```

Register the custom logger with `AddQueryLogger`.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(ConnectionString)
    .UseSqlServer()
    .AddQueryLogger<RedactingQueryLogger>()
);
```

You can also provide an instance or factory.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(ConnectionString)
    .UseSqlServer()
    .AddQueryLogger(sp => new RedactingQueryLogger(sp.GetRequiredService<ILogger<RedactingQueryLogger>>()))
);
```

When creating `DataConfiguration` directly, pass the logger to the constructor. The configured logger is exposed through `QueryLogger` and passed to sessions created from the configuration.

```csharp
var dataConfiguration = new DataConfiguration(
    SqlClientFactory.Instance,
    ConnectionString,
    queryLogger: new RedactingQueryLogger(logger)
);

var queryLogger = dataConfiguration.QueryLogger;
```

## Pass log state

Use `LogState` to pass per-command state into `IDataQueryLogger.LogCommand`. This is useful for correlation IDs, redaction policy, or workflow names.

```csharp
var state = new
{
    CorrelationId = correlationId,
    Operation = "UserLookup"
};

var user = session
    .Sql("select * from [User] where [EmailAddress] = @EmailAddress")
    .Parameter("@EmailAddress", email)
    .LogState(state)
    .QuerySingle<User>();
```

The same state object is passed to the query logger for both successful and failed command execution.

```csharp
public void LogCommand(IDbCommand command, TimeSpan duration, Exception? exception = null, object? state = null)
{
    if (state is not null)
        _logger.LogDebug("Command state: {State}", state);
}
```

## Capture SQL Server PRINT output

SQL Server sends `PRINT` output and other informational messages through the connection's `InfoMessage` event. Use `MessageInterceptor` to capture those messages and write them through `ILogger<MessageInterceptor>`.

Enable message capture with `CaptureMessages` when configuring SQL Server.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(ConnectionString)
    .UseSqlServer()
    .CaptureMessages()
);
```

`MessageInterceptor` is an `IDataConnectionInterceptor`. When FluentCommand opens a `SqlConnection`, the interceptor subscribes to `SqlConnection.InfoMessage`; when FluentCommand closes the connection, it unsubscribes.

```csharp
using var session = Services.GetRequiredService<IDataSession>();

var result = session
    .Sql("PRINT 'starting user query'; SELECT COUNT(*) FROM [User]")
    .QueryValue<int>();
```

The `PRINT` output is logged as an informational message.

```text
SQL Server message: starting user query
```

Rendered SQL Server messages are limited to `1024` characters by default so very large `InfoMessage` output does not produce oversized log entries. The original untrimmed message is included in the logging scope as `FullMessage` for logging providers configured to capture scopes.

Configure the rendered message length by passing the maximum length to `CaptureMessages`.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(ConnectionString)
    .UseSqlServer()
    .CaptureMessages(maxMessageLength: 256)
);
```

Register `MessageInterceptor` manually if you are not using `CaptureMessages`.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(ConnectionString)
    .UseSqlServer()
    .AddInterceptor<MessageInterceptor>()
);
```

Message capture only applies to SQL Server connections. Like other connection interceptors, it runs when FluentCommand opens the connection for the session.
