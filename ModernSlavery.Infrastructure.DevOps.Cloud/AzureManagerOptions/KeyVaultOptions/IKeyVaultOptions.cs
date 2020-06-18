using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.Infrastructure.DevOps.Cloud.AzureManagerOptions.Interfaces
{
    public interface IKeyVaultOptions: IAzureOptions
    {
        [Option("vault",Required = true, HelpText = "The name of the azure key vault")]
        public string VaultName { get; set; }
    }
}
