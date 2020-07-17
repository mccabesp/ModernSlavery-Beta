using System;
using System.Data;
using Microsoft.Data.SqlClient;

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

            if (deadline == null || deadline.Value == DateTime.MinValue)
            {
                ExecuteSqlCommand(
                    $"UPDATE UO SET AddressId=null FROM UserOrganisations UO WITH (ROWLOCK) JOIN organisations O ON O.OrganisationId=UO.OrganisationId where O.OrganisationName like '{_sharedOptions.TestPrefix}%'");
                ExecuteSqlCommand(
                    $"UPDATE UO SET AddressId=null FROM UserOrganisations UO WITH (ROWLOCK) JOIN Users U ON U.UserId=UO.UserId where U.Firstname like '{_sharedOptions.TestPrefix}%'");
                ExecuteSqlCommand(
                    $"UPDATE Organisations WITH (ROWLOCK) SET LatestAddressId=null, LatestRegistration_OrganisationId=null,LatestRegistration_UserId=null,LatestReturnId=null,LatestScopeId=null where OrganisationName like '{_sharedOptions.TestPrefix}%'");
                ExecuteSqlCommand($"DELETE Users WITH (ROWLOCK) where Firstname like '{_sharedOptions.TestPrefix}%'");
                ExecuteSqlCommand(
                    $"DELETE Organisations WITH (ROWLOCK) where OrganisationName like '{_sharedOptions.TestPrefix}%'");
            }
            else
            {
                var dl = deadline.Value.ToString("yyyy-MM-dd HH:mm:ss");
                ExecuteSqlCommand(
                    $"UPDATE UO SET AddressId=null FROM UserOrganisations UO WITH (ROWLOCK) JOIN organisations O ON O.OrganisationId=UO.OrganisationId where O.OrganisationName like '{_sharedOptions.TestPrefix}%' AND UO.Created<'{dl}'");
                ExecuteSqlCommand(
                    $"UPDATE UO SET AddressId=null FROM UserOrganisations UO WITH (ROWLOCK) JOIN Users U ON U.UserId=UO.UserId where U.Firstname like '{_sharedOptions.TestPrefix}%' AND UO.Created<'{dl}'");
                ExecuteSqlCommand(
                    $"UPDATE Organisations WITH (ROWLOCK) SET LatestAddressId=null, LatestRegistration_OrganisationId=null,LatestRegistration_UserId=null,LatestReturnId=null,LatestScopeId=null where OrganisationName like '{_sharedOptions.TestPrefix}%' AND Created<'{dl}'");
                ExecuteSqlCommand(
                    $"DELETE Users WITH (ROWLOCK) where Firstname like '{_sharedOptions.TestPrefix}%' AND Created<'{dl}'");
                ExecuteSqlCommand(
                    $"DELETE Organisations WITH (ROWLOCK) where OrganisationName like '{_sharedOptions.TestPrefix}%' AND Created<'{dl}'");
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