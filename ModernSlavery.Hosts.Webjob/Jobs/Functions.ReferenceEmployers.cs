using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        //Ensure all organisations have a unique employer reference
        public async Task ReferenceEmployers([TimerTrigger("01:00:00:00")] TimerInfo timer, ILogger log)
        {
            try
            {
                await ReferenceEmployersAsync().ConfigureAwait(false);
                log.LogDebug($"Executed {nameof(ReferenceEmployers)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(ReferenceEmployers)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }

        private async Task ReferenceEmployersAsync()
        {
            if (RunningJobs.Contains(nameof(ReferenceEmployers))) return;

            RunningJobs.Add(nameof(ReferenceEmployers));

            try
            {
                await _OrganisationBusinessLogic.SetUniqueEmployerReferencesAsync().ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(ReferenceEmployers));
            }
        }
    }
}