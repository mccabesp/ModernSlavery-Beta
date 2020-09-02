using CommandLine;

namespace ModernSlavery.Infrastructure.Azure.KeyVault
{
    [Verb("KeyVault-Import", HelpText = "Import a list of secrets values into the azure key vault from a json file")]
    public class KeyVaultImportOptions : KeyVaultOptions
    {
        [Option('f', Required = true, HelpText = "The filepath of the json import file")]
        public string filepath { get; set; }

        [Option('s', Required = false, HelpText = "The optional section name to import from the file")]
        public string section { get; set; }

    }
}
