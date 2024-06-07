# Configuration

## Configuration for SQL Server

```csharp
var dataConfiguration  = new DataConfiguration(
    SqlClientFactory.Instance, 
    ConnectionString
);
```

## Configure data logger

```csharp
var dataLogger = new DataQueryLogger(Output.WriteLine);
var dataConfiguration  = new DataConfiguration(
    SqlClientFactory.Instance, 
    ConnectionString,
    queryLogger: dataLogger
);
```

## Register with dependency injection

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(ConnectionString)
    .UseSqlServer()
);
```

## Register using a connection name from the appsettings.json

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionName("Tracker")
    .UseSqlServer()
);
```

```json
{
  "ConnectionStrings": {
    "Tracker": "Data Source=(local);Initial Catalog=TrackerTest;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

## Register for PostgreSQL

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionName("Tracker")
    .AddProviderFactory(NpgsqlFactory.Instance)
    .AddPostgreSqlGenerator()
);
```
