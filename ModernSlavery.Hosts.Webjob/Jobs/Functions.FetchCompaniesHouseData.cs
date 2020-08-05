using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        [Singleton(Mode = SingletonMode.Listener)]
        public async Task UpdateFromCompaniesHouseAsync([TimerTrigger("*/5 * * * *")] TimerInfo timer,ILogger log)
        {
            try
            {
                await UpdateFromCompaniesHouseAsync().ConfigureAwait(false);

                log.LogDebug($"Executed {nameof(UpdateFromCompaniesHouseAsync)} successfully");

            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(UpdateFromCompaniesHouseAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }

        private async Task UpdateFromCompaniesHouseAsync()
        {
            if (RunningJobs.Contains(nameof(UpdateFromCompaniesHouseAsync))) return;

            RunningJobs.Add(nameof(UpdateFromCompaniesHouseAsync));

            try
            {
                var lastCheck = VirtualDateTime.Now.AddMinutes(-5);

                var organisations = _SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                    .Where(org => !org.OptedOutFromCompaniesHouseUpdate && org.CompanyNumber != null && org.CompanyNumber != "" && (org.LastCheckedAgainstCompaniesHouse == null || org.LastCheckedAgainstCompaniesHouse < lastCheck))
                    .OrderByDescending(org => org.LastCheckedAgainstCompaniesHouse)
                    .Take(_SharedBusinessLogic.SharedOptions.MaxNumCallsCompaniesHouseApiPerFiveMins);

                foreach (var organisation in organisations)
                {
                    await _updateFromCompaniesHouseService.UpdateOrganisationDetailsAsync(organisation).ConfigureAwait(false);
                }
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateFromCompaniesHouseAsync));
            }
        }
    }
}