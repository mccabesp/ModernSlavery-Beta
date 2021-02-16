using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Redis.Fluent;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Infrastructure.Caching;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;
using static ModernSlavery.Infrastructure.Azure.Cache.RedisCacheRebootOptions;

namespace ModernSlavery.Infrastructure.Azure.Cache
{
    public class DistributedCacheManager
    {
        private readonly AzureManager _azureManager;
        private readonly DistributedCacheOptions _cacheOptions;
        private readonly IServiceProvider _serviceProvider;

        public readonly string CacheName;

        public DistributedCacheManager(AzureManager azureManager, DistributedCacheOptions cacheOptions, IServiceProvider serviceProvider)
        {
            _azureManager = azureManager ?? throw new ArgumentNullException(nameof(azureManager));
            _cacheOptions = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
            _serviceProvider = serviceProvider;
        }

        public async Task ClearCacheAsync()
        {
            var distributedCache=_serviceProvider.GetRequiredService<IDistributedCache>();
            if (distributedCache is ResetableMemoryCache memoryCache)
                await ClearMemoryCacheAsync(memoryCache).ConfigureAwait(false);
            else if (distributedCache is RedisCache redisCache)
                await ClearRedisCacheAsync(redisCache).ConfigureAwait(false);
            else
            {
                var cacheType = distributedCache.GetType();
                throw new ArgumentOutOfRangeException(nameof(cacheType), $"Unrecognised DistributedCache type='{cacheType}'");
            }
        }

        public async Task ClearMemoryCacheAsync(ResetableMemoryCache memoryCache)
        {
            memoryCache.Reset();
        }

        public async Task ClearRedisCacheAsync(RedisCache redisCache)
        {
            var options = ConfigurationOptions.Parse(_cacheOptions.AzureConnectionString);
            options.ConnectRetry = 5;
            options.AllowAdmin = true;

            var connectionMultiplexer = ConnectionMultiplexer.Connect(options);
            try
            {
                var endpoints = connectionMultiplexer.GetEndPoints(true);
                foreach (var endpoint in endpoints)
                {
                    var server = connectionMultiplexer.GetServer(endpoint);
                    await server.FlushAllDatabasesAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                connectionMultiplexer.Close();
            }
        }

        public void Reboot(string cacheName=null, RebootTypes rebootType = RebootTypes.AllNodes)
        {
            var redisCache = FindRedisCacheByName(cacheName);

            if (redisCache == null) throw new ArgumentException($"Cannot find cache '{cacheName}'", nameof(cacheName));

            redisCache.ForceReboot(rebootType.ToString());
        }

        public IRedisCache FindRedisCacheByName(string cacheName, string resourceGroup=null)
        {
            if (string.IsNullOrWhiteSpace(cacheName)) throw new ArgumentNullException(nameof(cacheName));

            IRedisCache redisCache = null;
            if (string.IsNullOrWhiteSpace(resourceGroup))
            {
                var caches = _azureManager.Azure.RedisCaches.List().Where(s => s.Name.EqualsI(cacheName)).ToList();
                if (caches.Count > 1)throw new ArgumentNullException(nameof(resourceGroup),$"You must specify a resourceGroup as redis cache {cacheName} exists in {caches.Count} resource groups");
                redisCache = caches.FirstOrDefault();
            }
            else
                redisCache = _azureManager.Azure.RedisCaches.GetByResourceGroup(resourceGroup, cacheName);

            return redisCache;
        }

        public IRedisCache FindRedisCacheByConnectionString(string redisConnectionString)
        {
            var cacheName = redisConnectionString.BeforeFirst(".");
            return FindRedisCacheByName(cacheName);
        }
    }
}
