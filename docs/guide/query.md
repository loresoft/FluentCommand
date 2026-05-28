# Query Methods

Query methods execute an `IDataCommand` and choose how the result is consumed. Use `Execute` for commands that do not return rows, `Query` and `QuerySingle` for row materialization, `QueryValue` for scalar values, and export helpers when the result should be written as JSON or CSV.

Most methods have synchronous and asynchronous forms. Async methods accept an optional `CancellationToken`.

## Execute commands

Use `Execute` or `ExecuteAsync` for commands that do not return a result set. The return value is the number of rows affected by the database provider.

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

var affected = await session
    .Sql("update [User] set [LastLogin] = @LastLogin where [Id] = @Id")
    .Parameter("@Id", userId)
    .Parameter("@LastLogin", DateTimeOffset.UtcNow)
    .ExecuteAsync(cancellationToken);
```

Stored procedure output and return parameters are populated after execution.

```csharp
long total = -1;

var result = session
    .StoredProcedure("[dbo].[UserCountByEmailAddress]")
    .Parameter("@EmailAddress", "william.adama@battlestar.com")
    .Return<long>(value => total = value ?? -1)
    .Execute();
```

## Query entities

Use `Query<TEntity>` or `QueryAsync<TEntity>` to materialize every row into an entity.

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

var users = await session
    .StoredProcedure("[dbo].[UserListByEmailAddress]")
    .Parameter("@EmailAddress", "%@battlestar.com")
    .Parameter("@Offset", 0)
    .Parameter("@Size", 10)
    .QueryAsync<User>(cancellationToken);
```

When a source-generated reader exists for `TEntity`, `QueryAsync<TEntity>()` uses the generated factory. See the source generator guide for generation setup and mapping rules.

Use `Query<TEntity>(factory)` when you want to map rows manually.

```csharp
var users = session
    .Sql("select * from [User] where [EmailAddress] like @EmailAddress")
    .Parameter("@EmailAddress", "%@battlestar.com")
    .Query(reader => new User
    {
        Id = reader.GetGuid("Id"),
        EmailAddress = reader.GetString("EmailAddress"),
        DisplayName = reader.GetString("DisplayName"),
        IsDeleted = reader.GetBoolean("IsDeleted"),
        Created = reader.GetDateTimeOffset("Created"),
        Updated = reader.GetDateTimeOffset("Updated")
    })
    .ToList();
```

## Query a single row

Use `QuerySingle<TEntity>` or `QuerySingleAsync<TEntity>` when only the first row matters. If the result set is empty, the method returns `null` for reference and nullable result types.

```csharp
var user = await session
    .Sql("select * from [User] where [EmailAddress] = @EmailAddress")
    .Parameter("@EmailAddress", "kara.thrace@battlestar.com")
    .QuerySingleAsync<User>(cancellationToken);
```

Manual mapping works the same way as list queries.

```csharp
var user = session
    .Sql("select * from [User] where [Id] = @Id")
    .Parameter("@Id", userId)
    .QuerySingle(reader => new User
    {
        Id = reader.GetGuid("Id"),
        EmailAddress = reader.GetString("EmailAddress"),
        DisplayName = reader.GetString("DisplayName"),
        IsDeleted = reader.GetBoolean("IsDeleted"),
        Created = reader.GetDateTimeOffset("Created"),
        Updated = reader.GetDateTimeOffset("Updated")
    });
```

## Query dynamic rows

Use `Query()` or `QuerySingle()` without a type argument to return dynamic rows. This is useful for ad-hoc projections.

```csharp
var rows = session
    .Sql("select [Id], [EmailAddress], [DisplayName] from [User]")
    .Query()
    .ToList();

foreach (var row in rows)
{
    Console.WriteLine($"{row.Id}: {row.EmailAddress}");
}
```

## Query scalar values

Use `QueryValue<T>` or `QueryValueAsync<T>` for the first column of the first row. Other columns and rows are ignored.

```csharp
var count = await session
    .Sql(builder => builder
        .Select<User>()
        .Count()
        .Where(p => p.EmailAddress, "@battlestar.com", FilterOperators.Contains)
    )
    .QueryValueAsync<int>(cancellationToken);
```

Use the converter overload when a provider returns a value that needs custom conversion.

```csharp
var id = session
    .Sql("select [Id] from [User] where [EmailAddress] = @EmailAddress")
    .Parameter("@EmailAddress", email)
    .QueryValue(value => value is Guid guid ? guid : Guid.Parse(value!.ToString()!));
```

