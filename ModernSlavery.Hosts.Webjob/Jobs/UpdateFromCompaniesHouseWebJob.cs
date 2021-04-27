using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Hosts.Webjob.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class UpdateFromCompaniesHouseWebJob : WebJob
    {
        #region Dependencies
        private readonly ISmtpMessenger _messenger;
        private readonly ICompaniesHouseService _companiesHouseService;
        #endregion

        public UpdateFromCompaniesHouseWebJob(
            ISmtpMessenger messenger,
            ICompaniesHouseService companiesHouseService)
        {
            _messenger = messenger;
            _companiesHouseService = companiesHouseService;
        }

        [Singleton(Mode = SingletonMode.Listener)]
        [Disable(typeof(DisableWebJobProvider))]
        public async Task UpdateFromCompaniesHouseAsync([TimerTrigger("%UpdateFromCompaniesHouse%")] TimerInfo timer, ILogger log)
        {
            try
            {
                await UpdateFromCompaniesHouseAsync().ConfigureAwait(false);

                log.LogDebug($"Executed WebJob {nameof(UpdateFromCompaniesHouseAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(UpdateFromCompaniesHouseAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
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
                await _companiesHouseService.UpdateOrganisationsAsync().ConfigureAwait(false);
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateFromCompaniesHouseAsync));
            }
        }
    }
}