using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Infrastructure.Azure;
using ModernSlavery.Infrastructure.Azure.DevOps;
using System;
using ModernSlavery.Core.Extensions;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ModernSlavery.Infrastructure.Azure.KeyVault;
using Microsoft.Azure.Management.Sql.Fluent.Models;
using Microsoft.Azure.Management.Sql.Fluent;
using SqlManager = ModernSlavery.Infrastructure.Azure.DevOps.SqlManager;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class AzureHelpers
    {
        private static AzureManager _azureManager = null;
        private static IConfiguration _config = null;
        private static SqlManager _sqlManager = null;
        private static string _sqlServerName = null;
        private static string _sqlDatabaseName = null;
        private static string _sqlFirewallRuleName = null;
        private static KeyVaultManager _keyVaultManager = null;

        private static IAzure Initialise(this IHost host)
        {
            if (_azureManager != null && AzureManager.Azure != null) return AzureManager.Azure;

            _config = host.Services.GetRequiredService<IConfiguration>();

            var clientId = _config["ClientId"];
            if (string.IsNullOrWhiteSpace(clientId)) return null;

            var clientSecret = _config["ClientSecret"];
            var tenantId = _config["TenantId"];

            _azureManager = new AzureManager();
            var _azure = _azureManager.Authenticate(clientId, clientSecret, tenantId);

            return _azure;
        }

        private static bool InitialiseSql(this IAzure azure)
        {
            SqlManager sqlManager = _sqlManager;
            if (sqlManager == null)
            {
                sqlManager = new SqlManager(azure);

                var connectionString = _config["Database:ConnectionString"];
                var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

                var sqlServerName = connectionStringBuilder.DataSource.BeforeFirst(".database.windows.net", StringComparison.OrdinalIgnoreCase, false, true).AfterFirst("tcp:", StringComparison.OrdinalIgnoreCase, false, true);
                if (sqlServerName.ContainsI("localdb")) return false;

                _sqlManager = sqlManager;
                _sqlServerName = sqlServerName;
                _sqlDatabaseName = connectionStringBuilder.InitialCatalog;
            }
            return true;
        }

        #region SQL Firewall
        /// <summary>
        /// Opens the firewall to the current SQL server for the current build agent
        /// </summary>
        /// <param name="host">The webhost</param>
        public static void OpenSQLFirewall(this IHost host)
        {
            var azure = host.Initialise();

            if (azure==null || !azure.InitialiseSql()) return;

            var sqlFirewallRuleName = $"TESTAGENT_{_config.GetValue("AGENT_NAME", Environment.MachineName)}";
            _sqlManager.OpenFirewall(_sqlServerName, sqlFirewallRuleName);
            _sqlFirewallRuleName = sqlFirewallRuleName;
        }

        /// <summary>
        /// Deletes the firewall to the current SQL server for the current build agent
        /// </summary>
        /// <param name="host">The webhost</param>
        public static void CloseSQLFirewall()
        {
            if (!string.IsNullOrWhiteSpace(_sqlServerName) && !string.IsNullOrWhiteSpace(_sqlFirewallRuleName))_sqlManager?.DeleteFirewall(_sqlServerName, _sqlFirewallRuleName);
        }
        #endregion

        public static void SetSqlDatabaseEdition(this IHost host, DatabaseEdition newDatabaseEdition)
        {
            var azure = host.Initialise();

            if (!azure.InitialiseSql()) return;

            _sqlManager.SetDatabaseEdition(_sqlDatabaseName, newDatabaseEdition);
        }

        #region KeyVault
        private static bool InitialiseKeyVault(this IAzure azure)
        {
            var vaultName = _config["Vault"];
            if (string.IsNullOrWhiteSpace(vaultName)) return false;
            if (_keyVaultManager == null || !_keyVaultManager.VaultName.EqualsI(vaultName)) _keyVaultManager = new KeyVaultManager(azure, vaultName);
            return true;
        }

        /// <summary>
        /// Gets a secret value from the keyVault
        /// </summary>
        /// <param name="host">The webhost</param>
        /// <param name="key">The name of the secret to retreive</param>
        public static string GetKeyVaultSecret(this IHost host, string vaultName, string key)
        {
            if (string.IsNullOrWhiteSpace(vaultName)) throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            var azure = host.Initialise();
            if (!azure.InitialiseKeyVault()) throw new Exception($"Could not create KeyVaultManager for vault '{vaultName}'");

            var value = _keyVaultManager.GetValue(key);
            return value;
        }

        /// <summary>
        /// Sets a secret value in the keyVault
        /// </summary>
        /// <param name="host">The webhost</param>
        /// <param name="key">The name of the secret</param>
        /// <param name="host">The value of the secret to set</param>
        public static void SetKeyVaultSecret(this IHost host, string vaultName, string key, string value)
        {
            if (string.IsNullOrWhiteSpace(vaultName)) throw new ArgumentNullException(nameof(vaultName));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            var azure = host.Initialise();
            if (!azure.InitialiseKeyVault()) throw new Exception($"Could not create KeyVaultManager for vault '{vaultName}'");

            _keyVaultManager.SetValue(key, value);
        }
        #endregion
    }
}
