using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ModernSlavery.Infrastructure.Caching
{
    public class ResetableMemoryCache : IDistributedCache
    {
        private readonly IOptions<MemoryDistributedCacheOptions> _optionsAccessor;
        private readonly ILoggerFactory _loggerFactory;
        public ResetableMemoryCache(IOptions<MemoryDistributedCacheOptions> optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
            Reset();
        }
        public ResetableMemoryCache(IOptions<MemoryDistributedCacheOptions> optionsAccessor, ILoggerFactory loggerFactory)
        {
            _optionsAccessor = optionsAccessor;
            _loggerFactory = loggerFactory;
            Reset();
        }
        MemoryDistributedCache _memoryCache;

        public byte[] Get(string key)
        {
            return _memoryCache.Get(key);
        }

        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            return _memoryCache.GetAsync(key, token);
        }

        public void Refresh(string key)
        {
            _memoryCache.Refresh(key);
        }

        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return _memoryCache.RefreshAsync(key, token);
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return _memoryCache.RemoveAsync(key, token);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            _memoryCache.Set(key, value, options);
        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return _memoryCache.SetAsync(key, value, options, token);
        }
        public void Reset()
        {
            if (_loggerFactory != null)
                _memoryCache = new MemoryDistributedCache(_optionsAccessor, _loggerFactory);
            else
                _memoryCache = new MemoryDistributedCache(_optionsAccessor);
        }
    }
}
