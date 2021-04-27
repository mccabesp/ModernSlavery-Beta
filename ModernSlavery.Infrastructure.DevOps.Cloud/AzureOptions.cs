using CommandLine;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Azure
{
    public class AzureOptions
    {
        [Option("client", Required = false, HelpText = "The ClientId of the Azure App Registration")]
        public string ClientId { get; set; }

        [Option("secret", Required = false, HelpText = "The ClientSecret of the Azure App Registration")]
        public string ClientSecret { get; set; }

        [Option("tenant", Required = false, HelpText = "The Id of the Azure tenant to connect to. If missing the default subscription is used")]
        public string TenantId { get; set; }

        [Option("sub", Required = false, HelpText = "The Id of the Azure subscription to connect to. If missing the default subscription is used")]
        public string SubscriptionId { get; set; }

        public bool HasCredentials()
        {
            return !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret) && !string.IsNullOrWhiteSpace(TenantId);
        }
    }
}
