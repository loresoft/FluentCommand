using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FluentCommand.Caching;

/// <summary>
/// Distributed cache implemenation
/// </summary>
/// <seealso cref="FluentCommand.IDataCache" />
public partial class DistributedDataCache : IDataCache
{
    private readonly ILogger<DistributedDataCache> _logger;
    private readonly IDistributedCache _distributedCache;
    private readonly IDistributedCacheSerializer _distributedCacheSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DistributedDataCache"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="distributedCache">The distributed cache.</param>
    /// <param name="distributedCacheSerializer">The distributed cache serializer.</param>
    public DistributedDataCache(ILogger<DistributedDataCache> logger, IDistributedCache distributedCache, IDistributedCacheSerializer distributedCacheSerializer)
    {
        _logger = logger;
        _distributedCache = distributedCache;
        _distributedCacheSerializer = distributedCacheSerializer;
    }

    /// <summary>
    /// Gets the specified cache entry from the cache as an object.
    /// </summary>
    /// <typeparam name="T">The type of item in cache</typeparam>
    /// <param name="key">A unique identifier for the cache entry.</param>
    /// <returns>
    /// <para>Success is true if the key was found; otherwise false</para>
    /// <para>Value is the cache entry that is identified by key</para>
    /// </returns>
    /// <exception cref="System.ArgumentException">'{nameof(key)}' cannot be null or empty. - key</exception>
    public (bool Success, T Value) Get<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));

        var cachedBuffer = _distributedCache.Get(key);

        if (cachedBuffer == null)
        {
            LogCacheMiss(_logger, key);
            return (false, default);
        }

        var cachedItem = _distributedCacheSerializer.Deserialize<T>(cachedBuffer);

        LogCacheHit(_logger, key);

        return (true, cachedItem);
    }

    /// <summary>
    /// Gets the specified cache entry from the cache as an object.
    /// </summary>
    /// <typeparam name="T">The type of item in cache</typeparam>
    /// <param name="key">A unique identifier for the cache entry.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <returns>
    /// <para>Success is true if the key was found; otherwise false</para>
    /// <para>Value is the cache entry that is identified by key</para>
    /// </returns>
    /// <exception cref="System.ArgumentException">'{nameof(key)}' cannot be null or empty. - key</exception>
    public async Task<(bool Success, T Value)> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));

        var cachedBuffer = await _distributedCache
            .GetAsync(key, cancellationToken)
            .ConfigureAwait(false);

        if (cachedBuffer == null)
        {
            LogCacheMiss(_logger, key);
            return (false, default);
        }

        var cachedItem = await _distributedCacheSerializer
            .DeserializeAsync<T>(cachedBuffer, cancellationToken)
            .ConfigureAwait(false);

        LogCacheHit(_logger, key);

        return (true, cachedItem);
    }

    /// <summary>
    /// Inserts a cache entry into the cache, specifying information about how the entry will be evicted.
    /// </summary>
    /// <typeparam name="T">The type of item in cache</typeparam>
    /// <param name="key">A unique identifier for the cache entry.</param>
    /// <param name="value">The object to insert into cache.</param>
    /// <param name="absoluteExpiration">The fixed date and time at which the cache entry will expire.</param>
    /// <param name="slidingExpiration">A value that indicates whether a cache entry should be evicted if it has not been accessed in a given span of time.</param>
    /// <exception cref="System.ArgumentException">'{nameof(key)}' cannot be null or empty. - key</exception>
    /// <exception cref="System.ArgumentNullException">value</exception>
    public void Set<T>(string key, T value, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));

        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var itemBuffer = _distributedCacheSerializer.Serialize(value);

        var distributedOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = absoluteExpiration,
            SlidingExpiration = slidingExpiration
        };

        _distributedCache.Set(key, itemBuffer, distributedOptions);

        LogCacheInsert(_logger, key);
    }

    /// <summary>
    /// Inserts a cache entry into the cache, specifying information about how the entry will be evicted.
    /// </summary>
    /// <typeparam name="T">The type of item in cache</typeparam>
    /// <param name="key">A unique identifier for the cache entry.</param>
    /// <param name="value">The object to insert into cache.</param>
    /// <param name="absoluteExpiration">The fixed date and time at which the cache entry will expire.</param>
    /// <param name="slidingExpiration">A value that indicates whether a cache entry should be evicted if it has not been accessed in a given span of time.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <exception cref="System.ArgumentException">'{nameof(key)}' cannot be null or empty. - key</exception>
    /// <exception cref="System.ArgumentNullException">value</exception>
    public async Task SetAsync<T>(string key, T value, DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));

        if (value is null)
            throw new ArgumentNullException(nameof(value));

        // next set distributed cache
        var itemBuffer = await _distributedCacheSerializer
            .SerializeAsync(value, cancellationToken)
            .ConfigureAwait(false);

        var distributedOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = absoluteExpiration,
            SlidingExpiration = slidingExpiration
        };

        await _distributedCache
            .SetAsync(key, itemBuffer, distributedOptions, cancellationToken)
            .ConfigureAwait(false);

        LogCacheInsert(_logger, key);
    }

    /// <summary>
    /// Removes the cache entry from the cache
    /// </summary>
    /// <param name="key">A unique identifier for the cache entry.</param>
    /// <exception cref="System.ArgumentException">'{nameof(key)}' cannot be null or empty. - key</exception>
    public void Remove(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));

        LogCacheRemove(_logger, key);

        _distributedCache.Remove(key);
    }

    /// <summary>
    /// Removes the cache entry from the cache
    /// </summary>
    /// <param name="key">A unique identifier for the cache entry.</param>
    /// <param name="cancellationToken">The cancellation instruction.</param>
    /// <exception cref="System.ArgumentException">'{nameof(key)}' cannot be null or empty. - key</exception>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));

        LogCacheRemove(_logger, key);

        await _distributedCache.RemoveAsync(key, cancellationToken);
    }


    [LoggerMessage(0, LogLevel.Information, "Cache Hit; Key: '{cacheKey}'")]
    static partial void LogCacheHit(ILogger logger, string cacheKey);

    [LoggerMessage(1, LogLevel.Information, "Cache Miss; Key: '{cacheKey}'")]
    static partial void LogCacheMiss(ILogger logger, string cacheKey);

    [LoggerMessage(2, LogLevel.Information, "Cache Insert; Key: '{cacheKey}'")]
    static partial void LogCacheInsert(ILogger logger, string cacheKey);

    [LoggerMessage(3, LogLevel.Information, "Cache Remove; Key: '{cacheKey}'")]
    static partial void LogCacheRemove(ILogger logger, string cacheKey);


}
