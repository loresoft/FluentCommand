using System;
using System.Threading;

namespace FluentCommand.Caching
{
    /// <summary>
    /// Represents a set of eviction and expiration details for a specific cache entry.
    /// </summary>
    public class CachePolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachePolicy"/> class.
        /// </summary>
        public CachePolicy()
        {
            SlidingExpiration = CacheManager.NoSlidingExpiration;
            AbsoluteExpiration = CacheManager.InfiniteAbsoluteExpiration;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether a cache entry should be evicted after a specified duration.
        /// </summary>
        public DateTimeOffset AbsoluteExpiration { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether a cache entry should be evicted if it has not been accessed in a given span of time. 
        /// </summary>
        public TimeSpan SlidingExpiration { get; set; }

        /// <summary>
        /// Gets or sets a reference to a <see cref="CacheItemExpiredCallback"/> delegate that is called after an entry is removed from the cache. 
        /// </summary>
        /// <remarks>
        /// The <see cref="CacheItemExpiredCallback"/> will be invoked asynchronously on the <see cref="ThreadPool"/>.
        /// </remarks>
        public CacheItemExpiredCallback ExpiredCallback { get; set; }
    }
}