using CommandLine;

namespace ModernSlavery.Infrastructure.Azure.KeyVault
{
    public class KeyVaultOptions : AzureOptions
    {
        [Option("vault", Required = true, HelpText = "The name of the azure key vault")]
        public string VaultName { get; set; }
    }
}
