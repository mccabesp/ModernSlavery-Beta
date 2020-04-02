using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Hosts
{
    [Options("DistributedCache")]
    public class DistributedCacheOptions : IOptions
    {
        public string Type { get; set; } = "Memory";
        public string AzureConnectionString { get; set; }
    }
}