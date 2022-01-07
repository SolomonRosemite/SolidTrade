namespace SolidTradeServer.Data.Models.Common.Cache
{
    /// <summary>
    /// Contains information about the cache.
    /// </summary>
    /// <typeparam name="T">Represents the type of the cache.</typeparam>
    public struct CacheEntry<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the the cache is Expired or not.
        /// </summary>
        public bool Expired { get; set; }

        /// <summary>
        /// Gets or sets the cached value.
        /// </summary>
        public T Value { get; set; }
    }
}
