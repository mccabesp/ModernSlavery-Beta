using CommandLine;
using Microsoft.Azure.Management.Sql.Fluent.Models;

namespace ModernSlavery.Infrastructure.Azure.SqlServer
{
    [Verb("SqlServer-OpenFirewall", HelpText = "Creates or updates a SQL Server firewall to the current egress IP")]
    public class SqlServerOpenFirewallOptions : SqlServerOptions
    {
        [Option("rule", Required = true, HelpText = "The name of the firewall rule to create/update")]
        public string RuleName { get; set; }
        [Option("start", Required = false, HelpText = "The start IP of the address range")]
        public string StartIP { get; set; }
        [Option("end", Required = false, HelpText = "The end IP of the address range")]
        public string EndIP { get; set; }
    }
}
