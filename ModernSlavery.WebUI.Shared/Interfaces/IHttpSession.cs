using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Shared.Interfaces
{
    public interface IHttpSession
    {
        string SessionID { get; }
        object this[string key] { get; set; }
        IEnumerable<string> Keys { get; }
        void Add(string key, object value);
        void Remove(string key);
        T Get<T>(string key);
        Task LoadAsync();
        Task SaveAsync();
    }
}