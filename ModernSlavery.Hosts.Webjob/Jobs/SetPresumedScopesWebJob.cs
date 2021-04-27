using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Options;
using ModernSlavery.Hosts.Webjob.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class SetPresumedScopesWebJob : WebJob
    {
        #region Dependencies
        private readonly SearchOptions _searchOptions;
        private readonly ISmtpMessenger _messenger;
        private readonly IScopeBusinessLogic _scopeBusinessLogic;
        private readonly ISearchBusinessLogic _searchBusinessLogic;
        #endregion

        public SetPresumedScopesWebJob(
            SearchOptions searchOptions,
            ISmtpMessenger messenger,
            IScopeBusinessLogic scopeBusinessLogic,
            ISearchBusinessLogic searchBusinessLogic)
        {
            _searchOptions = searchOptions;
            _messenger = messenger;
            _scopeBusinessLogic = scopeBusinessLogic;
            _searchBusinessLogic = searchBusinessLogic;
        }

        //Set presumed scope of previous years and current years
        [Disable(typeof(DisableWebJobProvider))]
        public async Task SetPresumedScopesAsync([TimerTrigger("%SetPresumedScopes%")]
            TimerInfo timer,
                ILogger log)
        {
            if (RunningJobs.Contains(nameof(SetPresumedScopesAsync))) return;
            RunningJobs.Add(nameof(SetPresumedScopesAsync));
            try
            {
                //Initialise any unknown scope statuses
                var changedOrgs = await _scopeBusinessLogic.FixScopeRowStatusesAsync().ConfigureAwait(false);

                //Initialise the presumed scoped
                changedOrgs.AddRange(await _scopeBusinessLogic.SetPresumedScopesAsync().ConfigureAwait(false));

                log.LogDebug($"Executed WebJob {nameof(SetPresumedScopesAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(SetPresumedScopesAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(SetPresumedScopesAsync));
            }

        }
    }
}