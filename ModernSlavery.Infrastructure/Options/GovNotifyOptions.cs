using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.Infrastructure.Message
{
    [Options("Email:Providers:GovNotify")]
    public class GovNotifyOptions:IOptions
    {

        public bool? Enabled { get; set; }

        public string ClientReference { get; set; } = "GpgAlphaTest";

        public string ApiKey { get; set; }

        public string ApiTestKey { get; set; }

    }

}
