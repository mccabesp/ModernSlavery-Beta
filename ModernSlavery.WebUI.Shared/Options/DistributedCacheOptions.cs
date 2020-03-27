using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Options
{
    [Options("DistributedCache")]
    public class DistributedCacheOptions : IOptions
    {
        public string Type { get; set; } = "Memory";
        public string AzureConnectionString { get; set; }
    }
}