using System;
using System.Data;
using System.Data.SqlClient;
using ModernSlavery.Core;
using ModernSlavery.Extensions.AspNetCore;

namespace ModernSlavery.Database
{
    public partial class DatabaseContext
    {

        public void DeleteAllTestRecords(DateTime? deadline = null)
        {
            if (Config.IsProduction())
            {
                throw new Exception("Attempt to delete all test data from production environment");
            }

            if (string.IsNullOrWhiteSpace(GlobalOptions.TestPrefix))
            {
                throw new ArgumentNullException(nameof(GlobalOptions.TestPrefix));
            }

            if (deadline == null || deadline.Value == DateTime.MinValue)
            {
                ExecuteSqlCommand(
                    $"UPDATE UO SET AddressId=null FROM UserOrganisations UO WITH (ROWLOCK) JOIN organisations O ON O.OrganisationId=UO.OrganisationId where O.OrganisationName like '{GlobalOptions.TestPrefix}%'");
                ExecuteSqlCommand(
                    $"UPDATE UO SET AddressId=null FROM UserOrganisations UO WITH (ROWLOCK) JOIN Users U ON U.UserId=UO.UserId where U.Firstname like '{GlobalOptions.TestPrefix}%'");
                ExecuteSqlCommand(
                    $"UPDATE Organisations WITH (ROWLOCK) SET LatestAddressId=null, LatestRegistration_OrganisationId=null,LatestRegistration_UserId=null,LatestReturnId=null,LatestScopeId=null where OrganisationName like '{GlobalOptions.TestPrefix}%'");
                ExecuteSqlCommand($"DELETE Users WITH (ROWLOCK) where Firstname like '{GlobalOptions.TestPrefix}%'");
                ExecuteSqlCommand($"DELETE Organisations WITH (ROWLOCK) where OrganisationName like '{GlobalOptions.TestPrefix}%'");
            }
            else
            {
                string dl = deadline.Value.ToString("yyyy-MM-dd HH:mm:ss");
                ExecuteSqlCommand(
                    $"UPDATE UO SET AddressId=null FROM UserOrganisations UO WITH (ROWLOCK) JOIN organisations O ON O.OrganisationId=UO.OrganisationId where O.OrganisationName like '{GlobalOptions.TestPrefix}%' AND UO.Created<'{dl}'");
                ExecuteSqlCommand(
                    $"UPDATE UO SET AddressId=null FROM UserOrganisations UO WITH (ROWLOCK) JOIN Users U ON U.UserId=UO.UserId where U.Firstname like '{GlobalOptions.TestPrefix}%' AND UO.Created<'{dl}'");
                ExecuteSqlCommand(
                    $"UPDATE Organisations WITH (ROWLOCK) SET LatestAddressId=null, LatestRegistration_OrganisationId=null,LatestRegistration_UserId=null,LatestReturnId=null,LatestScopeId=null where OrganisationName like '{GlobalOptions.TestPrefix}%' AND Created<'{dl}'");
                ExecuteSqlCommand($"DELETE Users WITH (ROWLOCK) where Firstname like '{GlobalOptions.TestPrefix}%' AND Created<'{dl}'");
                ExecuteSqlCommand(
                    $"DELETE Organisations WITH (ROWLOCK) where OrganisationName like '{GlobalOptions.TestPrefix}%' AND Created<'{dl}'");
            }
        }


        private static void ExecuteSqlCommand(string query, int timeOut = 120)
        {
            using (var sqlConnection1 = new SqlConnection(Config.GetConnectionString("ModernSlaveryDatabase")))
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
