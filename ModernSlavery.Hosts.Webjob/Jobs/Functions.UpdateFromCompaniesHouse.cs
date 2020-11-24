using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        [Singleton(Mode = SingletonMode.Listener)]
        [Disable(typeof(DisableWebjobProvider))]
        public async Task UpdateFromCompaniesHouseAsync([TimerTrigger("%UpdateFromCompaniesHouseAsync%")] TimerInfo timer,ILogger log)
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
                await _messenger.SendMsuMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
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
                await _companiesHouseService.UpdateOrganisationsAsync();
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateFromCompaniesHouseAsync));
            }
        }
    }
}