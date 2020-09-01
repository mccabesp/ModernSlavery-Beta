using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using ModernSlavery.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Management.Sql.Fluent.Models;

namespace ModernSlavery.Infrastructure.Azure.DevOps
{
    public class SqlManager
    {
        IAzure _azure;
        private IList<ISqlDatabase> _SqlDatabases;
        public IList<ISqlDatabase> SqlDatabases => _SqlDatabases ??= _azure.SqlServers.List().SelectMany(server => server.Databases.List()).ToList();

        private IList<ISqlServer> _SqlServers;
        public IList<ISqlServer> SqlServers => _SqlServers ??= _azure.SqlServers.List().ToList();

        public SqlManager(IAzure azure)
        {
            _azure = azure ?? throw new ArgumentNullException(nameof(azure));
        }

        public ISqlDatabase GetSqlDatabase(string sqlDatabaseName)
        {
            if (string.IsNullOrWhiteSpace(sqlDatabaseName)) throw new ArgumentNullException(nameof(sqlDatabaseName));

            return SqlDatabases.FirstOrDefault(w => w.Name.EqualsI(sqlDatabaseName));
        }
        public ISqlServer GetSqlServer(string sqlServerName)
        {
            if (string.IsNullOrWhiteSpace(sqlServerName)) throw new ArgumentNullException(nameof(sqlServerName));
            return SqlServers.FirstOrDefault(w => w.Name.EqualsI(sqlServerName));
        }
        public ISqlServer GetSqlServer(ISqlDatabase sqlDatabase)
        {
            if (sqlDatabase == null) throw new ArgumentNullException(nameof(sqlDatabase));

            var sqlServer = SqlServers.FirstOrDefault(server => server.Name.EqualsI(sqlDatabase.SqlServerName));
            return sqlServer;
        }

        #region Vertical scaling
        public void SetDatabaseEdition(string databaseName, DatabaseEdition newDatabaseEdition)
        {
            var sqlDatabase = GetSqlDatabase(databaseName);
            if (sqlDatabase == null) throw new ArgumentNullException(nameof(sqlDatabase), $"Cannot find Sql database '{databaseName}'");

            SetDatabaseEdition(sqlDatabase, newDatabaseEdition);
        }

        public void SetDatabaseEdition(ISqlDatabase sqlDatabase, DatabaseEdition newDatabaseEdition)
        {
            if (sqlDatabase == null) throw new ArgumentNullException(nameof(sqlDatabase));
            if (newDatabaseEdition == null) throw new ArgumentNullException(nameof(newDatabaseEdition));

            if (sqlDatabase.Edition.Value != newDatabaseEdition.Value) sqlDatabase.Update().WithEdition(newDatabaseEdition).Apply();
        }
        #endregion


        public ISqlFirewallRule GetFireWallRule(ISqlServer sqlServer, string ruleName)
        {
            if (sqlServer == null) throw new ArgumentNullException(nameof(sqlServer));
            if (string.IsNullOrWhiteSpace(ruleName)) throw new ArgumentNullException(nameof(ruleName));

            return sqlServer.FirewallRules.List().FirstOrDefault(r => r.Name.EqualsI(ruleName));
        }

        public ISqlFirewallRule OpenFirewall(string sqlServerName, string ruleName, string startIP=null, string endIP = null)
        {
            if (string.IsNullOrWhiteSpace(sqlServerName)) throw new ArgumentNullException(nameof(sqlServerName));
            if (string.IsNullOrWhiteSpace(ruleName)) throw new ArgumentNullException(nameof(ruleName));

            var sqlServer = GetSqlServer(sqlServerName);
            if (sqlServer == null) throw new ArgumentNullException(nameof(sqlServer), $"Cannot find Sql server '{sqlServerName}'");

            var firewallRule = sqlServer.FirewallRules.Get(ruleName);
            if (firewallRule != null) firewallRule.Delete();

            if (!string.IsNullOrWhiteSpace(endIP) && !endIP.Equals(startIP))
            {
                firewallRule = sqlServer.FirewallRules.Define(ruleName).WithIPAddressRange(startIP, endIP).Create();
                Console.WriteLine($"Open Firewall: SqlServer: '{sqlServerName}', Rule: '{ruleName}', IP Range: {startIP}-{endIP} opened successfully");
            }
            else if (!string.IsNullOrWhiteSpace(endIP))
                throw new ArgumentNullException(nameof(startIP), $"Missing StartIP address in range");
            else
            {
                if (string.IsNullOrWhiteSpace(startIP)) startIP = Networking.GetEgressIPAddressAsync().Result; ;
                firewallRule = sqlServer.FirewallRules.Define(ruleName).WithIPAddress(startIP).Create();
                Console.WriteLine($"Open Firewall: SqlServer: '{sqlServerName}', Rule: '{ruleName}', IP: {startIP} opened successfully");
            }

            return firewallRule;
        }

        public ISqlFirewallRule DeleteFirewall(string sqlServerName, string ruleName)
        {
            if (string.IsNullOrWhiteSpace(sqlServerName)) throw new ArgumentNullException(nameof(sqlServerName));
            if (string.IsNullOrWhiteSpace(ruleName)) throw new ArgumentNullException(nameof(ruleName));

            var sqlServer = GetSqlServer(sqlServerName);
            if (sqlServer == null) throw new ArgumentNullException(nameof(sqlServer), $"Cannot find Sql server '{sqlServerName}'");

            var firewallRule = sqlServer.FirewallRules.Get(ruleName);
            if (firewallRule == null)
                Console.WriteLine($"Delete Firewall: SqlServer: '{sqlServerName}', Rule: '{ruleName}' already deleted");
            else
            {
                firewallRule.Delete();
                Console.WriteLine($"Delete Firewall: SqlServer: '{sqlServerName}', Rule: '{ruleName}' deleted successfully");
            }
            return firewallRule;
        }

    }
}
