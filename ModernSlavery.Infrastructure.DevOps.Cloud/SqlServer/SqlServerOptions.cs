using CommandLine;

namespace ModernSlavery.Infrastructure.Azure.SqlServer
{
    public class SqlServerOptions : AzureOptions
    {
        [Option("server", Required = true, HelpText = "The name of the Sql server")]
        public string Server { get; set; }
    }
}
