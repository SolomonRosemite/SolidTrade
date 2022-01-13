using System;
using System.Runtime.Caching;
using Microsoft.Extensions.Configuration;
using SolidTradeServer.Data.Models.Common.Cache;

namespace SolidTradeServer.Services.Cache
{
    /// <inheritdoc/>
    public class CacheService : ICacheService
    {
        private const string DefaultCacheTimeoutKey = "CachePolicy:DefaultTimeout";
        private readonly CacheItemPolicy _cachePolicy;
        private readonly ObjectCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheService"/> class.
        /// </summary>
        /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
        public CacheService(IConfiguration configuration)
        {
            var expirationTimeSpan = GetDefaultTimeSpan(configuration);

            _cachePolicy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.Add(expirationTimeSpan) };
            _cache = MemoryCache.Default;
        }

        /// <inheritdoc/>
        public CacheEntry<T> GetCachedValue<T>(string identifier)
        {
            var cache = _cache[GetCacheKey(typeof(T), identifier)];
            var isExpired = cache is null;
            
            return new CacheEntry<T> { Expired = isExpired, Value = isExpired ?  default : (T)cache, };
        }

        /// <inheritdoc/>
        public void SetCachedValue<T>(string identifier, T value) => _cache.Set(GetCacheKey(typeof(T), identifier), value, _cachePolicy);

        private static string GetCacheKey(Type type, string identifier) => type.Name + "_" + identifier;

        private static TimeSpan GetDefaultTimeSpan(IConfiguration configuration) => TimeSpan.Parse(configuration[DefaultCacheTimeoutKey]);
    }
}
