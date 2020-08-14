using System;
using System.Data;
using System.Linq;
using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Infrastructure.Database
{
    public partial class DatabaseContext
    {
        public void DeleteAllTestRecords(DateTime? deadline = null)
        {
            if (_sharedOptions.IsProduction())
                throw new Exception("Attempt to delete all test data from production environment");

            if (string.IsNullOrWhiteSpace(_sharedOptions.TestPrefix))
                throw new ArgumentNullException(nameof(_sharedOptions.TestPrefix));

            using (var transaction = Database.BeginTransaction())
            {
                try
                {

                    if (deadline == null || deadline.Value == DateTime.MinValue)
                    {
                        Set<AuditLog>().Where(a => a.OriginalUser != null && a.OriginalUser.Firstname.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                        Set<AuditLog>().Where(a => a.ImpersonatedUser != null && a.OriginalUser.Firstname.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                        Set<Feedback>().Where(f => f.EmailAddress.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                        Set<Organisation>().Where(o => o.OrganisationName.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchUpdate(o => new Organisation { LatestAddressId = null, LatestStatementId = null, LatestPublicSectorTypeId = null, LatestRegistrationOrganisationId = null, LatestRegistrationUserId = null, LatestScopeId = null });
                        Set<StatementOrganisation>().Where(o => o != null && o.OrganisationName.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchUpdate(o => new StatementOrganisation { OrganisationId = null });
                        Set<UserOrganisation>().Where(uo => uo.User.Firstname.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                        Set<UserOrganisation>().Where(uo => uo.Organisation.OrganisationName.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                        Set<Organisation>().Where(o => o.OrganisationName.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                        Set<ReminderEmail>().Where(r => r.User.FirstName.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                        Set<User>().Where(u => u.Firstname.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                    }
                    else
                    {
                        Set<AuditLog>().Where(a => a.CreatedDate < deadline.Value && a.OriginalUser != null && a.OriginalUser.Firstname.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                        Set<AuditLog>().Where(a => a.CreatedDate < deadline.Value && a.ImpersonatedUser != null && a.OriginalUser.Firstname.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                        Set<Feedback>().Where(f => f.CreatedDate < deadline.Value && f.EmailAddress.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                        Set<Organisation>().Where(o => o.Created < deadline.Value && o.OrganisationName.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchUpdate(o => new Organisation { LatestAddressId = null, LatestStatementId = null, LatestPublicSectorTypeId = null, LatestRegistrationOrganisationId = null, LatestRegistrationUserId = null, LatestScopeId = null });
                        Set<StatementOrganisation>().Where(o => o != null && o.Created < deadline.Value && o.OrganisationName.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchUpdate(o => new StatementOrganisation { OrganisationId = null });
                        Set<UserOrganisation>().Where(uo => uo.Created < deadline.Value && uo.User.Firstname.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                        Set<UserOrganisation>().Where(uo => uo.Created < deadline.Value && uo.Organisation.OrganisationName.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                        Set<Organisation>().Where(o => o.Created < deadline.Value && o.OrganisationName.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                        Set<ReminderEmail>().Where(r => r.Created < deadline.Value && r.User.FirstName.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                        Set<User>().Where(u => u.Created < deadline.Value && u.Firstname.StartsWith(_sharedOptions.TestPrefix, StringComparison.OrdinalIgnoreCase)).BatchDelete();
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void ExecuteSqlCommand(string query, int timeOut = 120)
        {
            using (var sqlConnection1 = new SqlConnection(_databaseOptions.ConnectionString))
            {
                using (var cmd = new SqlCommand(query, sqlConnection1))
                {
                    sqlConnection1.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = timeOut;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}