# Source Generator

FluentCommand includes a source generator that creates fast `IDataReader` mapping code for entity types. The generated code avoids reflection for query materialization and adds typed query extension methods for each generated entity.

The generator runs when it finds either:

- `TableAttribute` on a concrete class or record.
- `GenerateReaderAttribute` pointing to a type.

## Setup

The generator is included with the main `FluentCommand` package and runs during build.

```powershell
PM> Install-Package FluentCommand
```

No runtime registration is required. Add the mapping attributes to your entity or query model, rebuild the project, and call the generated query methods through the normal `IDataSession` command pipeline.

## Generate from a table entity

Add `TableAttribute` to an entity you own. Public properties with supported types are included in the generated reader.

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentCommand;

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

The generated extension methods materialize query results by matching returned column names to mapped property names.

```csharp
await using var session = configuration.CreateSession();

var statuses = await session
    .Sql("select * from [dbo].[Status] order by [DisplayOrder]")
    .QueryAsync<Status>();
```

Generated methods are also used by single-row queries.

```csharp
var status = await session
    .Sql("select * from [dbo].[Status] where [Id] = @Id")
    .Parameter("@Id", id)
    .QuerySingleAsync<Status>();
```

## Generate for external types

Use `GenerateReaderAttribute` when a type is defined in another project or package and you cannot add `TableAttribute` to it. The attribute can be applied at the assembly level and repeated for each type.

```csharp
using FluentCommand.Attributes;
using MyApp.Contracts;

[assembly: GenerateReader(typeof(ProductDto))]
[assembly: GenerateReader(typeof(CustomerDto))]
```

The generator reads the public properties from the specified type and creates the same reader and type-accessor extensions that are generated for `[Table]` entities.

`GenerateReaderAttribute` is also useful for local DTOs that are query models rather than table models.

```csharp
using FluentCommand.Attributes;

[assembly: GenerateReader(typeof(StatusSummary))]

public class StatusSummary
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int TaskCount { get; set; }
}
```

```csharp
var summaries = await session
    .Sql("""
        select s.[Id], s.[Name], count(t.[Id]) as [TaskCount]
        from [dbo].[Status] s
        left join [dbo].[Task] t on t.[StatusId] = s.[Id]
        group by s.[Id], s.[Name]
        """)
    .QueryAsync<StatusSummary>();
```

## Generated members

For an entity named `Status`, the generator creates a static `StatusDataReaderExtensions` class in the entity namespace. The generated class includes:

- `Query<Status>()` for synchronous list queries.
- `QueryAsync<Status>()` for asynchronous list queries.
- `QuerySingle<Status>()` for synchronous single-row queries.
- `QuerySingleAsync<Status>()` for asynchronous single-row queries.
- `StatusFactory(IDataReader dataRecord)` for direct row materialization.

The query methods call the existing FluentCommand query APIs with the generated factory and use `CommandBehavior.SequentialAccess`.

## Column mapping

By default, the returned column name must match the property name.

```sql
select [Id], [Name], [DisplayOrder]
from [dbo].[Status]
```

Use `ColumnAttribute` when the database column name differs from the property name.

```csharp
using System.ComponentModel.DataAnnotations.Schema;

[Table("Status", Schema = "dbo")]
public class StatusView
{
    public int Id { get; set; }

    [Column("display_name")]
    public string Name { get; set; }
}
```

```sql
select [Id], [display_name]
from [dbo].[Status]
```

Columns returned by the query that do not map to a property are ignored. Mapped properties keep their default value when a returned column value is `DBNull`.

## Supported property types

The generator maps public, non-indexer, non-abstract properties with supported types.

Supported types include:

- `bool`, `byte`, `char`, `decimal`, `double`, `float`, `short`, `int`, `long`, and nullable versions.
- `string`.
- `byte[]`.
- `DateTime`, `DateTimeOffset`, `Guid`, `TimeSpan`, `DateOnly`, and `TimeOnly`.
- Enums.
- `ConcurrencyToken`.
- JSON properties marked with `JsonColumnAttribute`.

Unsupported properties are reported by analyzer diagnostic `FLC004` and are not mapped unless they are ignored or marked as JSON columns.

## Ignore properties

Use `NotMappedAttribute` for model properties that are not returned by the query.

```csharp
using System.ComponentModel.DataAnnotations.Schema;

[NotMapped]
public ICollection<Task> Tasks { get; set; } = new List<Task>();
```

