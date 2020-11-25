using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        /// <summary>
        ///     Ensures only latest organisation address is active one
        /// </summary>
        [Disable(typeof(DisableWebjobProvider))]
        public async Task FixOrganisationAddresses([TimerTrigger(typeof(OncePerWeekendRandomSchedule))]
            TimerInfo timer,
            ILogger log)
        {
            if (RunningJobs.Contains(nameof(FixOrganisationAddresses))) return;
            RunningJobs.Add(nameof(FixOrganisationAddresses));
            try
            {
                //Load all the organisation in the database
                var orgs = await _dataRepository.GetAll<Organisation>().ToListAsync().ConfigureAwait(false);
                var count = 0;

                //Loop through each organisation in the list
                foreach (var org in orgs)
                {
                    //Get all the active or retired addresses for this organisation sorted by when they are thoughted to have been first activated
                    var addresses = org.OrganisationAddresses
                        .Where(a => a.Status == AddressStatuses.Active || a.Status == AddressStatuses.Retired)
                        .OrderBy(a => a.GetFirstRegisteredDate())
                        .ToList();

                    var changed = false;
                    for (var i = 0; i < addresses.Count; i++)
                    {
                        //Get the current address
                        var address = addresses[i];

                        //Get the next address if there is one
                        var nextAddress = i + 1 < addresses.Count ? addresses[i + 1] : null;

                        //If current address is a previous address and its status is active
                        if (nextAddress != null && address.Status == AddressStatuses.Active)
                        {
                            //If its replacement address has no creator id
                            if (nextAddress.CreatedByUserId < 1)
                            {
                                //Lookup the status when the replacement address was last activated
                                var activeStatus = nextAddress.AddressStatuses.OrderByDescending(st => st.StatusDate)
                                    .FirstOrDefault(st => st.Status == AddressStatuses.Active);
                                //Set the creator id of the replacement address to that saved with its activation status
                                nextAddress.CreatedByUserId = activeStatus.ByUserId;
                            }

                            //Retire the current address address
                            address.SetStatus(
                                AddressStatuses.Retired,
                                nextAddress.CreatedByUserId,
                                $"Replaced by {nextAddress.Source}",
                                nextAddress.StatusDate);
                            changed = true;
                        }
                    }

                    //Get the actual latest active address
                    var latestAddress = addresses.LastOrDefault(a => a.Status == AddressStatuses.Active);

                    //If the LatestAddress of the organisation is wrong then fix it
                    if (latestAddress != null &&
                        (org.LatestAddress == null || org.LatestAddress.AddressId != latestAddress.AddressId))
                    {
                        org.LatestAddress = latestAddress;
                        changed = true;
                    }

                    //If there were database changes then save them
                    if (changed) await _dataRepository.SaveChangesAsync().ConfigureAwait(false);

                    count++;
                }
                log.LogDebug($"Executed Webjob {nameof(FixOrganisationAddresses)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(FixOrganisationAddresses)}):{ex.Message}:{ex.GetDetailsText()}";
                log.LogError(ex, $"Failed webjob ({nameof(FixOrganisationAddresses)})");

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(FixOrganisationAddresses));
            }
            
        }
    }
}