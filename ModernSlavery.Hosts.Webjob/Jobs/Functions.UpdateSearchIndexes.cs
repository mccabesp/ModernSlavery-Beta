using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        //Update the search indexes
        [Disable(typeof(DisableWebjobProvider))]
        public async Task UpdateOrganisationSearchIndexesAsync([TimerTrigger("%UpdateOrganisationSearchIndexesAsync%", RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            if (_searchOptions.Disabled) return;

            try
            {
                await UpdateOrganisationSearchAsync(log).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", ex.Message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }

        private async Task UpdateOrganisationSearchAsync(ILogger log,
            string userEmail = null,
            bool force = false)
        {
            if (RunningJobs.Contains(nameof(UpdateOrganisationSearchAsync)))
            {
                log.LogInformation("The set of running jobs already contains 'UpdateOrganisationSearch'");
                return;
            }

            try
            {
                await _searchBusinessLogic.UpdateOrganisationSearchIndexAsync().ConfigureAwait(false);

                if (force && !string.IsNullOrWhiteSpace(userEmail))
                    try
                    {
                        await _Messenger.SendMessageAsync("UpdateOrganisationSearchIndexes complete", userEmail, "The update of the search indexes completed successfully.").ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "UpdateOrganisationSearch: An error occurred trying to send an email");
                    }
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateOrganisationSearchAsync));
            }
        }

    }
}