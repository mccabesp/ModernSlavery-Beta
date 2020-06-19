using CommandLine;
using Microsoft.Azure.Management.Sql.Fluent.Models;

namespace ModernSlavery.Infrastructure.Azure.SqlServer
{
    [Verb("SqlDatabase-SetEdition", HelpText = "Set the pricing edition the Sql database")]
    public class SqlDatabasetSetEditionOptions : SqlDatabaseOptions
    {
        [Option("edition", Required = true, HelpText = "The pricing edition to set")]
        public DatabaseEdition DatabaseEdition { get; set; }
    }
}
