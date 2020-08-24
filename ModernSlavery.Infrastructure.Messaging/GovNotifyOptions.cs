using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Messaging
{
    [Options("Email:Providers:GovNotify")]
    public class GovNotifyOptions : IOptions
    {
        public bool? Enabled { get; set; }

        public string ApiServer { get; set; } = "https://api.notifications.service.gov.uk";

        public string ClientReference { get; set; } = "ModernSlaveryAlphaTest";

        public string ApiKey { get; set; }

        public string ApiTestKey { get; set; }

        public bool AllowTestKeyOnly { get; set; }
    }
}