using Microsoft.Azure.Management.BatchAI.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.KeyVault.Fluent;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Extensions.Configuration;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Infrastructure.Configuration;
using ModernSlavery.Infrastructure.DevOps.Cloud.AzureManagerOptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ModernSlavery.Infrastructure.DevOps.Cloud.AzureResourceManagers
{
    public class KeyVaultManager
    {
        IAzure _azure;
        IVault _vault;
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
        }

        public string GetValue(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var secret = _vault.Secrets.GetByName(name);
            return secret?.Value;
        }

        public void SetValue(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));

            var secret = _vault.Secrets.GetByName(name);
            if (secret == null)
                _vault.Secrets.Define(name).WithValue(value);
            else
                secret.Update().WithValue(value);
        }

        public Dictionary<string, string> GetValues(string sectionName = null)
        {
            var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            sectionName = sectionName?.Replace("--", ":");
            _vault.Secrets.List().ForEach(secret =>
            {
                var newKey = secret.Name.Replace("--", ":");
                var versions = secret.ListVersions().ToList();
                if (string.IsNullOrWhiteSpace(sectionName) || newKey.StartsWithI(sectionName))
                    settings[newKey] = secret.ListVersions().FirstOrDefault()?.Value;
            });

            return settings;
        }

        public string ImportValues(string filepath, string sectionName=null)
        {
            var settings = Configuration.Extensions.LoadSettings(filepath,sectionName);

            settings.Keys.ForEach(key =>
            {
                var newKey = key.Replace(":", "--");
                var secret = _vault.Secrets.GetByName(newKey);
                if (secret == null)
                    _vault.Secrets.Define(newKey).WithValue(settings[key]);
                else
                    secret.Update().WithValue(settings[key]);
            });
            return null;
        }
        public void ExportValues(string vaultName, string filepath, string sectionName = null)
        {
            if (string.IsNullOrWhiteSpace(filepath)) throw new ArgumentNullException(nameof(filepath));

            var settings = GetValues(sectionName);

            settings.SaveSettings(filepath);
        }
    }
}
