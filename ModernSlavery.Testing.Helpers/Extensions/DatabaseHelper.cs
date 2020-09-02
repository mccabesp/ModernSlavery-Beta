using EFCore.BulkExtensions;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using ModernSlavery.Core.Extensions;
using Microsoft.EntityFrameworkCore;

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
        public static void ResetDatabase(this IHost host, bool force = false)
        {
            var config = host.GetConfiguration();
            var vaultName = config["Vault"];

            if (!force)
            {
                var lastFullDatabaseReset = string.IsNullOrWhiteSpace(vaultName) ? Environment.GetEnvironmentVariable(LastFullDatabaseResetKey).ToDateTime() : host.GetKeyVaultSecret(vaultName, LastFullDatabaseResetKey).ToDateTime();
                if (lastFullDatabaseReset > DateTime.MinValue)
                {
                    host.ResetDatabase(lastFullDatabaseReset);
                    return;
                }
            }
            
            var dataRepository = host.GetDataRepository();
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

            //Set the last database reset
            if (string.IsNullOrWhiteSpace(vaultName))
                Environment.SetEnvironmentVariable(LastFullDatabaseResetKey, DateTime.Now.ToString());
            else
                host.SetKeyVaultSecret(vaultName, LastFullDatabaseResetKey, DateTime.Now.ToString());
        }

        /// <summary>
        /// Deletes all records in all tables created after a specified date
        /// </summary>
        /// <param name="host"></param>
        public static void ResetDatabase(this IHost host, DateTime createdDate)
        {
            var dataRepository = host.GetDataRepository();
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
            var dbContext = host.GetDbContext();
            dbContext.DeleteAllTestRecords(deadline);
        }

        public static async Task SaveChangesAsync(this IHost host)
        {
            var dataRepository = host.GetDataRepository();
            await dataRepository.SaveChangesAsync();
        }


    }
}
