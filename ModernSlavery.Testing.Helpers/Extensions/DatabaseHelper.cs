using EFCore.BulkExtensions;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace ModernSlavery.Testing.Helpers.Extensions
{
    public static class DatabaseHelper
    {

        /// <summary>
        /// Deletes all tables in the database (except _EFMigrationsHistory) and reimports all seed data
        /// </summary>
        /// <param name="host"></param>
        public static void ResetDatabase(this IHost host)
        {
            var dataRepository = host.GetDataRepository();
            dataRepository.BeginTransactionAsync(async () =>
            {
                try
                {
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
                    dataRepository.CommitTransaction();
                }
                catch
                {
                    dataRepository.RollbackTransaction();
                    throw;
                }
            }).Wait();

            var dataImporter = host.Services.GetService<IDataImporter>();
            dataImporter.ImportSICSectionsAsync().Wait();
            dataImporter.ImportSICCodesAsync().Wait();
            dataImporter.ImportStatementDiligenceTypesAsync().Wait();
            dataImporter.ImportStatementPolicyTypesAsync().Wait();
            dataImporter.ImportStatementRiskTypesAsync().Wait();
            dataImporter.ImportStatementSectorTypesAsync().Wait();
            dataImporter.ImportStatementTrainingTypesAsync().Wait();
            dataImporter.ImportPrivateOrganisationsAsync(-1).Wait();
            dataImporter.ImportPublicOrganisationsAsync(-1).Wait();
        }


        /// <summary>
        /// Delete all the test records in the database created before the specified deadline 
        /// </summary>
        /// <param name="host">The WebHost connected to the database</param>
        /// <param name="deadline">If empty deletes all test records</param>
        public static void DeleteAllTestRecords(this IHost host, DateTime? deadline = null)
        {
            var databaseContext = host.GetDatabaseContext();
            databaseContext.DeleteAllTestRecords(deadline);
        }

        public static async Task SaveChangesAsync(this IHost host)
        {
            var dataRepository = host.GetDataRepository();
            await dataRepository.SaveChangesAsync();
        }

    }
}
