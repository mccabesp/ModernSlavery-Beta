using CommandLine;

namespace ModernSlavery.Infrastructure.Azure.SqlServer
{
    public class SqlDatabaseOptions : AzureOptions
    {
        [Option("db", Required = true, HelpText = "The name of the Sql database")]
        public string Database { get; set; }
    }
}
