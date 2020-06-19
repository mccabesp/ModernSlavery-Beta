using CommandLine;

namespace ModernSlavery.Infrastructure.Azure.KeyVault
{
    [Verb("KeyVault-Export", HelpText = "Export a list of secrets values from the azure key vault into a json file")]
    public class KeyVaultExportOptions : KeyVaultOptions
    {
        [Option('f', Required = true, HelpText = "The filepath of the json export file")]
        public string filepath { get; set; }

        [Option('s', Required = false, HelpText = "The optional section name to export from the vault")]
        public string section { get; set; }
    }
}
