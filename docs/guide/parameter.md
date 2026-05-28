# Parameters

Parameters are added to an `IDataCommand` before the command is executed. FluentCommand creates provider-specific `DbParameter` instances from the active connection or command and assigns the parameter name, value, `DbType`, direction, and size.

Use parameters for SQL text and stored procedures instead of string interpolation.

## SQL parameters

Add input parameters with `Parameter(name, value)`.

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

var email = "kara.thrace@battlestar.com";

var user = await session
    .Sql("select * from [User] where [EmailAddress] = @EmailAddress")
    .Parameter("@EmailAddress", email)
    .QuerySingleAsync<User>();
```

Parameter names should match the provider syntax used in the command text. SQL Server uses names such as `@EmailAddress`; other providers may use different conventions.

## Null values

When the value is `null`, specify the generic parameter type so FluentCommand can choose the correct `DbType`.

```csharp
await session
    .Sql("update [User] set [InviteHash] = @InviteHash where [Id] = @Id")
    .Parameter("@Id", userId)
    .Parameter<string>("@InviteHash", null)
    .ExecuteAsync();
```

You can also pass the type explicitly when the runtime value is not enough.

```csharp
session
    .Sql("insert into [Audit] ([Id], [Created]) values (@Id, @Created)")
    .Parameter("@Id", id)
    .Parameter("@Created", null, typeof(DateTimeOffset))
    .Execute();
```

Null values are sent as `DBNull.Value`.

## Conditional parameters

Use `ParameterIf` when a parameter should only be added for a specific condition.

```csharp
var users = await session
    .StoredProcedure("[dbo].[UserSearch]")
    .ParameterIf("@EmailAddress", email, (_, value) => !string.IsNullOrWhiteSpace(value))
    .QueryAsync<User>();
```

If the condition returns `false`, the parameter is not added to the command. Use this for stored procedures with optional parameters or for SQL text that you build to match the parameters you add.

## Configure parameter metadata

Use the fluent `IDataParameter<T>` overload when you need to set the `DbType`, size, direction, or output callback.

```csharp
var users = session
    .StoredProcedure("[dbo].[UserListByEmailAddress]")
    .Parameter("@EmailAddress", "%@battlestar.com")
    .Parameter("@Offset", 0)
    .Parameter("@Size", 10)
    .Parameter<long>(parameter => parameter
        .Name("@Total")
        .Type(DbType.Int64)
        .Size(8)
        .Direction(ParameterDirection.Output)
        .Output(value => total = value ?? -1)
    )
    .Query<User>()
    .ToList();
```

The fluent parameter API supports:

- `Name` to set the parameter name.
- `Value` to set the parameter value.
- `Type` to set the `DbType`.
- `Size` to set the maximum parameter size.
- `Direction` to set `Input`, `Output`, `InputOutput`, or `ReturnValue`.
- `Output` to register an output callback.
- `Return` to register a return-value callback.

## Output parameters

Use `ParameterOut<T>` for output parameters. FluentCommand invokes the callback after the command has executed.

```csharp
long total = -1;

var users = session
    .StoredProcedure("[dbo].[UserListByEmailAddress]")
    .Parameter("@EmailAddress", "%@battlestar.com")
    .Parameter("@Offset", 0)
    .Parameter("@Size", 10)
    .ParameterOut<long>("@Total", value => total = value ?? -1)
    .Query<User>()
    .ToList();
```

Output string and binary parameters need a size. `ParameterOut` defaults output parameter size to `-1`.

## Input-output parameters

Pass an initial value to `ParameterOut<T>` to create an `InputOutput` parameter.

```csharp
string normalizedEmail = email;

session
    .StoredProcedure("[dbo].[NormalizeEmailAddress]")
    .ParameterOut<string>("@EmailAddress", email, value => normalizedEmail = value!)
    .Execute();
```

The same behavior is available through the fluent parameter API. Calling `Value` before `Output` makes the parameter bidirectional unless you override the direction explicitly.

```csharp
session
    .StoredProcedure("[dbo].[NormalizeEmailAddress]")
    .Parameter<string>(parameter => parameter
        .Name("@EmailAddress")
        .Value(email)
        .Size(256)
        .Output(value => normalizedEmail = value!)
    )
    .Execute();
```

## Return values

Use `Return<T>` to capture a stored procedure return value.

```csharp
long total = -1;

