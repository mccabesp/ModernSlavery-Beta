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
        [Disable(typeof(DisableWebjobProvider))]
        public async Task FixLatestAsync([TimerTrigger("%FixLatestAsync%")] TimerInfo timer, ILogger log)
        {
            try
            {
                await FixLatestAsync().ConfigureAwait(false);
                log.LogDebug($"Executed {nameof(FixLatestAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(FixLatestAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }

        private async Task FixLatestAsync()
        {
            if (RunningJobs.Contains(nameof(FixLatestAsync))) return;

            RunningJobs.Add(nameof(FixLatestAsync));

            try
            {
                await _organisationBusinessLogic.FixLatestAddressesAsync().ConfigureAwait(false);
                await _organisationBusinessLogic.FixLatestScopesAsync().ConfigureAwait(false);
                await _organisationBusinessLogic.FixLatestStatementsAsync().ConfigureAwait(false);
                await _organisationBusinessLogic.FixLatestRegistrationsAsync().ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(FixLatestAsync));
            }
        }
    }
}