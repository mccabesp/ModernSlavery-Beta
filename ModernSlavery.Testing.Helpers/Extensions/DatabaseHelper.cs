using EFCore.BulkExtensions;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Extensions;
using System.Threading;
using Microsoft.Extensions.Configuration;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using System.Threading.Tasks;
using ModernSlavery.Testing.Helpers.Classes;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class DatabaseHelper
    {
        private const string LastFullDatabaseResetKey = "LastFullDatabaseReset";
        private static readonly int? CommandTimeout=500;

        /// <summary>
        /// Deletes all tables in the database (except _EFMigrationsHistory) and reimports all seed data
        /// </summary>
        /// <param name="host"></param>
        public static async System.Threading.Tasks.Task ResetDatabaseAsync(this IHost host, bool force = true)
        {
            var config = host.Services.GetRequiredService<IConfiguration>();
            var vaultName = config["Vault"];

            var dataRepository = host.Services.GetRequiredService<IDataRepository>();

            if (!force)
            {
                var lastFullDatabaseReset = string.IsNullOrWhiteSpace(vaultName) ? RegistryHelper.GetRegistryKey(LastFullDatabaseResetKey).ToDateTime() : host.GetKeyVaultSecret(vaultName, LastFullDatabaseResetKey).ToDateTime();
                if (lastFullDatabaseReset > DateTime.MinValue && lastFullDatabaseReset.Date==DateTime.Now.Date)
                {
                    dataRepository.ResetDatabase(lastFullDatabaseReset);
                    return;
                }
            }
            
            dataRepository.SetCommandTimeout(CommandTimeout);//Give the batch command 5 mins to timout - required on Basic SQL tier
            dataRepository.GetAll<AuditLog>().BatchDelete();
            dataRepository.GetAll<Feedback>().BatchDelete();
            dataRepository.GetAll<Organisation>().BatchUpdate(o => new Organisation { LatestAddressId = null, LatestStatementId = null, LatestPublicSectorTypeId = null, LatestRegistrationOrganisationId = null, LatestRegistrationUserId = null, LatestScopeId = null });
            dataRepository.GetAll<StatementOrganisation>().BatchUpdate(o => new StatementOrganisation { OrganisationId = null });
            dataRepository.GetAll<UserOrganisation>().BatchDelete();
            dataRepository.GetAll<Organisation>().BatchDelete();
            dataRepository.GetAll<ReminderEmail>().BatchDelete();
            dataRepository.GetAll<User>().BatchDelete();
            dataRepository.GetAll<PublicSectorType>().BatchDelete();
            dataRepository.GetAll<SicCode>().BatchDelete();
            dataRepository.GetAll<SicSection>().BatchDelete();
            dataRepository.GetAll<StatementDiligenceType>().BatchDelete();
            dataRepository.GetAll<StatementPolicyType>().BatchDelete();
            dataRepository.GetAll<StatementRiskType>().BatchDelete();
            dataRepository.GetAll<StatementTrainingType>().BatchDelete();

            var dataImporter = host.Services.GetService<IDataImporter>();
            dataImporter.ImportSICSectionsAsync().Wait();
            dataImporter.ImportSICCodesAsync().Wait();
            dataImporter.ImportStatementDiligenceTypesAsync().Wait();
            dataImporter.ImportStatementPolicyTypesAsync().Wait();
            dataImporter.ImportStatementRiskTypesAsync().Wait();
            dataImporter.ImportStatementSectorTypesAsync().Wait();
            dataImporter.ImportStatementTrainingTypesAsync().Wait();
            dataImporter.ImportPrivateOrganisationsAsync(-1, 100).Wait();
            dataImporter.ImportPublicOrganisationsAsync(-1, 100).Wait();

            var organisationBusinessLogic = host.Services.GetRequiredService<IOrganisationBusinessLogic>();
            await organisationBusinessLogic.FixLatestAddressesAsync();
            await organisationBusinessLogic.SetUniqueOrganisationReferencesAsync();
            await organisationBusinessLogic.FixLatestAddressesAsync();
            var scopeBusinessLogic = host.Services.GetRequiredService<IScopeBusinessLogic>();
            await scopeBusinessLogic.SetPresumedScopesAsync();
            organisationBusinessLogic = host.Services.GetRequiredService<IOrganisationBusinessLogic>();
            await organisationBusinessLogic.FixLatestScopesAsync();

            //Set the full reset time to now (+15 seconds grace)
            Thread.Sleep(15);
            var importTime = DateTime.Now;

            //Set the last database reset
            if (string.IsNullOrWhiteSpace(vaultName))
                RegistryHelper.SetRegistryKey(LastFullDatabaseResetKey, importTime.ToString());
            else
                host.SetKeyVaultSecret(vaultName, LastFullDatabaseResetKey, importTime.ToString());
        }

        /// <summary>
        /// Deletes all records in all tables created after a specified date
        /// </summary>
        /// <param name="host"></param>
        public static void ResetDatabase(this IDataRepository dataRepository, DateTime createdDate)
        {
            dataRepository.SetCommandTimeout(CommandTimeout);//Give the batch command 5 mins to timout - required on Basic SQL tier
            dataRepository.GetAll<AuditLog>().Where(r => r.CreatedDate >= createdDate).BatchDelete();
            dataRepository.GetAll<Feedback>().Where(r => r.CreatedDate >= createdDate).BatchDelete();
            dataRepository.GetAll<Organisation>().Where(r => r.Created >= createdDate).BatchUpdate(o => new Organisation { LatestAddressId = null, LatestStatementId = null, LatestPublicSectorTypeId = null, LatestRegistrationOrganisationId = null, LatestRegistrationUserId = null, LatestScopeId = null });
            dataRepository.GetAll<StatementOrganisation>().Where(r => r.Created >= createdDate).BatchUpdate(o => new StatementOrganisation { OrganisationId = null });
            dataRepository.GetAll<UserOrganisation>().Where(r => r.Created >= createdDate).BatchDelete();
            dataRepository.GetAll<Organisation>().Where(r => r.Created >= createdDate).BatchDelete();
            dataRepository.GetAll<ReminderEmail>().Where(r => r.Created >= createdDate).BatchDelete();
            dataRepository.GetAll<User>().Where(r => r.Created >= createdDate).BatchDelete();
            dataRepository.GetAll<PublicSectorType>().Where(r => r.Created >= createdDate).BatchDelete();
            dataRepository.GetAll<SicCode>().Where(r => r.Created >= createdDate).BatchDelete();
            dataRepository.GetAll<SicSection>().Where(r => r.Created >= createdDate).BatchDelete();
            dataRepository.GetAll<StatementDiligenceType>().Where(r => r.Created >= createdDate).BatchDelete();
            dataRepository.GetAll<StatementPolicyType>().Where(r => r.Created >= createdDate).BatchDelete();
            dataRepository.GetAll<StatementRiskType>().Where(r => r.Created >= createdDate).BatchDelete();
            dataRepository.GetAll<StatementTrainingType>().Where(r => r.Created >= createdDate).BatchDelete();
        }

        /// <summary>
        /// Delete all the test records in the database created before the specified deadline 
        /// </summary>
        /// <param name="host">The WebHost connected to the database</param>
        /// <param name="deadline">If empty deletes all test records</param>
        public static void DeleteAllTestRecords(this IHost host, DateTime? deadline = null)
        {
            var dbContext = host.Services.GetRequiredService<IDbContext>();
            dbContext.DeleteAllTestRecords(deadline);
        }

        /// <summary>
        /// Shim for saving when needed, should prob refactor this out.
        /// </summary>
        /// <returns></returns>
        public static async Task SaveDatabaseAsync(this BaseUITest uiTest)
        {
            await uiTest.ServiceScope.GetDataRepository().SaveChangesAsync();
        }
    }
}
