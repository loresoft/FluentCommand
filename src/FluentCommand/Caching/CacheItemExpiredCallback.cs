using System;
using System.Threading;

namespace FluentCommand.Caching
{
    /// <summary>
    /// Defines a reference to a method that is called after a cache entry is removed from the cache.
    /// </summary>
    /// <param name="cacheItem">The <see cref="CacheItem"/> that has expired..</param>
    /// <remarks>
    /// The <see cref="CacheItemExpiredCallback"/> will be invoked asynchronously on the <see cref="ThreadPool"/>.
    /// </remarks>
    public delegate void CacheItemExpiredCallback(CacheItem cacheItem);
}