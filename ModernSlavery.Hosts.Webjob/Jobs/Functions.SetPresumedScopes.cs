﻿using System;
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
        public async Task SetPresumedScopes([TimerTrigger("%SetPresumedScopes%")]
            TimerInfo timer,
            ILogger log)
        {
            if (RunningJobs.Contains(nameof(SetPresumedScopes))) return;
            RunningJobs.Add(nameof(SetPresumedScopes));
            try
            {
                //Initialise any unknown scope statuses
                var changedOrgs = await _scopeBusinessLogic.FixScopeRowStatusesAsync().ConfigureAwait(false);

                //Initialise the presumed scoped
                changedOrgs.AddRange(await _scopeBusinessLogic.SetPresumedScopesAsync().ConfigureAwait(false));

                //Update the search indexes
                if (changedOrgs.Count > 0 && !_searchOptions.Disabled) await _searchBusinessLogic.RefreshSearchDocumentsAsync(changedOrgs.ToArray()).ConfigureAwait(false);

                log.LogDebug($"Executed Webjob {nameof(SetPresumedScopes)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(SetPresumedScopes)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(SetPresumedScopes));
            }
            
        }
    }
}