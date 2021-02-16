using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Caching
{
    [Options("DistributedCache")]
    public class DistributedCacheOptions : IOptions
    {
        public string Type { get; set; } = "Memory";
        public string AzureConnectionString { get; set; }

        /// <summary>
        /// Specifies the time in milliseconds that should be allowed for connection (defaults to 15 seconds unless SyncTimeout is higher)
        /// </summary>
        public int ConnectTimeout { get; set; } = 15000;

        /// <summary>
        /// Specifies the time in milliseconds that the system should allow for synchronous operations (defaults to 15 seconds)
        /// </summary>
        public int SyncTimeout { get; set; } = 15000;
    }
}