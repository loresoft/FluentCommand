# Bulk Operations

The `FluentCommand.SqlServer` project includes SQL Server-specific helpers for moving larger sets of data into a database. Use these APIs when row-by-row `INSERT` or `UPDATE` commands would be too slow, or when uploaded tabular data needs to be validated and merged into a target table.

These features require a session backed by `Microsoft.Data.SqlClient.SqlConnection`. Configure SQL Server with `UseSqlServer()` before using them.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseSqlServer()
);
```

Use these namespaces in application code as needed.

```csharp
using FluentCommand.Bulk;
using FluentCommand.Merge;
using FluentCommand.Import;
```

## Choose an API

Use `BulkCopy` when you only need to append rows to a SQL Server table. It wraps `SqlBulkCopy` and supports `IEnumerable<T>`, `DataTable`, `DataRow[]`, and `IDataReader` sources.

Use `MergeData` when source rows should be inserted, updated, or deleted in a target table based on key columns. It can use a generated SQL `MERGE` statement or bulk copy into a temporary table before merging.

Use `IImportProcessor` when user-provided tabular data needs field mapping, type conversion, defaults, validation, and then merge into a target table.

## Bulk copy

Start a bulk copy operation from an `IDataSession`.

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

await session
    .BulkCopy<User>()
    .Mapping<User>(map => map
        .Ignore(u => u.Id)
        .Ignore(u => u.RowVersion)
        .Ignore(u => u.Audits)
        .Ignore(u => u.AssignedTasks)
        .Ignore(u => u.CreatedTasks)
        .Ignore(u => u.Roles))
    .WriteToServerAsync(users, cancellationToken);
```

`BulkCopy<TEntity>()` uses entity table metadata for the destination table and enables automatic mapping. Automatic mapping maps source field names to destination column names. For `IEnumerable<T>`, FluentCommand reads entities through `ListDataReader<T>` and applies the mappings to the underlying `SqlBulkCopy`.

Use `BulkCopy("table")` when you want to specify the destination table explicitly.

```csharp
session
    .BulkCopy("dbo.User")
    .AutoMap()
    .Ignore("RowVersion")
    .WriteToServer(users);
```

You can configure mappings by source and destination column names.

```csharp
session
    .BulkCopy("dbo.User")
    .Mapping("EmailAddress", "EmailAddress")
    .Mapping("FirstName", "FirstName")
    .Mapping("LastName", "LastName")
    .Mapping("DisplayName", "DisplayName")
    .WriteToServer(users);
```

Strongly typed mapping is useful when source property names should be checked by the compiler.

```csharp
session
    .BulkCopy("dbo.User")
    .Mapping<UserImport>(map => map
        .Mapping(u => u.EmailAddress)
        .Mapping(u => u.FirstName)
        .Mapping(u => u.LastName)
        .Mapping(u => u.DisplayName))
    .WriteToServer(importRows);
```

Bulk copy also accepts `DataTable`, `DataRow[]`, and `IDataReader`.

```csharp
await session
    .BulkCopy("dbo.User")
    .AutoMap()
    .EnableStreaming()
    .WriteToServerAsync(reader, cancellationToken);
```

## Bulk copy options

The bulk copy builder exposes common `SqlBulkCopy` options.

```csharp
await session
    .BulkCopy("dbo.User")
    .AutoMap()
    .BatchSize(1_000)
    .BulkCopyTimeout(120)
    .EnableStreaming()
    .TableLock()
    .KeepNulls()
    .CheckConstraints()
    .FireTriggers()
    .WriteToServerAsync(reader, cancellationToken);
```

`BatchSize` controls how many rows are sent in each batch. `BulkCopyTimeout` is in seconds. `TableLock`, `KeepIdentity`, `KeepNulls`, `CheckConstraints`, `FireTriggers`, and `UseInternalTransaction` map to `SqlBulkCopyOptions` flags.

If the session has an active SQL transaction, bulk copy uses that transaction. Otherwise, `UseInternalTransaction()` can be used when each bulk-copy batch should run in its own internal transaction.

## Merge data

`MergeData` inserts rows that are missing from the target table and updates rows that already exist. By default, insert and update are enabled, delete is disabled, and `DataMergeMode.Auto` chooses a merge strategy based on the source size.

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

var processed = await session
    .MergeData("dbo.User")
    .Map<UserImport>(map => map
        .AutoMap()
        .Column(u => u.EmailAddress).Key())
    .ExecuteAsync(users, cancellationToken);
