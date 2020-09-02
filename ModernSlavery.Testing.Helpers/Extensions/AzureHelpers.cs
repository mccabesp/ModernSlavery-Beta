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

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class AzureHelpers
    {
        private static AzureManager _azureManager = null;
        private static IConfiguration _config=null;
        private static SqlManager _sqlManager = null;
        private static string _sqlServerName = null;
        private static string _sqlFirewallRuleName = null;
        private static KeyVaultManager _keyVaultManager = null;

        private static IAzure Initialise(this IHost host)
        {
            if (_azureManager != null && AzureManager.Azure!=null) return AzureManager.Azure;
            _azureManager = new AzureManager();

            _config = host.Services.GetRequiredService<IConfiguration>();

            var clientId = _config["ClientId"];
            var clientSecret = _config["ClientSecret"];
            var tenantId = _config["TenantId"];
            var _azure = _azureManager.Authenticate(clientId, clientSecret, tenantId);

            return _azure;
        }

        #region SQL Firewall
        /// <summary>
        /// Opens the firewall to the current SQL server for the current build agent
        /// </summary>
        /// <param name="host">The webhost</param>
        public static void OpenSQLFirewall(this IHost host)
        {
            var azure = host.Initialise();

            SqlManager sqlManager = _sqlManager;
            if (sqlManager == null)
            {
                sqlManager = new SqlManager(azure);

                var connectionString = _config["Database:ConnectionString"];
                var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);

                var sqlServerName = connectionStringBuilder.DataSource.BeforeFirst(".database.windows.net", StringComparison.OrdinalIgnoreCase, false, true).AfterFirst("tcp:", StringComparison.OrdinalIgnoreCase, false, true);
                if (sqlServerName.ContainsI("localdb")) return;

                _sqlServerName = sqlServerName;
                _sqlFirewallRuleName = $"TESTAGENT_{_config.GetValue("AGENT_NAME", Environment.MachineName)}";
            }
            sqlManager.OpenFirewall(_sqlServerName, _sqlFirewallRuleName);
            _sqlManager = sqlManager;
        }

        /// <summary>
        /// Deletes the firewall to the current SQL server for the current build agent
        /// </summary>
        /// <param name="host">The webhost</param>
        public static void CloseSQLFirewall(this IHost host)
        {
            _sqlManager?.DeleteFirewall(_sqlServerName, _sqlFirewallRuleName);
        }
        #endregion

        #region KeyVault
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

            var keyVaultManager = _keyVaultManager;
            if (keyVaultManager == null) keyVaultManager = new KeyVaultManager(azure, vaultName); var value=keyVaultManager.GetValue(key);
            _keyVaultManager = keyVaultManager;
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

            var keyVaultManager = _keyVaultManager;
            if (keyVaultManager == null)keyVaultManager = new KeyVaultManager(azure,vaultName);
            keyVaultManager.SetValue(key,value);
            _keyVaultManager = keyVaultManager;
        }
        #endregion
    }
}
