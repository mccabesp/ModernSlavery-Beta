using System.Net.Http;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Shared.Interfaces
{
    public interface IWebTracker
    {

        Task<HttpResponseMessage> SendPageViewTrackingAsync(string title, string url);

    }
}
