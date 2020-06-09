using CommandLine;
using ModernSlavery.Infrastructure.DevOps.Cloud.AzureManagerOptions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Infrastructure.DevOps.Cloud.AzureManagerOptions
{
    [Verb("KeyVault-Export", HelpText = "Export a list of secrets values from the azure key vault into a json file")]
    public class KeyVaultExportOptions: IKeyVaultOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }

        public string VaultName { get; set; }

        [Option('f',Required=true, HelpText = "The filepath of the json export file")]
        public string filepath { get; set; }

        [Option('s', Required = false, HelpText = "The optional section name to export from the vault")]
        public string section { get; set; }
    }
}
