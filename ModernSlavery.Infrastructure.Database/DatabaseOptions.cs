using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Database
{
    [Options("Database")]
    public class DatabaseOptions : IOptions
    {
        public string ConnectionString { get; set; } =
            @"Server=(localdb)\ProjectsV13;Initial Catalog=ModernSlaveryDb;Trusted_Connection=True;";

        public bool UseMigrations { get; set; }
        public bool EncryptEmails { get; set; } = true;
    }
}