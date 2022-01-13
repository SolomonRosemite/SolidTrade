using SolidTradeServer.Data.Models.Common.Cache;

namespace SolidTradeServer.Services.Cache
{
    /// <summary>
    /// A Service which can be used to get or set cache values.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Gets the cached value.
        /// </summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="identifier">The entity id.</param>
        /// <returns>Returns the cached value or if expired null.</returns>
        CacheEntry<T> GetCachedValue<T>(string identifier);

        /// <summary>
        /// Sets a cache value.
        /// </summary>
        /// <typeparam name="T">The expected type.</typeparam>
        /// <param name="identifier">The entity id.</param>
        /// <param name="value">The item that should be cached.</param>
        void SetCachedValue<T>(string identifier, T value);
    }
}
