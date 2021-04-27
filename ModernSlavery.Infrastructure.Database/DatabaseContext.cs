using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Database
{
    public partial class DatabaseContext : DbContext, IDbContext
    {
        public static string ConnectionString =
            @"Server=(localdb)\ProjectsV13;Initial Catalog=ModernSlaveryDb;Trusted_Connection=True;";

        private readonly DatabaseOptions _databaseOptions;
        private readonly SharedOptions _sharedOptions;
        private static bool _migrationEnsured;
        public bool MigrationsApplied { get; }
        //private static bool _encryptionInitialised;

        public DatabaseContext(SharedOptions sharedOptions, DatabaseOptions databaseOptions)
        {
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _databaseOptions = databaseOptions ?? throw new ArgumentNullException(nameof(databaseOptions));

            if (!string.IsNullOrWhiteSpace(_databaseOptions.ConnectionString))
                ConnectionString = _databaseOptions.ConnectionString;

            //TODO: Not tested or fully implemented yet
            //    InitializeAzureKeyVaultProvider();

            if (databaseOptions.GetIsMigrationApp())
                MigrationsApplied=EnsureMigrated();
        }
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        private bool EnsureMigrated()
        {
            if (_migrationEnsured)
                return MigrationsApplied; //This static variable is a temporary measure otherwise each request for a Database context takes a few seconds to check for migrations or if the database exists
            
            Database.Migrate();
            _migrationEnsured = true;

            var lastDefinedMigration = Database.GetMigrations().LastOrDefault();
            var lastAppliedMigration = Database.GetAppliedMigrations().LastOrDefault();

            return lastAppliedMigration == lastDefinedMigration;
        }


        public async Task<int> SaveChangesAsync()
        {
            #region Validate the new or changed entities

            var entities = from e in ChangeTracker.Entries()
                where e.State == EntityState.Added
                      || e.State == EntityState.Modified
                select e.Entity;

            var innerExceptions = new List<ValidationException>();
            foreach (var entity in entities)
            {
                var validationContext = new ValidationContext(entity);

                try
                {
                    Validator.ValidateObject(entity, validationContext, true);
                }
                catch (ValidationException vex)
                {
                    innerExceptions.Add(vex);
                }
            }

            if (innerExceptions.Any()) throw new AggregateException(innerExceptions);

            #endregion

            return await base.SaveChangesAsync().ConfigureAwait(false);
        }

        public DatabaseFacade GetDatabase()
        {
            return Database;
        }

        public new DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }

        public async Task BulkInsertAsync<TEntity>(IEnumerable<TEntity> entities, bool setOutputIdentity=false, int batchSize = 2000, int? timeout = null) where TEntity : class
        {
            int? previousTimeout = null;
            if (timeout != null)
            {
                previousTimeout = Database.GetCommandTimeout();
                Database.SetCommandTimeout(timeout);
            }
            await this.BulkInsertAsync(entities.ToList(), b=> { b.SetOutputIdentity = setOutputIdentity; b.PreserveInsertOrder = setOutputIdentity; b.BatchSize = batchSize; }).ConfigureAwait(false);

            if (timeout != null) Database.SetCommandTimeout(previousTimeout);
        }

        public async Task BulkDeleteAsync<TEntity>(IEnumerable<TEntity> entities, int batchSize = 2000, int? timeout = null) where TEntity : class
        {
            int? previousTimeout = null;
            if (timeout != null)
            {
                previousTimeout = Database.GetCommandTimeout();
                Database.SetCommandTimeout(timeout);
            }
            await this.BulkDeleteAsync(entities.ToList(), b => { b.BatchSize = batchSize; }).ConfigureAwait(false);

            if (timeout != null) Database.SetCommandTimeout(previousTimeout);
        }

        public async Task BulkUpdateAsync<TEntity>(IEnumerable<TEntity> entities, int batchSize = 2000, int? timeout = null) where TEntity : class
        {
            int? previousTimeout = null;
            if (timeout != null)
            {
                previousTimeout = Database.GetCommandTimeout();
                Database.SetCommandTimeout(timeout);
            }
            await this.BulkUpdateAsync(entities.ToList(), b => { b.BatchSize = batchSize; }).ConfigureAwait(false);

            if (timeout != null) Database.SetCommandTimeout(previousTimeout);
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#if DEBUG || DEBUGLOCAL
                #region On local development machines add migration version to database name
                if (_sharedOptions.IsDevelopment() || _sharedOptions.IsTest())
                {
                    var connectionString = new SqlConnectionStringBuilder(ConnectionString);
                    if (connectionString.DataSource.ContainsI("(localdb") && connectionString.InitialCatalog.ContainsI("-V?"))
                    {
                        var migrationClasses = Assembly.GetExecutingAssembly().ExportedTypes.Where(p => p.IsClass && typeof(Migration).IsAssignableFrom(p)).ToList();
                        connectionString.InitialCatalog = $"{connectionString.InitialCatalog.BeforeLast("-V?")}-V{migrationClasses.Count}";
                        ConnectionString = connectionString.ToString();
                    }
                }
                #endregion
#endif
                //Setup the SQL server with automatic retry on failure
                optionsBuilder.UseSqlServer(ConnectionString, options => options.EnableRetryOnFailure());
            }

            //Use lazy loading for related virtual items
            optionsBuilder.UseLazyLoadingProxies();
        }

        #region Column Encryption
        //private bool _encryptionInitialised=false;
        //private void InitializeAzureKeyVaultProvider()
        //{
        //    if (_encryptionInitialised) return;
        //    SqlColumnEncryptionAzureKeyVaultProvider azureKeyVaultProvider;
        //    if (!string.IsNullOrWhiteSpace(_sharedOptions.ClientId) || !string.IsNullOrWhiteSpace(_sharedOptions.ClientSecret))
        //    {
        //        azureKeyVaultProvider = new SqlColumnEncryptionAzureKeyVaultProvider(GetToken);
        //    }
        //    else
        //    {
        //        var azureServiceTokenProvider = new AzureServiceTokenProvider();
        //        azureKeyVaultProvider = new SqlColumnEncryptionAzureKeyVaultProvider(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
        //    }

        //    var providers = new Dictionary<string, SqlColumnEncryptionKeyStoreProvider>();
        //    providers.Add(SqlColumnEncryptionAzureKeyVaultProvider.ProviderName, azureKeyVaultProvider);
        //    SqlConnection.RegisterColumnEncryptionKeyStoreProviders(providers);
        //    _encryptionInitialised = true;
        //}

        //private async Task<string> GetToken(string authority, string resource, string scope)
        //{
        //    var appCredentials = new ClientCredential(_sharedOptions.ClientId, _sharedOptions.ClientSecret);
        //    var context = new AuthenticationContext(authority, TokenCache.DefaultShared);

        //    var result = await context.AcquireTokenAsync(resource, appCredentials);
        //    if (result == null) throw new InvalidOperationException("Failed to obtain the access token");

        //    return result.AccessToken;
        //}
        #endregion

        #region Tables

        public virtual DbSet<AddressStatus> AddressStatus { get; set; }
        public virtual DbSet<Organisation> Organisation { get; set; }
        public virtual DbSet<OrganisationAddress> OrganisationAddress { get; set; }
        public virtual DbSet<OrganisationName> OrganisationName { get; set; }
        public virtual DbSet<OrganisationReference> OrganisationReference { get; set; }
        public virtual DbSet<OrganisationScope> OrganisationScope { get; set; }
        public virtual DbSet<OrganisationSicCode> OrganisationSicCode { get; set; }
        public virtual DbSet<OrganisationStatus> OrganisationStatus { get; set; }
        public virtual DbSet<SicCode> SicCodes { get; set; }
        public virtual DbSet<SicSection> SicSections { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserOrganisation> UserOrganisations { get; set; }
        public virtual DbSet<UserSetting> UserSettings { get; set; }
        public virtual DbSet<UserStatus> UserStatuses { get; set; }
        public virtual DbSet<PublicSectorType> PublicSectorTypes { get; set; }
        public virtual DbSet<OrganisationPublicSectorType> OrganisationPublicSectorTypes { get; set; }
        public virtual DbSet<Feedback> Feedback { get; set; }
        public virtual DbSet<AuditLog> AuditLogs { get; set; }
        public virtual DbSet<ReminderEmail> ReminderEmails { get; set; }
        public virtual DbSet<Statement> Statements { get; set; }
        public virtual DbSet<StatementOrganisation> StatementOrganisations { get; set; }
        public virtual DbSet<StatementSectorType> StatementSectorTypes { get; set; }
        public virtual DbSet<StatementSector> StatementSectors { get; set; }
        public virtual DbSet<StatementStatus> StatementStatuses { get; set; }

        #endregion
    }
}