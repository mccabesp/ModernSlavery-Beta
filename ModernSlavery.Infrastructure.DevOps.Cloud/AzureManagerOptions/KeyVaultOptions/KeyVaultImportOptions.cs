using CommandLine;
using ModernSlavery.Infrastructure.DevOps.Cloud.AzureManagerOptions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Infrastructure.DevOps.Cloud.AzureManagerOptions
{
    [Verb("KeyVault-Import", HelpText = "Import a list of secrets values into the azure key vault from a json file")]
    public class KeyVaultImportOptions: IKeyVaultOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }

        public string VaultName { get; set; }

        [Option('f',Required=true, HelpText = "The filepath of the json import file")]
        public string filepath { get; set; }

        [Option('s',Required = false, HelpText = "The optional section name to import from the file")]
        public string section { get; set; }

    }
}
