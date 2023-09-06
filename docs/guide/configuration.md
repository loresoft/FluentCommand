# Configuration

Configuration for SQL Server

```csharp
var dataConfiguration  = new DataConfiguration(
    SqlClientFactory.Instance, 
    ConnectionString
);
```

Configure data logger

```csharp
var dataLogger = new DataQueryLogger(Output.WriteLine);
var dataConfiguration  = new DataConfiguration(
    SqlClientFactory.Instance, 
    ConnectionString,
    queryLogger: dataLogger
);
```
