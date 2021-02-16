using CommandLine;

namespace ModernSlavery.Infrastructure.Azure.Cache
{
    public class RedisCacheOptions : AzureOptions
    {
        [Option("cache", Required = true, HelpText = "The name of the azure redis cache")]
        public string CacheName { get; set; }
    }
}
