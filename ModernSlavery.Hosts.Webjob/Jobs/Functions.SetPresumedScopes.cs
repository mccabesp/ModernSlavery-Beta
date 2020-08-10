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
        public async Task SetPresumedScopes([TimerTrigger("%SetPresumedScopes%", RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                //Initialise any unknown scope statuses
                var changedOrgs = await _ScopeBusinessLogic.SetScopeStatusesAsync().ConfigureAwait(false);

                //Initialise the presumed scoped
                changedOrgs.AddRange(await _ScopeBusinessLogic.SetPresumedScopesAsync().ConfigureAwait(false));

                //Update the search indexes
                if (changedOrgs.Count > 0) await SearchBusinessLogic.UpdateSearchIndexAsync(changedOrgs.ToArray()).ConfigureAwait(false);

                log.LogDebug($"Executed {nameof(SetPresumedScopes)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(SetPresumedScopes)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }
    }
}