```

`AutoMap()` maps entity properties to merge columns. Mark at least one column as a key with `Key()`. Key columns are used to match source rows to target rows and are not updated.

Configure individual columns when you need explicit SQL Server native types or different insert/update behavior.

```csharp
var processed = session
    .MergeData("dbo.User")
    .Mode(DataMergeMode.BulkCopy)
    .Map<UserImport>(map =>
    {
        map.Column(u => u.EmailAddress)
            .NativeType("nvarchar(256)")
            .Key();

        map.Column(u => u.FirstName)
            .NativeType("nvarchar(256)");

        map.Column(u => u.LastName)
            .NativeType("nvarchar(256)");

        map.Column(u => u.DisplayName)
            .NativeType("nvarchar(256)");
    })
    .Execute(users);
```

You can also let entity metadata create the target table and column mapping.

```csharp
var processed = await session
    .MergeData<User>()
    .Map<User>(map => map
        .AutoMap()
        .Column(u => u.EmailAddress).Key())
    .ExecuteAsync(users, cancellationToken);
```

## Merge modes and actions

`DataMergeMode.Auto` uses a generated SQL statement for smaller source sets and bulk copy for larger source sets. Use `Mode(DataMergeMode.SqlStatement)` or `Mode(DataMergeMode.BulkCopy)` when you want to choose the strategy explicitly.

```csharp
await session
    .MergeData("dbo.User")
    .Mode(DataMergeMode.SqlStatement)
    .IncludeInsert()
    .IncludeUpdate()
    .IncludeDelete(false)
    .Map<UserImport>(map => map
        .AutoMap()
        .Column(u => u.EmailAddress).Key())
    .ExecuteAsync(users, cancellationToken);
```

Use `IncludeInsert(false)` for update-only merges, `IncludeUpdate(false)` for insert-only merges, and `IncludeDelete()` when rows missing from the source should be deleted from the target table. Treat delete-enabled merges carefully because the source data becomes the complete desired set for the key space being merged.

Use `IdentityInsert()` when identity values from the source should be inserted into the target table.

```csharp
var processed = session
    .MergeData("dbo.User")
    .IdentityInsert()
    .Map<User>(map => map
        .AutoMap()
        .Column(u => u.Id).Key())
    .Execute(users);
```

Set merge command timeout with seconds or a `TimeSpan`.

```csharp
await session
    .MergeData("dbo.User")
    .CommandTimeout(TimeSpan.FromMinutes(2))
    .Map<UserImport>(map => map
        .AutoMap()
        .Column(u => u.EmailAddress).Key())
    .ExecuteAsync(users, cancellationToken);
```

## Merge output

Use `ExecuteOutput` or `ExecuteOutputAsync` when you need to inspect what changed. Each `DataMergeOutputRow` includes an action such as `INSERT`, `UPDATE`, or `DELETE` and a list of changed columns.

```csharp
var changes = await session
    .MergeData("dbo.User")
    .Map<UserImport>(map => map
        .AutoMap()
        .Column(u => u.EmailAddress).Key())
    .ExecuteOutputAsync(users, cancellationToken);

foreach (var change in changes)
{
    var email = change["EmailAddress"]?.Current;
    logger.LogInformation("{Action} {EmailAddress}", change.Action, email);
}
```

## Import services

Import processing is a higher-level workflow built on merge. It converts tabular string data into a typed `DataTable`, applies defaults and translators, validates rows, and merges valid rows into the target table.

Register import services with `AddFluentImport()`.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseSqlServer()
);

services.AddFluentImport();
```

`AddFluentImport()` registers the default `ImportValidator` and `IImportProcessor`. It uses the configured `IDataSession`, so the same SQL Server configuration is used for the final merge.

## Import definitions

An `ImportDefinition` describes the import name, target table, allowed operations, field definitions, mapping expressions, defaults, translators, and validator key.

```csharp
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
        .Expression("^email$")
        .Expression("e-mail"))
    .Field(field => field
        .FieldName("FirstName")
        .DisplayName("First Name")
        .DataType<string>())
    .Field(field => field
        .FieldName("LastName")
        .DisplayName("Last Name")
        .DataType<string>())
    .Field(field => field
        .FieldName("UpdatedBy")
        .DataType<string>()
        .Default(FieldDefault.UserName))
    .Field(field => field
        .FieldName("Updated")
        .DataType<DateTimeOffset>()
        .Default(FieldDefault.CurrentDate))
);
```

