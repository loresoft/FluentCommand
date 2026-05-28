# Configuration

FluentCommand can be configured directly with `DataConfiguration` or through dependency injection with `AddFluentCommand`. Most applications should use dependency injection so sessions, configuration, provider factories, query generators, loggers, caches, and interceptors are resolved from the same service container.

## Quick start

For SQL Server, reference the SQL Server provider package and use `UseSqlServer()` to register `SqlClientFactory` and the SQL Server query generator.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseSqlServer()
);
```

Then request an `IDataSession` where you need to run commands.

```csharp
public sealed class UserRepository
{
    private readonly IDataSession _session;

    public UserRepository(IDataSession session)
    {
        _session = session;
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _session
            .Sql("select * from [User] where [EmailAddress] = @EmailAddress")
            .Parameter("@EmailAddress", email)
            .QuerySingleAsync<User>(cancellationToken: cancellationToken);
    }
}
```

`AddFluentCommand` registers `IDataConfiguration` as a singleton, `IDataSessionFactory` as transient, and `IDataSession` as transient. Each resolved session owns its database connection and should be disposed when the consuming scope ends.

## Connection strings

Use `UseConnectionString` when you already have the full connection string.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString("Data Source=(local);Initial Catalog=Tracker;Integrated Security=True;TrustServerCertificate=True;")
    .UseSqlServer()
);
```

Use `UseConnectionName` when the value should be resolved from `IConfiguration`. FluentCommand first checks the `ConnectionStrings` section and then checks the root configuration key.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionName("Tracker")
    .UseSqlServer()
);
```

```json
{
  "ConnectionStrings": {
    "Tracker": "Data Source=(local);Initial Catalog=Tracker;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

The string overload is a shortcut for `UseConnectionName`.

```csharp
services.AddFluentCommand("Tracker");
```

When using the shortcut, register the provider factory and query generator elsewhere or prefer the builder overload so the provider setup is explicit.

## Providers and query generators

The database provider factory creates connections. The query generator controls SQL builder syntax such as identifier quoting, paging, output clauses, and upsert SQL.

SQL Server has a convenience method.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionName("Tracker")
    .UseSqlServer()
);
```

PostgreSQL and SQLite can be configured with `AddProviderFactory` and the matching generator.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(postgresConnectionString)
    .AddProviderFactory(NpgsqlFactory.Instance)
    .AddPostgreSqlGenerator()
);
```

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionName("Tracker")
    .AddProviderFactory(SqliteFactory.Instance)
    .AddSqliteGenerator()
);
```

For a custom provider or SQL dialect, register a `DbProviderFactory` and an `IQueryGenerator` implementation.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .AddProviderFactory(MyProviderFactory.Instance)
    .AddQueryGenerator<MyQueryGenerator>()
);
```

## Direct configuration

Use `DataConfiguration` directly when you are not using dependency injection.

```csharp
var configuration = new DataConfiguration(
    SqlClientFactory.Instance,
    connectionString,
    queryGenerator: new SqlServerGenerator()
);

await using var session = configuration.CreateSession();
```

The constructor also accepts a cache, query logger, interceptors, and a default command timeout.

```csharp
var configuration = new DataConfiguration(
    SqlClientFactory.Instance,
    connectionString,
    cache: dataCache,
    queryGenerator: new SqlServerGenerator(),
    queryLogger: new DataQueryLogger(loggerFactory.CreateLogger<DataQueryLogger>()),
    interceptors:
    [
        new CommandAuditInterceptor(logger),
        new SessionContextInterceptor(userContext)
    ],
    commandTimeout: 60
);
```

Create a session from a transaction when several commands should share the same transaction and connection.

```csharp
await using var connection = configuration.CreateConnection();
await connection.OpenAsync(cancellationToken);

await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
await using var session = configuration.CreateSession(transaction);

await session
    .Sql("update [User] set [LastLogin] = @LastLogin where [Id] = @Id")
    .Parameter("@LastLogin", DateTimeOffset.UtcNow)
    .Parameter("@Id", userId)
    .ExecuteAsync(cancellationToken: cancellationToken);

await transaction.CommitAsync(cancellationToken);
```

## Command timeout

Set a default command timeout for commands created by sessions from the configuration.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseCommandTimeout(60)
    .UseSqlServer()
);
```

The timeout can also be configured with a `TimeSpan`.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseCommandTimeout(TimeSpan.FromMinutes(2))
    .UseSqlServer()
);
```

When creating `DataConfiguration` directly, pass `commandTimeout` in seconds.

```csharp
var configuration = new DataConfiguration(
    SqlClientFactory.Instance,
    connectionString,
    commandTimeout: 60
);
```

Override the timeout for a single command with `CommandTimeout`.

```csharp
var users = await session
    .Sql("select * from [User]")
    .CommandTimeout(TimeSpan.FromSeconds(10))
    .QueryAsync<User>();
```

## Query logging

