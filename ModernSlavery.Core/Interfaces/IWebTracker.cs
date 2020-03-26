using System.Threading.Tasks;

namespace ModernSlavery.Core.Interfaces
{
    public interface IWebTracker
    {
        Task SendPageViewTrackingAsync(string title, string url);
    }
}