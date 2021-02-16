using System;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis;

namespace ModernSlavery.Infrastructure.Caching
{
    public static class Extensions
    {
        public static void AddDistributedCache(this IServiceCollection services, DistributedCacheOptions cacheOptions)
        {
            switch (cacheOptions.Type.ToLower())
            {
                case "redis":
                    if (string.IsNullOrWhiteSpace(cacheOptions.AzureConnectionString))
                        throw new Exception("Cannot find 'DistributedCache:AzureConnectionString'");

                    services.RemoveAll<IDistributedCache>();

                    //Add distributed cache service backed by Redis cache
                    services.AddStackExchangeRedisCache(options => {
                        var configurationOptions=ConfigurationOptions.Parse(cacheOptions.AzureConnectionString);
                        configurationOptions.ConnectTimeout = cacheOptions.ConnectTimeout;
                        configurationOptions.SyncTimeout = cacheOptions.SyncTimeout;
                        configurationOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                        options.ConfigurationOptions = configurationOptions;
                    });
                    break;
                case "memory":
                    //Use a memory cache
                    services.AddResetableMemoryCache();
                    break;
                default:
                    throw new Exception($"Unrecognised DistributedCache:Type='{cacheOptions.Type}'");
            }
        }

        /// <summary>
        /// Adds a resetable implementation of <see cref="IDistributedCache"/> that stores items in memory
        /// to the <see cref="IServiceCollection" />. Frameworks that require a distributed cache to work
        /// can safely add this dependency as part of their dependency list to ensure that there is at least
        /// one implementation available.
        /// </summary>
        /// <remarks>
        /// <see cref="AddResetableMemoryCache(IServiceCollection)"/> should only be used in single
        /// server scenarios as this cache stores items in memory and doesn't expand across multiple machines.
        /// For those scenarios it is recommended to use a proper distributed cache that can expand across
        /// multiple machines.
        /// </remarks>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddResetableMemoryCache(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();
            services.RemoveAll<IDistributedCache>();
            services.TryAdd(ServiceDescriptor.Singleton<IDistributedCache, ResetableMemoryCache>());

            return services;
        }

    }
}
