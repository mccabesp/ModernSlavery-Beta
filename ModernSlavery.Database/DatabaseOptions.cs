using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.Database
{
    [Options("Database")]

    public class DatabaseOptions:IOptions
    {
        public string ConnectionString { get; set; } = @"Server=(localdb)\ProjectsV13;Initial Catalog=ModernSlaveryDb;Trusted_Connection=True;";
        public bool UseMigrations { get; set; }
        public bool EncryptEmails { get; set; } = true;

    }
}
