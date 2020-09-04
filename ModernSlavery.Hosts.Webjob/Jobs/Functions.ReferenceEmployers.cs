using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        //Ensure all organisations have a unique organisation reference
        [Disable(typeof(DisableWebjobProvider))]
        public async Task ReferenceOrganisations([TimerTrigger("%ReferenceOrganisations%")] TimerInfo timer, ILogger log)
        {
            try
            {
                await ReferenceOrganisationsAsync().ConfigureAwait(false);
                log.LogDebug($"Executed {nameof(ReferenceOrganisations)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(ReferenceOrganisations)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }

        private async Task ReferenceOrganisationsAsync()
        {
            if (RunningJobs.Contains(nameof(ReferenceOrganisations))) return;

            RunningJobs.Add(nameof(ReferenceOrganisations));

            try
            {
                await _organisationBusinessLogic.SetUniqueEmployerReferencesAsync().ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(ReferenceOrganisations));
            }
        }
    }
}