Use `IgnorePropertyAttribute` when you need FluentCommand-specific ignore behavior. It can be applied to a property or to the class.

```csharp
using FluentCommand.Attributes;

[IgnoreProperty(nameof(InternalState))]
public class StatusModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string InternalState { get; set; }
}
```

## Custom field conversion

Use `DataFieldConverterAttribute` when a column needs custom conversion from the data record.

```csharp
[ConcurrencyCheck]
[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
[DataFieldConverter(typeof(ConcurrencyTokenHandler))]
public ConcurrencyToken RowVersion { get; set; }
```

The generated reader creates the converter and calls `ReadValue` for that property.

## JSON columns

Use `JsonColumnAttribute` for properties whose database column stores JSON text. The generated reader deserializes the column with `GetFromJson<T>` and uses JSON serializer options in this order:

1. An explicit `JsonSerializerOptions?` argument passed to the generated overload.
2. Options configured on FluentCommand with `UseJsonSerializerOptions(...)`.
3. `null`, which uses default `System.Text.Json` behavior.

```csharp
using FluentCommand;
using FluentCommand.Attributes;

[Table("Import", Schema = "dbo")]
public class ImportRecord
{
    public int Id { get; set; }

    [JsonColumn]
    public ImportMetadata Metadata { get; set; }
}

public class ImportMetadata
{
    public string FileName { get; set; }
    public int RowCount { get; set; }
}
```

Use `DataFieldConverterAttribute` with `JsonElementHandler` when you want to keep the JSON payload as a raw `JsonElement` instead of deserializing it into a POCO. The handler reads JSON text from the data record and returns a `JsonElement`. Database `NULL` values are returned as the default `JsonElement` with `JsonValueKind.Undefined`.

```csharp
using System.Text.Json;
using FluentCommand;
using FluentCommand.Handlers;

[Table("Import", Schema = "dbo")]
public class ImportRecord
{
    public int Id { get; set; }

    [DataFieldConverter(typeof(JsonElementHandler))]
    public JsonElement Metadata { get; set; }
}
```

Configure shared JSON options on the FluentCommand configuration builder:

```csharp
using System.Text.Json;

services.AddFluentCommand(builder => builder
    .UseConnectionString(connectionString)
    .UseJsonSerializerOptions(new JsonSerializerOptions(JsonSerializerDefaults.Web)));
```

Generated query extensions for entities with JSON columns also include overloads that accept explicit options. Explicit options override configured context options for that call.

```csharp
var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
var imports = session.Sql("select Id, Metadata from dbo.Import")
    .Query<ImportRecord>(options);
```

For assembly-level generated readers, use `GenerateReaderAttribute.JsonProperties` to identify JSON properties on types you cannot annotate directly.

## Constructor initialization

Types with a parameterless constructor are created with an object initializer. Types without a parameterless constructor can still be generated when they have a constructor whose parameter count matches the number of mappable properties.

Constructor parameter names are matched to public property names case-insensitively.

```csharp
[Table("Status", Schema = "dbo")]
public class StatusSnapshot
{
    public StatusSnapshot(int id, string name, bool isActive)
    {
        Id = id;
        Name = name;
        IsActive = isActive;
    }

    public int Id { get; }
    public string Name { get; }
    public bool IsActive { get; }
}
```

Records with primary constructors follow the same constructor matching rules.

```csharp
[Table("Status", Schema = "dbo")]
public record StatusRecord(int Id, string Name, bool IsActive);
```

## Diagnostics

The generator includes analyzer diagnostics to catch mapping issues during build.

| Diagnostic | Severity | Description |
| --- | --- | --- |
| `FLC001` | Error | The type has no parameterless constructor and no constructor matching the mappable property count. |
| `FLC002` | Warning | A constructor parameter does not match any public property name. |
| `FLC003` | Warning | The type has no mappable properties. |
| `FLC004` | Info | A property has an unsupported type and will not be mapped. |
| `FLC005` | Error | `GenerateReaderAttribute` has an invalid or missing type argument. |
| `FLC006` | Warning | `TableAttribute` is applied to a static or abstract type. |

## Generated files

Enable generated file output in a project when you want to inspect the generated reader code.

```xml
<PropertyGroup Condition="'$(Configuration)'=='Debug'">
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>
```

The generated files include the entity reader extensions and a generated type accessor for the mapped entity.
