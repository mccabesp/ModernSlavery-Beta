using CommandLine;
using ModernSlavery.Infrastructure.DevOps.Cloud.AzureManagerOptions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Infrastructure.DevOps.Cloud.AzureManagerOptions
{
    [Verb("KeyVault-Set", HelpText = "Set the value of a secret in the azure key vault")]
    public class KeyVaultSetOptions: IKeyVaultOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Subscription { get; set; }
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }

        public string VaultName { get; set; }

        [Option('k',Required=true, HelpText = "The unique key of the secret to set")]
        public string key { get; set; }

        [Option('v',Required = true, HelpText = "The new value of the secret")]
        public string value { get; set; }
    }
}