var result = session
    .StoredProcedure("[dbo].[UserCountByEmailAddress]")
    .Parameter("@EmailAddress", "william.adama@battlestar.com")
    .Return<long>(value => total = value ?? -1)
    .Execute();
```

`Return<T>` adds a `ReturnValue` parameter named `@ReturnValue`.

## Stored procedure example

Parameters can be chained with query methods for stored procedures.

```csharp
int errorCode = -1;

var userId = Guid.NewGuid();
var username = "test." + DateTime.Now.Ticks;
var email = username + "@email.com";

using var session = Services.GetRequiredService<IDataSession>();

var user = session
    .StoredProcedure("[dbo].[UserUpsert]")
    .Parameter("@Id", userId)
    .Parameter("@EmailAddress", email)
    .Parameter("@IsEmailAddressConfirmed", true)
    .Parameter("@DisplayName", "Unit Test")
    .Parameter("@PasswordHash", "T@est" + DateTime.Now.Ticks)
    .Parameter<string>("@ResetHash", null)
    .Parameter<string>("@InviteHash", null)
    .Parameter("@AccessFailedCount", 0)
    .Parameter("@LockoutEnabled", false)
    .Parameter("@IsDeleted", false)
    .Return<int>(value => errorCode = value ?? -1)
    .QuerySingle<User>();
```

## JSON parameters

Use `ParameterJson` to serialize a value with `System.Text.Json` and send it as a string parameter.

```csharp
var metadata = new
{
    Source = "Import",
    Count = 42,
    Processed = DateTimeOffset.UtcNow
};

var result = session
    .Sql("insert into [JsonLog] ([Data]) values (@Data)")
    .ParameterJson("@Data", metadata)
    .Execute();
```

Pass `JsonSerializerOptions` when the database expects a specific JSON shape.

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
};

session
    .Sql("insert into [JsonLog] ([Data]) values (@Data)")
    .ParameterJson("@Data", metadata, options)
    .Execute();
```

Use a source-generated `JsonTypeInfo<T>` when you want source-generated serialization metadata.

```csharp
var importMetadata = new ImportMetadata
{
    Source = "Import",
    Count = 42
};

session
    .Sql("insert into [JsonLog] ([Data]) values (@Data)")
    .ParameterJson("@Data", importMetadata, ImportJsonContext.Default.ImportMetadata)
    .Execute();
```

When the JSON value is `null`, `ParameterJson` sends a null string parameter.

## Existing DbParameter instances

Use `Parameter(DbParameter)` when provider-specific settings are needed.

```csharp
var parameter = new SqlParameter("@EmailAddress", SqlDbType.NVarChar, 256)
{
    Value = email
};

var user = session
    .Sql("select * from [User] where [EmailAddress] = @EmailAddress")
    .Parameter(parameter)
    .QuerySingle<User>();
```

You can add multiple provider parameters at once.

```csharp
var parameters = new[]
{
    new SqlParameter("@Offset", SqlDbType.Int) { Value = 0 },
    new SqlParameter("@Size", SqlDbType.Int) { Value = 10 }
};

var users = session
    .StoredProcedure("[dbo].[UserPage]")
    .Parameter(parameters)
    .Query<User>()
    .ToList();
```

## Custom parameter handlers

FluentCommand uses `DataParameterHandlers` to set `DbType` and provider values for special types. Built-in handlers include `ConcurrencyToken`, `DateOnly`, and `TimeOnly` when supported by the target framework.

Register an `IDataParameterHandler` for custom value types.

```csharp
DataParameterHandlers.AddTypeHandler(new MoneyParameterHandler());
```

```csharp
public readonly record struct Money(decimal Amount);

public sealed class MoneyParameterHandler : IDataParameterHandler
{
    public Type ValueType => typeof(Money);

    public object? ReadValue(IDbDataParameter parameter)
    {
        return parameter.Value is DBNull ? null : new Money((decimal)parameter.Value);
    }

    public void SetValue(IDbDataParameter parameter, object? value)
    {
        parameter.DbType = DbType.Decimal;
        parameter.Value = value is Money money ? money.Amount : DBNull.Value;
    }
}
```

After registration, normal `Parameter` calls use the handler.

```csharp
session
    .Sql("insert into [Invoice] ([Total]) values (@Total)")
    .Parameter("@Total", new Money(42.50m))
    .Execute();
```

## Caching note

Commands with `Output`, `InputOutput`, or `ReturnValue` parameters cannot be cached. Use `UseCache` only with input parameters.
