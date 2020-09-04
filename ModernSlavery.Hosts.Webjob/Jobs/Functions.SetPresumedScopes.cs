using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        //Set presumed scope of previous years and current years
        [Disable(typeof(DisableWebjobProvider))]
        public async Task SetPresumedScopes([TimerTrigger("%SetPresumedScopes%",RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                //Initialise any unknown scope statuses
                var changedOrgs = await _scopeBusinessLogic.FixScopeRowStatusesAsync().ConfigureAwait(false);

                //Initialise the presumed scoped
                changedOrgs.AddRange(await _scopeBusinessLogic.SetPresumedScopesAsync().ConfigureAwait(false));

                //Update the search indexes
                if (changedOrgs.Count > 0 && !_searchOptions.Disabled) await _searchBusinessLogic.UpdateOrganisationSearchIndexAsync(changedOrgs.ToArray()).ConfigureAwait(false);

                log.LogDebug($"Executed {nameof(SetPresumedScopes)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(SetPresumedScopes)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }
    }
}