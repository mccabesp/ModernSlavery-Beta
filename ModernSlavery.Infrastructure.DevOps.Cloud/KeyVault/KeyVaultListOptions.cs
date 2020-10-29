using CommandLine;

namespace ModernSlavery.Infrastructure.Azure.KeyVault
{
    [Verb("KeyVault-List", HelpText = "Export a list of secrets values from the azure key vault into a json file")]
    public class KeyVaultListOptions : KeyVaultOptions
    {
        [Option('s', Required = false, HelpText = "The optional section to list from the vault")]
        public string section { get; set; }
    }
}
