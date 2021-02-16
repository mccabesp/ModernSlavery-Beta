using CommandLine;

namespace ModernSlavery.Infrastructure.Azure.Cache
{
    [Verb("RedisCache-Reboot", HelpText = "Reboot specified Redis node(s).")]
    public class RedisCacheRebootOptions : RedisCacheOptions
    {
        public enum RebootTypes
        {
            AllNodes,
            PrimaryNode,
            SecondaryNode
        }
        [Option('n', Required = false, Default = RebootTypes.AllNodes, HelpText = "Which Redis node(s) to reboot. Depending on this value data loss is possible.")]
        public RebootTypes nodes { get; set; }
    }
}
