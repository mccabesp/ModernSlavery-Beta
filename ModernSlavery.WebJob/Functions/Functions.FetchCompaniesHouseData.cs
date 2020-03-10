using System;
using System.Linq;
using ModernSlavery.Entities;
using ModernSlavery.Extensions;
using ModernSlavery.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs;

namespace ModernSlavery.WebJob
{
    public partial class Functions
    {

        [Singleton(Mode = SingletonMode.Listener)]
        public void FetchCompaniesHouseData([TimerTrigger("*/5 * * * *")] TimerInfo timer)
        {
            try
            {
                string runId = Guid.NewGuid().ToString("N").Substring(0, 8);
                _CustomLogger.Information($"Fetch companies house data web job has been started. Run id {runId}");

                UpdateFromCompaniesHouse(runId);

                _CustomLogger.Information($"Fetch companies house data web job has been finished. Run id {runId}");
            }
            catch (Exception ex)
            {
                string message = $"Failed fetch companies house webjob({nameof(FetchCompaniesHouseData)})";
                _CustomLogger.Error(message, ex);
                throw;
            }
        }

        private void UpdateFromCompaniesHouse(string runId)
        {
            int maxNumCallCompaniesHouseApi = Config.GetAppSetting("MaxNumCallsCompaniesHouseApiPerFiveMins").ToInt32(500);

            for (var i = 0; i < maxNumCallCompaniesHouseApi; i++)
            {
                long organisationId = DataRepository.GetAll<Organisation>()
                    .Where(org => !org.OptedOutFromCompaniesHouseUpdate && org.CompanyNumber != null && org.CompanyNumber != "")
                    .OrderByDescending(org => org.LastCheckedAgainstCompaniesHouse == null)
                    .ThenBy(org => org.LastCheckedAgainstCompaniesHouse)
                    .Select(org => org.OrganisationId)
                    .FirstOrDefault();

                _CustomLogger.Information($"Start update companies house data organisation id {organisationId}. Run id {runId}");
                _updateFromCompaniesHouseService.UpdateOrganisationDetails(organisationId);
                _CustomLogger.Information($"End update companies house data organisation id {organisationId}. Run id {runId}");
            }
        }

    }
}
