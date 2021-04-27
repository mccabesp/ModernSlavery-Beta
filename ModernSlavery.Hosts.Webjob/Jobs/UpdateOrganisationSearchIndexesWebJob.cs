using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Options;
using ModernSlavery.Hosts.Webjob.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class UpdateOrganisationSearchIndexesWebJob : WebJob
    {
        #region Dependencies
        private readonly SearchOptions _searchOptions;
        private readonly ISmtpMessenger _messenger;
        private readonly ISearchBusinessLogic _searchBusinessLogic;
        #endregion

        public UpdateOrganisationSearchIndexesWebJob(
            SearchOptions searchOptions,
            ISmtpMessenger messenger,
            ISearchBusinessLogic searchBusinessLogic)
        {
            _searchOptions = searchOptions;
            _messenger = messenger;
            _searchBusinessLogic = searchBusinessLogic;
        }

        //Update the search indexes
        [Disable(typeof(DisableWebJobProvider))]
        public async Task UpdateOrganisationSearchIndexesAsync([TimerTrigger("%UpdateOrganisationSearchIndexes%")]
            TimerInfo timer,
                ILogger log)
        {
            if (_searchOptions.Disabled) return;

            try
            {
                await UpdateOrganisationSearchAsync(log).ConfigureAwait(false);

                log.LogDebug($"Executed WebJob {nameof(UpdateOrganisationSearchIndexesAsync)} successfully");
            }
            catch (Exception ex)
            {
                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", ex.Message).ConfigureAwait(false);
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
                await _searchBusinessLogic.RefreshSearchDocumentsAsync().ConfigureAwait(false);

                if (force && !string.IsNullOrWhiteSpace(userEmail))
                    try
                    {
                        await _messenger.SendMessageAsync("UpdateOrganisationSearchIndexes complete", userEmail, "The update of the search indexes completed successfully.").ConfigureAwait(false);
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