using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.KeyVault.Fluent;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Infrastructure.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModernSlavery.Infrastructure.Azure.KeyVault
{
    public class KeyVaultManager
    {
        IAzure _azure;
        IVault _vault;

        public readonly string VaultName;

        public KeyVaultManager(IAzure azure, string vaultName, string resourceGroup = null)
        {
            _azure = azure ?? throw new ArgumentNullException(nameof(azure));

            if (string.IsNullOrWhiteSpace(vaultName)) throw new ArgumentNullException(nameof(vaultName));

            if (string.IsNullOrWhiteSpace(resourceGroup))
            {
                List<IVault> vaults = _azure.Vaults.List().Where(s => s.Name.EqualsI(vaultName)).ToList();
                if (vaults.Count > 1)
                {
                    throw new ArgumentNullException(
                        nameof(resourceGroup),
                        $"You must specify a resourceGroup as server {vaultName} exists in {vaults.Count} resource groups");
                }

                _vault = vaults.FirstOrDefault();
            }
            else
            {
                _vault = _azure.Vaults.GetByResourceGroup(resourceGroup, vaultName);
            }
            if (_vault == null) throw new ArgumentException($"Cannot find vault '{vaultName}'", nameof(vaultName));
            VaultName = vaultName;
        }

        private ISecret GetSecret(string name, IList<ISecret> secrets = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            name = name.Replace(":", "--");
            secrets = secrets ?? _vault.Secrets.List().ToList();
            return secrets.FirstOrDefault(s => s.Name == name);
        }

        public string GetValue(string name, IList<ISecret> secrets = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var secret = GetSecret(name, secrets);
            var latestValue = secret?.ListVersions().LastOrDefault();
            return latestValue?.Value;
        }

        public ISecret SetValue(string name, string value, IList<ISecret> secrets = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));


            name = name.Replace(":", "--");
            secrets = secrets ?? _vault.Secrets.List().ToList();
            var secret = GetSecret(name, secrets);

            if (secret == null)
                return _vault.Secrets.Define(name).WithValue(value).Create();
            else if (GetValue(name) != value)
                return secret.Update().WithValue(value).Apply();

            return null;
        }

        public Dictionary<string, string> GetValues(string sectionName = null, IList<ISecret> secrets = null)
        {
            var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            secrets = secrets ?? _vault.Secrets.List().ToList();

            sectionName = sectionName?.Replace("--", ":");
            secrets.ForEach(secret =>
            {
                var newKey = secret.Name.Replace("--", ":");
                if (string.IsNullOrWhiteSpace(sectionName) || newKey.StartsWithI(sectionName))
                    settings[newKey] = GetValue(secret.Name);
            });

            return settings;
        }

        public string ImportValues(string filepath, string sectionName = null)
        {
            if (string.IsNullOrWhiteSpace(filepath)) throw new ArgumentNullException(nameof(filepath));

            filepath = FileSystem.ExpandLocalPath(filepath);
            if (!System.IO.File.Exists(filepath)) throw new FileNotFoundException("Cannot find file", filepath);

            var settings = Configuration.Extensions.LoadSettings(filepath, sectionName);

            var secrets = _vault.Secrets.List().ToList();

            settings.Keys.ForEach(name =>
            {
                var secret = SetValue(name, settings[name], secrets);
                if (secret != null) Console.WriteLine($"Imported secret '{name}'");
            });

            return null;
        }
        public void ExportValues(string vaultName, string filepath, string sectionName = null)
        {
            if (string.IsNullOrWhiteSpace(filepath)) throw new ArgumentNullException(nameof(filepath));

            filepath = FileSystem.ExpandLocalPath(filepath);

            var settings = GetValues(sectionName);

            settings.SaveSettings(filepath);
        }


    }
}
