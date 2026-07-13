# FluentCommand

Fluent wrapper for ADO.NET `DbCommand` with automatic object mapping, caching, query building, and source-generated data readers.

[![Build status](https://github.com/loresoft/FluentCommand/workflows/Build/badge.svg)](https://github.com/loresoft/FluentCommand/actions)

[![Coverage Status](https://coveralls.io/repos/github/loresoft/FluentCommand/badge.svg?branch=master)](https://coveralls.io/github/loresoft/FluentCommand?branch=master)

| Package | Version |
| :--- | :--- |
| [FluentCommand](https://www.nuget.org/packages/FluentCommand/) | [![FluentCommand](https://img.shields.io/nuget/v/FluentCommand.svg)](https://www.nuget.org/packages/FluentCommand/) |
| [FluentCommand.SqlServer](https://www.nuget.org/packages/FluentCommand.SqlServer/) | [![FluentCommand.SqlServer](https://img.shields.io/nuget/v/FluentCommand.SqlServer.svg)](https://www.nuget.org/packages/FluentCommand.SqlServer/) |
| [FluentCommand.Caching](https://www.nuget.org/packages/FluentCommand.Caching/) | [![FluentCommand.Caching](https://img.shields.io/nuget/v/FluentCommand.Caching.svg)](https://www.nuget.org/packages/FluentCommand.Caching/) |

## Features

- Fluent wrapper over `DbConnection` and `DbCommand`
- Automatic connection state management
- Source-generated `IDataReader` mapping (no reflection)
- SQL query builder with Select, Insert, Update, Delete, and Upsert support
- JSON column support with `[JsonColumn]` and configurable `JsonSerializerOptions`
- JSON and CSV export directly from query results
- JSON parameter and query-builder value serialization with `ParameterJson` and `ValueJson`
- Parameterized queries with output, input-output, and return value callbacks
- Conditional parameters and query builder filters (`ParameterIf`, `WhereIf`, `ValueIf`)
- Result caching with sliding or absolute expiration
- Distributed cache integration via `FluentCommand.Caching`
- Query logging with elapsed time and parameter details
- Connection and command interceptors
- Multiple result set handling
- Multiple database configuration with discriminated registrations
- SQL Server bulk copy and merge data operations
- Tabular data import with field mapping, validation, and merge
- Multi-target: `netstandard2.0`, `net8.0`, `net9.0`, `net10.0`
- Supports SQL Server, PostgreSQL, and SQLite

## Installation

```shell
dotnet add package FluentCommand
```

For SQL Server bulk copy, merge, and import features:

```shell
dotnet add package FluentCommand.SqlServer
```

For distributed caching:

```shell
dotnet add package FluentCommand.Caching
```

## Quick Start

### Configuration

Register with dependency injection for SQL Server:

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseSqlServer()
);
```

Configure JSON serialization once when using JSON parameters, JSON query-builder values, or `[JsonColumn]` generated readers:

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseSqlServer()
    .UseJsonSerializerOptions(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    })
);
```

Register using a connection name from `appsettings.json`:

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

For PostgreSQL:

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .AddProviderFactory(NpgsqlFactory.Instance)
    .AddPostgreSqlGenerator()
);
```

For SQLite:

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionName("Tracker")
    .AddProviderFactory(SqliteFactory.Instance)
    .AddSqliteGenerator()
);
```

Inject `IDataSession` where you need to run commands:

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

### Direct Configuration

Use `DataConfiguration` when not using dependency injection:

```csharp
var configuration = new DataConfiguration(
    SqlClientFactory.Instance,
    connectionString,
    queryGenerator: new SqlServerGenerator()
);

await using var session = configuration.CreateSession();
```

## Query Examples

### Query Entities

```csharp
var users = await session
    .Sql("select * from [User] where [EmailAddress] like @EmailAddress")
    .Parameter("@EmailAddress", "%@battlestar.com")
    .QueryAsync<User>();
```

`Query<T>` and `QueryAsync<T>` materialize all rows and return `IReadOnlyList<T>`.

### Query a Single Row

```csharp
var user = await session
    .Sql("select * from [User] where [EmailAddress] = @EmailAddress")
    .Parameter("@EmailAddress", "kara.thrace@battlestar.com")
    .QuerySingleAsync<User>();
```

### Query Scalar Values

```csharp
var count = await session
    .Sql("select count(*) from [User] where [IsDeleted] = @IsDeleted")
    .Parameter("@IsDeleted", false)
    .QueryValueAsync<int>();
```

### Execute Commands

```csharp
var affected = await session
    .Sql("update [User] set [LastLogin] = @LastLogin where [Id] = @Id")
    .Parameter("@Id", userId)
    .Parameter("@LastLogin", DateTimeOffset.UtcNow)
    .ExecuteAsync();
```

### Multiple Result Sets

```csharp
User? user = null;
IReadOnlyList<Role> roles = [];
IReadOnlyList<Priority> priorities = [];

await session
    .Sql("""
        select * from [User] where [EmailAddress] = @EmailAddress;
        select * from [Role];
        select * from [Priority];
        """)
    .Parameter("@EmailAddress", "kara.thrace@battlestar.com")
    .QueryMultipleAsync(async query =>
    {
        user = await query.QuerySingleAsync<User>();
        roles = await query.QueryAsync<Role>();
        priorities = await query.QueryAsync<Priority>();
    });
```

### Stored Procedures with Output Parameters

```csharp
long total = -1;

var users = session
    .StoredProcedure("[dbo].[UserListByEmailAddress]")
    .Parameter("@EmailAddress", "%@battlestar.com")
    .Parameter("@Offset", 0)
    .Parameter("@Size", 10)
    .ParameterOut<long>("@Total", value => total = value ?? -1)
    .Query<User>();
```

### JSON Export

```csharp
var json = await session
    .Sql("select * from [Status] order by [DisplayOrder]")
    .QueryJsonAsync();
```

### CSV Export

```csharp
var csv = await session
    .Sql("select * from [Status] order by [DisplayOrder]")
    .QueryCsvAsync();
```

### JSON Parameters

```csharp
var metadata = new { Source = "Import", Count = 42 };

session
    .Sql("insert into [JsonLog] ([Data]) values (@Data)")
    .ParameterJson("@Data", metadata)
    .Execute();
```

`ParameterJson` uses the configured `JsonSerializerOptions` from the session by default. You can also pass options or a source-generated `JsonTypeInfo<T>` for a single parameter.

If the JSON value is already represented as a `JsonElement`, pass it as a regular parameter:

```csharp
using var document = JsonDocument.Parse("""
{
  "Source": "Import",
  "Count": 42
}
""");

JsonElement jsonElement = document.RootElement;

session
    .Sql("insert into [JsonLog] ([Data]) values (@Json)")
    .Parameter("@Json", jsonElement)
    .Execute();
```

## SQL Query Builder

Build parameterized SQL statements using fluent expressions. The builder uses `DataAnnotations` schema attributes to extract table and column information.

### Select

```csharp
var users = await session
    .Sql(builder => builder
        .Select<User>()
        .Column(u => u.Id)
        .Column(u => u.DisplayName)
        .Column(u => u.EmailAddress)
        .Where(u => u.IsDeleted, false)
        .OrderBy(u => u.DisplayName)
        .Page(page: 1, pageSize: 25)
    )
    .QueryAsync<User>();
```

### Conditional Filters

```csharp
var users = await session
    .Sql(builder => builder
        .Select<User>()
        .WhereIf(
            property: u => u.EmailAddress,
            parameterValue: emailFilter,
            filterOperator: FilterOperators.Contains,
            condition: (_, value) => !string.IsNullOrWhiteSpace(value)
        )
        .WhereInIf(
            property: u => u.Id,
            parameterValues: selectedUserIds,
            condition: (_, values) => values.Any()
        )
    )
    .QueryAsync<User>();
```

### Joins

```csharp
var users = await session
    .Sql(builder => builder
        .Select<User>()
        .Column(u => u.DisplayName, "u")
        .Column(u => u.EmailAddress, "u")
        .Column<Role>(r => r.Name, "r", "RoleName")
        .From(tableAlias: "u")
        .Join<UserRole>(join => join
            .Left(u => u.Id, "u")
            .Right(ur => ur.UserId, "ur")
        )
        .Join<UserRole, Role>(join => join
            .Left(ur => ur.RoleId, "ur")
            .Right(r => r.Id, "r")
        )
        .Where(u => u.EmailAddress, "@battlestar.com", "u", FilterOperators.Contains)
        .OrderBy(u => u.DisplayName, "u")
    )
    .QueryAsync<User>();
```

### Insert

```csharp
var userId = await session
    .Sql(builder => builder
        .Insert<User>()
        .Value(u => u.Id, id)
        .Value(u => u.EmailAddress, $"{id}@email.com")
        .Value(u => u.DisplayName, "Last, First")
        .Output(u => u.Id)
    )
    .QueryValueAsync<Guid>();
```

### Update

```csharp
var updatedId = await session
    .Sql(builder => builder
        .Update<User>()
        .Value(u => u.DisplayName, "Updated Name")
        .Output(u => u.Id)
        .Where(u => u.Id, id)
    )
    .QueryValueAsync<Guid>();
```

### Delete

```csharp
var deletedId = await session
    .Sql(builder => builder
        .Delete<User>()
        .Output(u => u.Id)
        .Where(u => u.Id, id)
    )
    .QueryValueAsync<Guid>();
```

### Upsert

```csharp
await session
    .Sql(builder => builder
        .Upsert<StatusUpsert>()
        .Values(status)
        .Output(s => s.Id)
    )
    .QueryValueAsync<int>();
```

### JSON Values in Query Builder

```csharp
await session
    .Sql(builder => builder
        .Insert()
        .Into("JsonLog")
        .Value("Id", Guid.NewGuid())
        .ValueJson("Data", audit)
    )
    .ExecuteAsync();
```

`ValueJson` is available for insert, update, and upsert builders. It uses the session's configured `JsonSerializerOptions` unless options or `JsonTypeInfo<T>` are passed explicitly.

```csharp
await session
    .Sql(builder => builder
        .Upsert()
        .Into("JsonLog")
        .Key("Id")
        .Value("Id", id)
        .ValueJson("Data", audit)
    )
    .ExecuteAsync();
```

### Aggregates and Grouping

```csharp
var total = await session
    .Sql(builder => builder
        .Select<Status>()
        .Aggregate(s => s.DisplayOrder, AggregateFunctions.Sum, columnAlias: "Total")
        .GroupBy(s => s.IsActive)
    )
    .QueryValueAsync<int>();
```

### Raw Statements

```csharp
var statuses = await session
    .Sql(builder =>
    {
        builder
            .Statement()
            .Query("CREATE TABLE #ids (Id int);");

        builder
            .Statement()
            .Query("INSERT INTO #ids (Id) SELECT CONVERT(int, value) FROM STRING_SPLIT(@Ids, @Sep);")
            .Parameter("@Ids", values)
            .Parameter("@Sep", ",");

        builder
            .Select<Status>()
            .From(tableAlias: "s")
            .Join(join => join
                .Left("Id", "s")
                .Right("Id", "#ids", null, "i"));
    })
    .QueryAsync<Status>();
```

## Source Generator

FluentCommand includes a source generator that creates fast `IDataReader` mapping code for entity types, avoiding reflection at runtime. The generator runs when it finds `[Table]` on a class or `[GenerateReader]` pointing to a type.

```csharp
[Table("Status", Schema = "dbo")]
public class Status
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset Created { get; set; }
    public string CreatedBy { get; set; }
    public DateTimeOffset Updated { get; set; }
    public string UpdatedBy { get; set; }

    [ConcurrencyCheck]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    [DataFieldConverter(typeof(ConcurrencyTokenHandler))]
    public ConcurrencyToken RowVersion { get; set; }

    [NotMapped]
    public ICollection<Task> Tasks { get; set; } = new List<Task>();
}
```

Generated extension methods are used automatically by `Query<T>`, `QueryAsync<T>`, `QuerySingle<T>`, and `QuerySingleAsync<T>`. List queries return `IReadOnlyList<T>`:

```csharp
var statuses = await session
    .Sql("select * from [dbo].[Status] order by [DisplayOrder]")
    .QueryAsync<Status>();
```

### Generate for External Types

Use `[GenerateReader]` at the assembly level when you cannot modify the type:

```csharp
[assembly: GenerateReader(typeof(ProductDto))]
[assembly: GenerateReader(typeof(CustomerDto))]
```

### JSON Columns

Use `[JsonColumn]` for properties whose database column stores JSON text:

```csharp
[Table("Import", Schema = "dbo")]
public class ImportRecord
{
    public int Id { get; set; }

    [JsonColumn]
    public ImportMetadata Metadata { get; set; }
}
```

Generated readers deserialize JSON columns with the `JsonSerializerOptions` configured on the active `IDataSession`.

To keep JSON text as a raw `JsonElement`, use `JsonElementHandler` with `[DataFieldConverter]` instead of `[JsonColumn]`:

```csharp
public class ImportRecord
{
    public int Id { get; set; }

    [DataFieldConverter(typeof(JsonElementHandler))]
    public JsonElement Metadata { get; set; }
}
```

### Records and Constructor Initialization

Records with primary constructors are supported:

```csharp
[Table("Status", Schema = "dbo")]
public record StatusRecord(int Id, string Name, bool IsActive);
```

## Caching

Opt-in caching per command with sliding or absolute expiration:

```csharp
var statuses = await session
    .Sql(builder => builder
        .Select<Status>()
        .OrderBy(p => p.DisplayOrder)
    )
    .UseCache(TimeSpan.FromMinutes(5))
    .QueryAsync<Status>();
```

### Distributed Caching

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

## Logging

FluentCommand logs executed commands through `IDataQueryLogger` with command text, parameters, and elapsed time:

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseSqlServer()
    .AddQueryLogger<DataQueryLogger>()
);
```

```text
Executed DbCommand (12.3 ms) [CommandType='Text', CommandTimeout='30']
select * from [User] where [EmailAddress] = @EmailAddress
-- @EmailAddress: Input String(Size=0; Precision=0; Scale=0) [kara.thrace@battlestar.com]
```

## Interceptors

Run code during connection open/close and before command execution:

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseSqlServer()
    .AddInterceptor<CommandAuditInterceptor>()
    .AddInterceptor(sp => new SessionContextInterceptor(sp.GetRequiredService<IUserContext>()))
);
```

## SQL Server Features

```shell
dotnet add package FluentCommand.SqlServer
```

### Bulk Copy

```csharp
await session
    .BulkCopy<User>()
    .Mapping<User>(map => map
        .Ignore(u => u.Id)
        .Ignore(u => u.RowVersion))
    .WriteToServerAsync(users);
```

### Merge Data

```csharp
var processed = await session
    .MergeData("dbo.User")
    .Map<UserImport>(map => map
        .AutoMap()
        .Column(u => u.EmailAddress).Key())
    .ExecuteAsync(users);
```

### Data Import

Higher-level import workflow with field mapping, type conversion, defaults, validation, and merge:

```csharp
services.AddFluentImport();

var definition = ImportDefinition.Build(builder => builder
    .Name("User")
    .TargetTable("dbo.User")
    .CanInsert()
    .CanUpdate()
    .MaxErrors(10)
    .Field(field => field
        .FieldName("EmailAddress")
        .DisplayName("Email Address")
        .DataType<string>()
        .IsKey()
        .Expression("^email$"))
    .Field(field => field
        .FieldName("FirstName")
        .DisplayName("First Name")
        .DataType<string>())
);

var processor = Services.GetRequiredService<IImportProcessor>();
var result = await processor.ImportAsync(definition, importData, username);
```

## Multiple Database Configurations

Use discriminated registrations for multiple databases:

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

## Documentation

Full documentation is available at the [FluentCommand documentation site](https://loresoft.github.io/FluentCommand/).
