# Caching

FluentCommand can cache query results through an `IDataCache` implementation configured on `DataConfiguration`. The configured cache is exposed as `DataCache` on the configuration and as `Cache` on sessions created from that configuration.

Caching is opt-in per command. Add `UseCache` before a query method to read from the cache first and store the query result when no cached value exists.

## Configure a cache

Register an `IDataCache` implementation with `AddDataCache`.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(ConnectionString)
    .UseSqlServer()
    .AddDataCache<MyDataCache>()
);
```

You can also register an instance or factory.

```csharp
services.AddFluentCommand(builder => builder
    .UseConnectionString(ConnectionString)
    .UseSqlServer()
    .AddDataCache<MyDataCache>(sp => new MyDataCache(sp.GetRequiredService<IMemoryCache>()))
);
```

When creating `DataConfiguration` directly, pass the cache to the constructor.

```csharp
var dataConfiguration = new DataConfiguration(
    SqlClientFactory.Instance,
    ConnectionString,
    cache: new MyDataCache(memoryCache)
);

var dataCache = dataConfiguration.DataCache;
```

## Implement IDataCache

Implement `IDataCache` to use any cache provider. The interface supports synchronous and asynchronous get, set, and remove operations.

```csharp
public sealed class MyDataCache : IDataCache
{
    private readonly IMemoryCache _memoryCache;

    public MyDataCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public (bool Success, T? Value) Get<T>(string key)
    {
        var success = _memoryCache.TryGetValue(key, out T? value);
        return (success, value);
    }

    public Task<(bool Success, T? Value)> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Get<T>(key));
    }

    public void Set<T>(string key, T value, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = absoluteExpiration,
            SlidingExpiration = slidingExpiration
        };

        _memoryCache.Set(key, value, options);
    }

    public Task SetAsync<T>(string key, T value, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        Set(key, value, absoluteExpiration, slidingExpiration);
        return Task.CompletedTask;
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        Remove(key);
        return Task.CompletedTask;
    }
}
```

## Cache query results

Use a sliding expiration to keep a cached result alive while it is being read.

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

var statuses = await session
    .Sql(builder => builder
        .Select<Status>()
        .OrderBy(p => p.DisplayOrder)
    )
    .UseCache(TimeSpan.FromMinutes(5))
    .QueryAsync<Status>();
```

Use an absolute expiration to expire the cached result at a fixed point in time.

```csharp
await using var session = Services.GetRequiredService<IDataSession>();

var expires = DateTimeOffset.UtcNow.AddMinutes(10);

var user = await session
    .Sql("select * from [User] where [EmailAddress] = @EmailAddress")
    .Parameter("@EmailAddress", email)
    .UseCache(expires)
    .QuerySingleAsync<User>();
```

The cache key is generated from the command type, command text, result type, input parameter names, input parameter values, and input parameter database types. Reusing the same command and parameters with `UseCache` reads the same cache entry.

Commands with output, input-output, or return parameters cannot be cached.

## Expire cached results

Use `ExpireCache<TEntity>` with the same command text and parameters used for the cached query. FluentCommand rebuilds the same cache key and removes that entry from the configured `IDataCache`.

```csharp
using var session = Services.GetRequiredService<IDataSession>();

session
    .Sql("select * from [User] where [EmailAddress] = @EmailAddress")
    .Parameter("@EmailAddress", email)
    .UseCache(TimeSpan.FromMinutes(5))
    .ExpireCache<User>();
```

## Use FluentCommand.Caching

The `FluentCommand.Caching` package provides distributed cache support through `Microsoft.Extensions.Caching.Distributed`.

```powershell
PM> Install-Package FluentCommand.Caching
```

Register an `IDistributedCache` provider, then call `AddDistributedDataCache`.

```csharp
using FluentCommand.Caching;

services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = RedisConnectionString;
    options.InstanceName = "FluentCommand";
});

services.AddFluentCommand(builder => builder
    .UseConnectionString(ConnectionString)
    .UseSqlServer()
    .AddDistributedDataCache()
);
```

`AddDistributedDataCache` registers `DistributedDataCache` as the configured `IDataCache`. It uses `IDistributedCache` for storage and `MessagePackCacheSerializer` as the default serializer.

Distributed cache entries use the expiration supplied by `UseCache`.

```csharp
var priorities = await session
    .Sql(builder => builder
        .Select<Priority>()
        .OrderBy(p => p.Name)
    )
    .UseCache(TimeSpan.FromMinutes(15))
    .QueryAsync<Priority>();
```

`DistributedDataCache` writes debug logs for cache hits, misses, inserts, and removals.

```text
Cache Miss; Key: 'fluent:data:query:1A2B3C4D'
Cache Insert; Key: 'fluent:data:query:1A2B3C4D'
Cache Hit; Key: 'fluent:data:query:1A2B3C4D'
```

## Custom distributed serialization

Override the serializer by registering your own `IDistributedCacheSerializer` before calling `AddDistributedDataCache`.

```csharp
services.AddSingleton<IDistributedCacheSerializer, JsonDistributedCacheSerializer>();

services.AddFluentCommand(builder => builder
    .UseConnectionString(ConnectionString)
    .UseSqlServer()
    .AddDistributedDataCache()
);
```

`AddDistributedDataCache` uses `TryAddSingleton`, so an existing `IDistributedCacheSerializer` registration is preserved.
