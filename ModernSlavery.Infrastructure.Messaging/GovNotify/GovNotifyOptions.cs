using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Messaging.GovNotify
{
    [Options("Email:Providers:GovNotify")]
    public class GovNotifyOptions : IOptions
    {
        public bool? Enabled { get; set; }

        public string ApiServer { get; set; } = "https://api.notifications.service.gov.uk";

        public string ClientReference { get; set; } = "ModernSlaveryBeta";

        public string ApiKey { get; set; }

        public string ApiTestKey { get; set; }
    }
}