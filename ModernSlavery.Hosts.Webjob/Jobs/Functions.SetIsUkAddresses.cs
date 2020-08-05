using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        //Ensure all addresses have are known to be UK or not
        public async Task SetIsUkAddressesAsync([TimerTrigger("01:00:00:00")] TimerInfo timer, ILogger log)
        {
            try
            {
                await SetIsUkAddressesAsync().ConfigureAwait(false);
                log.LogDebug($"Executed {nameof(SetIsUkAddressesAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(SetIsUkAddressesAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }

        private async Task SetIsUkAddressesAsync()
        {
            if (RunningJobs.Contains(nameof(SetIsUkAddressesAsync))) return;

            RunningJobs.Add(nameof(SetIsUkAddressesAsync));

            try
            {
                await _RegistrationService.SetIsUkAddressesAsync().ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(SetIsUkAddressesAsync));
            }
        }
    }
}