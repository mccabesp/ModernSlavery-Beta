using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        /// <summary>
        ///     Once per weekend execute healthchecks on database and file storage and report faults to logs and email to GEO
        ///     admins
        /// </summary>
        [Disable(typeof(DisableWebjobProvider))]
        public async Task StorageHealthCheck([TimerTrigger(typeof(OncePerWeekendRandomSchedule))]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                //TODO: Check for any records with future date/times in when Tardis is zero

                //TODO: Check for organisations with multiple active addresses

                //TODO: Check for consecutive duplicate addresses from same source

                //TODO: Check for organisations with multiple active scopes per year

                //TODO: Check for active organisations with no active scopes for every year

                //TODO: Check address status matches latest status

                //TODO: Check organisation status matches latest status

                //TODO: Check organisation name matches latest organisation name

                //TODO: Check organisation latest address is correct

                //TODO: Check organisation latest return is correct

                //TODO: Check organisation latest scope is correct

                //TODO: Check organisation latest registration is correct

                //TODO: Check organisation latest public sector type is correct

                //TODO: Check scope snapshot dates for org sectors

                //TODO: Check return accounting dates for org sectors

                //TODO: Check for orgs with same name are not active

                //TODO: Check organisations with same company number 

                //TODO: Check organisations with same DUNS number 

                //TODO: Check organisations with same EmployerRef 

                //TODO: Check organisations with same OrganisationReferences

                //TODO: Check return status matches latest status

                //TODO: Check for multiple active returns per year

                //TODO: Check user status matches latest status

                //TODO: Check users with too many login attempts 
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(StorageHealthCheck)}):{ex.Message}:{ex.GetDetailsText()}";
                log.LogError(ex, $"Failed webjob ({nameof(StorageHealthCheck)})");

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }
    }
}