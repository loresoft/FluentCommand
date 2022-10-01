namespace FluentCommand;

/// <summary>
/// An <see langword="interface"/> for data cache.
/// </summary>
public interface IDataCache
{
    /// <summary>
    /// Gets the specified cache entry from the cache as an object.
    /// </summary>
    /// <param name="key">A unique identifier for the cache entry.</param>
    /// <returns>The cache entry that is identified by key.</returns>
    object Get(string key);

    /// <summary>
    /// Inserts a cache entry into the cache, specifying information about how the entry will be evicted.
    /// </summary>
    /// <param name="key">A unique identifier for the cache entry.</param>
    /// <param name="value">The object to insert into cache.</param>
    /// <param name="absoluteExpiration">The fixed date and time at which the cache entry will expire.</param>
    void Set(string key, object value, DateTimeOffset absoluteExpiration);

    /// <summary>
    /// Inserts a cache entry into the cache, specifying information about how the entry will be evicted.
    /// </summary>
    /// <param name="key">A unique identifier for the cache entry.</param>
    /// <param name="value">The object to insert into cache.</param>
    /// <param name="slidingExpiration">A value that indicates whether a cache entry should be evicted if it has not been accessed in a given span of time.</param>
    void Set(string key, object value, TimeSpan slidingExpiration);

    /// <summary>
    /// Removes the cache entry from the cache
    /// </summary>
    /// <param name="key">A unique identifier for the cache entry.</param>
    void Remove(string key);
}
