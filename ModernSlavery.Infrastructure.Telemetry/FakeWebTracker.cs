using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.Infrastructure.Telemetry
{
    public class FakeWebTracker : IWebTracker
    {
        public async Task SendPageViewTrackingAsync(string title, string url)
        {
        }
    }
}