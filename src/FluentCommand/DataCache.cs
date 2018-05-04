using System;
using FluentCommand.Caching;

namespace FluentCommand
{
    /// <summary>
    /// A simple in memory data cache implementation.
    /// </summary>
    /// <seealso cref="FluentCommand.IDataCache" />
    public class DataCache : IDataCache
    {
        private readonly CacheManager _cacheManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCache"/> class.
        /// </summary>
        public DataCache() : this(CacheManager.Cache)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCache"/> class.
        /// </summary>
        /// <param name="cacheManager">The cache manager.</param>
        public DataCache(CacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        /// <summary>
        /// Gets the specified cache entry from the cache as an object.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <returns>
        /// The cache entry that is identified by key.
        /// </returns>
        public object Get(string key)
        {
            return _cacheManager.Get(key);
        }

        /// <summary>
        /// Inserts a cache entry into the cache, specifying information about how the entry will be evicted.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert into cache.</param>
        /// <param name="absoluteExpiration">The fixed date and time at which the cache entry will expire.</param>
        public void Set(string key, object value, DateTimeOffset absoluteExpiration)
        {
            _cacheManager.Set(key, value, absoluteExpiration);
        }

        /// <summary>
        /// Inserts a cache entry into the cache, specifying information about how the entry will be evicted.
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        /// <param name="value">The object to insert into cache.</param>
        /// <param name="slidingExpiration">A value that indicates whether a cache entry should be evicted if it has not been accessed in a given span of time.</param>
        public void Set(string key, object value, TimeSpan slidingExpiration)
        {
            _cacheManager.Set(key, value, slidingExpiration);
        }

        /// <summary>
        /// Removes the cache entry from the cache
        /// </summary>
        /// <param name="key">A unique identifier for the cache entry.</param>
        public void Remove(string key)
        {
            _cacheManager.Remove(key);
        }

        #region Singleton
        private static readonly Lazy<DataCache> _current = new Lazy<DataCache>(() => new DataCache());

        /// <summary>
        /// Gets a reference to the default <see cref="DataCache"/> instance.
        /// </summary>
        public static DataCache Default => _current.Value;
        #endregion

    }
}