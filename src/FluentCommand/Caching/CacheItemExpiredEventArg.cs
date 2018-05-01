using System;

namespace FluentCommand.Caching
{
    /// <summary>
    /// An <see cref="EventArgs"/> class for CacheItemExpired event.
    /// </summary>
    public class CacheItemExpiredEventArg : EventArgs
    {
        /// <summary>
        /// Gets or sets the cache item.
        /// </summary>
        /// <value>
        /// The cache item.
        /// </value>
        public CacheItem CacheItem { get; set; }
    }
}