Register an `IDataQueryLogger` to capture formatted command text, parameters, elapsed time, and result state. See the logging guide for full details.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseSqlServer()
    .AddQueryLogger<DataQueryLogger>()
);
```

You can also register a custom logger by type or factory.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseSqlServer()
    .AddQueryLogger(sp => new DataQueryLogger(sp.GetRequiredService<ILogger<DataQueryLogger>>()))
);
```

## Caching

Register an `IDataCache` implementation when commands should be able to opt in to caching with `UseCache`.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseSqlServer()
    .AddDataCache<AppDataCache>()
);
```

In this example, `AppDataCache` is your implementation of `IDataCache`.

The optional `FluentCommand.Caching` package adds distributed-cache integration.

```csharp
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "FluentCommand";
});

services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseSqlServer()
    .AddDistributedDataCache()
);
```

See the caching guide for query-level cache usage and expiration.

## Multiple configurations

Use discriminated registrations when an application needs more than one database configuration. A common case is a primary connection plus a read-only connection.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(primaryConnectionString)
    .UseSqlServer()
);

services.AddFluentCommand<ReadOnlyIntent>(builder => builder
    .UseConnectionString(readOnlyConnectionString)
    .UseSqlServer()
);
```

Request the matching session type for the alternate configuration.

```csharp
public sealed class ReportRepository
{
    private readonly IDataSession<ReadOnlyIntent> _session;

    public ReportRepository(IDataSession<ReadOnlyIntent> session)
    {
        _session = session;
    }
}
```

Use discriminators for multiple configurations instead of registering `IDataConfiguration` more than once. The non-discriminated registration uses `TryAdd`, so the first default configuration wins.

## Interceptors

Interceptors let you run code during connection and command execution. Register interceptors with `AddInterceptor`; each interceptor is registered as a singleton `IDataInterceptor` and is applied to sessions created by the configuration.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseSqlServer()
    .AddInterceptor<CommandAuditInterceptor>()
    .AddInterceptor(sp => new SessionContextInterceptor(sp.GetRequiredService<IUserContext>()))
);
```

When creating `DataConfiguration` directly, pass the interceptors to the constructor.

```csharp
var configuration = new DataConfiguration(
    SqlClientFactory.Instance,
    connectionString,
    interceptors:
    [
        new CommandAuditInterceptor(logger),
        new SessionContextInterceptor(userContext)
    ]
);
```

Use `IDataCommandInterceptor` to inspect or modify the underlying `DbCommand` immediately before execution.

```csharp
public sealed class CommandAuditInterceptor : IDataCommandInterceptor
{
    private readonly ILogger<CommandAuditInterceptor> _logger;

    public CommandAuditInterceptor(ILogger<CommandAuditInterceptor> logger)
    {
        _logger = logger;
    }

    public void CommandExecuting(DbCommand command, IDataSession session)
    {
        _logger.LogDebug(
            "Executing command {CommandType}: {CommandText}",
            command.CommandType,
            command.CommandText);
    }

    public Task CommandExecutingAsync(
        DbCommand command,
        IDataSession session,
        CancellationToken cancellationToken = default)
    {
        CommandExecuting(command, session);
        return Task.CompletedTask;
    }
}
```

Use `IDataConnectionInterceptor` to run code after a connection opens and before FluentCommand closes it.

```csharp
public sealed class SessionContextInterceptor : IDataConnectionInterceptor
{
    private readonly IUserContext _userContext;

    public SessionContextInterceptor(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public void ConnectionOpened(DbConnection connection, IDataSession session)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sys.sp_set_session_context @key=N'UserId', @value=@UserId";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@UserId";
        parameter.Value = _userContext.UserId;
        command.Parameters.Add(parameter);

        command.ExecuteNonQuery();
    }

    public async Task ConnectionOpenedAsync(
        DbConnection connection,
        IDataSession session,
        CancellationToken cancellationToken = default)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "EXEC sys.sp_set_session_context @key=N'UserId', @value=@UserId";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@UserId";
        parameter.Value = _userContext.UserId;
        command.Parameters.Add(parameter);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public void ConnectionClosing(DbConnection connection, IDataSession session)
    {
    }

    public Task ConnectionClosingAsync(
        DbConnection connection,
        IDataSession session,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
```

Connection interceptors run only when FluentCommand opens or closes the connection. If a session is created with an already-open connection or transaction, the connection-opened callback has already happened outside FluentCommand. Command interceptors run once for each executed command, after the connection is available and before the command is executed.

SQL Server message capture is implemented as a connection interceptor. It subscribes to `SqlConnection.InfoMessage`, which captures SQL Server informational messages such as `PRINT` output.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseSqlServer()
    .CaptureMessages()
);

```

## Advanced service registration

Use `AddService` to register services that are needed by factories, custom loggers, caches, query generators, or interceptors while keeping the setup in one FluentCommand block.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseSqlServer()
    .AddService(services => services.AddSingleton<IUserContext, HttpUserContext>())
    .AddInterceptor(sp => new SessionContextInterceptor(sp.GetRequiredService<IUserContext>()))
);
```
