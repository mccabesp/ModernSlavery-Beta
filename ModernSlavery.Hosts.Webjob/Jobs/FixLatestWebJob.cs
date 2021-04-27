using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Hosts.Webjob.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class FixLatestWebJob : WebJob
    {
        #region Dependencies
        private readonly ISmtpMessenger _messenger;
        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;
        #endregion
        public FixLatestWebJob(
            ISmtpMessenger messenger,
            IOrganisationBusinessLogic organisationBusinessLogic)
        {
            _messenger = messenger;
            _organisationBusinessLogic = organisationBusinessLogic;
        }

        //Ensure all addresses have are known to be UK or not
        [Disable(typeof(DisableWebJobProvider))]
        public async Task FixLatestAsync([TimerTrigger("%FixLatest%")] TimerInfo timer, ILogger log)
        {
            try
            {
                await FixLatestAsync().ConfigureAwait(false);
                log.LogDebug($"Executed WebJob {nameof(FixLatestAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(FixLatestAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
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