## Query scalar lists

Use `QueryValues<T>` or `QueryValuesAsync<T>` for the first column of every row.

```csharp
var ids = await session
    .Sql("select [Id] from [Status] order by [DisplayOrder]")
    .QueryValuesAsync<int>(cancellationToken);
```

## Query multiple result sets

Use `QueryMultiple` or `QueryMultipleAsync` when one command returns more than one result set. The callback receives an open query object that advances as each result is read.

```csharp
var sql = """
    select * from [User] where [EmailAddress] = @EmailAddress;
    select * from [Role];
    select * from [Priority];
    """;

User? user = null;
List<Role> roles = [];
List<Priority> priorities = [];

await session
    .Sql(sql)
    .Parameter("@EmailAddress", "kara.thrace@battlestar.com")
    .QueryMultipleAsync(async query =>
    {
        user = await query.QuerySingleAsync<User>(cancellationToken);
        roles = (await query.QueryAsync<Role>(cancellationToken)).ToList();
        priorities = (await query.QueryAsync<Priority>(cancellationToken)).ToList();
    }, cancellationToken);
```

Read the result sets in the same order the SQL command returns them.

## Query a DataTable

Use `QueryTable` or `QueryTableAsync` when a `DataTable` is the most convenient result format.

```csharp
var table = await session
    .Sql("select * from [User] where [IsDeleted] = @IsDeleted")
    .Parameter("@IsDeleted", false)
    .QueryTableAsync(cancellationToken);
```

## Read with IDataReader

Use `Read` or `ReadAsync` for complete control over the open `IDataReader`.

```csharp
var emails = new List<string>();

await session
    .Sql("select [EmailAddress] from [User] order by [EmailAddress]")
    .ReadAsync(async (reader, token) =>
    {
        while (await ((DbDataReader)reader).ReadAsync(token))
            emails.Add(reader.GetString("EmailAddress"));
    }, cancellationToken: cancellationToken);
```

The reader is disposed after the callback completes.

## Export JSON

Use `QueryJson` or `QueryJsonAsync` to execute a command and serialize the result set as a JSON array.

```csharp
var json = await session
    .Sql(builder => builder
        .Select<Status>()
        .OrderBy(p => p.DisplayOrder)
        .Limit(0, 1000)
    )
    .QueryJsonAsync(cancellationToken: cancellationToken);
```

Write JSON directly to a stream for large exports.

```csharp
await using var stream = File.Create("status.json");

await session
    .Sql(builder => builder
        .Select<Status>()
        .OrderBy(p => p.DisplayOrder)
    )
    .QueryJsonAsync(stream, cancellationToken: cancellationToken);
```

Use `JsonWriterOptions` to control JSON writer behavior.

```csharp
var options = new JsonWriterOptions { Indented = true };

var json = session
    .Sql("select [Id], [Name] from [Status]")
    .QueryJson(options);
```

## Export CSV

Use `QueryCsv` or `QueryCsvAsync` to execute a command and serialize the result set as CSV.

```csharp
var csv = await session
    .Sql(builder => builder
        .Select<Status>()
        .OrderBy(p => p.DisplayOrder)
        .Limit(0, 1000)
    )
    .QueryCsvAsync(cancellationToken);
```

Write CSV directly to a stream for large exports.

```csharp
await using var stream = File.Create("status.csv");

await session
    .Sql(builder => builder
        .Select<Status>()
        .OrderBy(p => p.DisplayOrder)
    )
    .QueryCsvAsync(stream, cancellationToken);
```

CSV output includes a header row and uses UTF-8 when writing to a stream.

## Command behavior

Factory-based query methods accept an optional `CommandBehavior` argument when you need to override the default reader behavior.

```csharp
var users = session
    .Sql("select * from [User]")
    .Query(
        reader => new User
        {
            Id = reader.GetGuid("Id"),
            EmailAddress = reader.GetString("EmailAddress")
        },
        CommandBehavior.SequentialAccess | CommandBehavior.SingleResult);
```

Generated entity queries use `SingleResult` and single-row generated queries also use `SingleRow`.

## Caching note

`Query`, `QuerySingle`, `QueryValue`, `QueryValues`, and `QueryTable` can use command caching when an `IDataCache` is configured and the command opts in with `UseCache`. `Execute`, `Read`, `QueryMultiple`, `QueryJson`, and `QueryCsv` are not cached.
