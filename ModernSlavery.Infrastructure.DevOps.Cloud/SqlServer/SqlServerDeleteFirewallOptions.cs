using CommandLine;
using Microsoft.Azure.Management.Sql.Fluent.Models;

namespace ModernSlavery.Infrastructure.Azure.SqlServer
{
    [Verb("SqlServer-DeleteFirewall", HelpText = "Deletes an SQL Server firewall rule")]
    public class SqlServerDeleteFirewallOptions : SqlServerOptions
    {
        [Option("rule", Required = true, HelpText = "The name of the firewall rule to delete")]
        public string RuleName { get; set; }
    }
}
