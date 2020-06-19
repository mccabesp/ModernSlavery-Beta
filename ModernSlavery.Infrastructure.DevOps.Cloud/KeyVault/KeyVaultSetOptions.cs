using CommandLine;

namespace ModernSlavery.Infrastructure.Azure.KeyVault
{
    [Verb("KeyVault-Set", HelpText = "Set the value of a secret in the azure key vault")]
    public class KeyVaultSetOptions : KeyVaultOptions
    {
        [Option('k', Required = true, HelpText = "The unique key of the secret to set")]
        public string key { get; set; }

        [Option('v', Required = true, HelpText = "The new value of the secret")]
        public string value { get; set; }
    }
}