Use the typed builder when a model already describes the target table.

```csharp
var definition = ImportDefinitionBuilder<UserImport>.Build(builder => builder
    .AutoMap()
    .Field(u => u.EmailAddress)
        .DisplayName("Email Address")
        .Required()
        .IsKey()
        .Expression("^email$")
);
```

The typed builder reads table metadata from attributes, sets the import name from the model type, and sets the default validator key to the built-in `ImportValidator`.

## Import data and mapping

`ImportData` carries the source file name, source rows, field mappings, and whether the first row is a header. Data is represented as `string[][]` so it can come from CSV, Excel, JSON, or another upload parser.

```csharp
var importData = new ImportData
{
    FileName = "users.csv",
    HasHeader = true,
    Mappings =
    [
        new FieldMap { Name = "EmailAddress", Index = 0 },
        new FieldMap { Name = "FirstName", Index = 1 },
        new FieldMap { Name = "LastName", Index = 2 }
    ],
    Data =
    [
        ["Email", "First", "Last"],
        ["user1@email.com", "Kara", "Thrace"],
        ["user2@email.com", "Lee", "Adama"]
    ]
};
```

You can build the mappings from source headers using the field expressions in the definition.

```csharp
var headers = importData.Data[0]
    .Select((name, index) => new FieldMap { Name = name, Index = index })
    .ToList();

importData.Mappings = definition.BuildMapping(headers);
```

## Run an import

Resolve `IImportProcessor` and call `ImportAsync` with the definition, data, and current user name. The returned `ImportResult` includes the processed count and any row errors collected before `MaxErrors` is exceeded.

```csharp
var processor = Services.GetRequiredService<IImportProcessor>();

var result = await processor.ImportAsync(
    definition,
    importData,
    username: currentUser.EmailAddress,
    cancellationToken);

if (result.Errors is { Count: > 0 })
{
    foreach (var error in result.Errors)
        logger.LogWarning("Import error: {Error}", error);
}
```

During processing, mapped string values are converted to each field's `DataType`. Empty values convert to `null` for nullable types. Fields with defaults are not read from the source mapping; the processor supplies the current UTC date, current username, or static value.

## Translators and validators

Use an `IFieldTranslator` when a source value needs custom transformation before conversion or merge.

```csharp
public sealed class ActiveFlagTranslator : IFieldTranslator
{
    public Task<object> Translate(string original)
    {
        var value = original.Equals("active", StringComparison.OrdinalIgnoreCase);
        return Task.FromResult<object>(value);
    }
}
```

Register translators as keyed services and reference the key from a field definition.

```csharp
services.AddKeyedTransient<IFieldTranslator, ActiveFlagTranslator>("active-flag");

var definition = ImportDefinition.Build(builder => builder
    .Name("User")
    .TargetTable("dbo.User")
    .Field(field => field
        .FieldName("IsActive")
        .DataType<bool>()
        .TranslatorKey("active-flag"))
);
```

Use an `IImportValidator` when validation needs to look at a complete target row after conversion.

```csharp
public sealed class UserImportValidator : IImportValidator
{
    public Task ValidateRow(ImportDefinition importDefinition, DataRow targetRow)
    {
        if (targetRow["EmailAddress"] is not string email || !email.Contains('@'))
            throw new ValidationException("EmailAddress must be a valid email address.");

        return Task.CompletedTask;
    }
}
```

Register the validator as a keyed service and configure the definition with the same key.

```csharp
services.AddKeyedScoped<IImportValidator, UserImportValidator>("user-import");

var definition = ImportDefinitionBuilder<UserImport>.Build(builder =>
{
    builder.AutoMap();
    builder.ValidatorKey("user-import");
    builder.Field(u => u.EmailAddress).IsKey();
});
```

## Operational notes

Bulk copy and bulk merge require SQL Server. If the session was not created with `SqlConnection`, FluentCommand throws an invalid operation exception.

Merge definitions must have a target table, at least one non-ignored column, at least one key column, source column names, and SQL Server native types. `AutoMap()` fills native types from model metadata when possible. If you map columns manually for bulk-copy merge mode, set `NativeType` for each source column.

Import processing skips empty rows, collects row errors, and stops by rethrowing when the error count exceeds `MaxErrors`. Valid rows are merged with `MergeData`, using `CanInsert`, `CanUpdate`, field key settings, and field insert/update flags from the import definition.
