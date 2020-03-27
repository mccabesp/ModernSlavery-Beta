using System;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Shared.Interfaces
{
    public interface IHttpCache
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value);

        Task AddAsync(string key, object value, DateTime? absoluteExpiration = null,
            TimeSpan? slidingExpiration = null);

        Task RemoveAsync(string key);
    }
}