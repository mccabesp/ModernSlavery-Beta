using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.Infrastructure.Messaging
{
    [Options("Email:Providers:GovNotify")]
    public class GovNotifyOptions : IOptions
    {
        public bool? Enabled { get; set; }

        public string ClientReference { get; set; } = "ModernSlaveryAlphaTest";

        public string ApiKey { get; set; }

        public string ApiTestKey { get; set; }
    }
}