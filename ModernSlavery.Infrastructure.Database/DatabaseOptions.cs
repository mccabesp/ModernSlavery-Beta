using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Database
{
    [Options("Database")]
    public class DatabaseOptions : IOptions
    {
        private readonly SharedOptions _sharedOptions;
        public DatabaseOptions(SharedOptions sharedOptions)
        {
            _sharedOptions = sharedOptions;
        }

        public string ConnectionString { get; set; } =
            @"Server=(localdb)\ProjectsV13;Initial Catalog=ModernSlaveryDb;Trusted_Connection=True;";

        public string MigrationAppName { get; set; }
        public bool UseMigrations { get; set; }
        public bool EncryptEmails { get; set; } = true;

        /// <summary>
        /// Returns true if the current application os marked to perform migrations
        /// </summary>
        /// <returns></returns>
        public bool GetIsMigrationApp()
        {
            return UseMigrations && (string.IsNullOrWhiteSpace(MigrationAppName) || MigrationAppName.EqualsI(_sharedOptions.ApplicationName));
        }
    }
}