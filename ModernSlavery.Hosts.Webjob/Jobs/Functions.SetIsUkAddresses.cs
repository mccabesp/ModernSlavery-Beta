using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        //Ensure all addresses have are known to be UK or not
        [Disable(typeof(DisableWebjobProvider))]
        public async Task SetIsUkAddressesAsync([TimerTrigger("%SetIsUkAddressesAsync%")] TimerInfo timer, ILogger log)
        {
            if (RunningJobs.Contains(nameof(SetIsUkAddressesAsync))) return;
            RunningJobs.Add(nameof(SetIsUkAddressesAsync));
            try
            {
                var addresses = _dataRepository.GetAll<OrganisationAddress>().Where(a => a.IsUkAddress == null);
                foreach (var org in addresses) await SetIsUkAddressAsync(org);

                log.LogDebug($"Executed Webjob {nameof(SetIsUkAddressesAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(SetIsUkAddressesAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
            finally
            {
                RunningJobs.Remove(nameof(SetIsUkAddressesAsync));
            }
           
        }

        public async Task SetIsUkAddressAsync(OrganisationAddress address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (string.IsNullOrWhiteSpace(address.PostCode)) throw new ArgumentNullException(nameof(address.PostCode));

            //Check if the address is a valid UK postcode
            address.IsUkAddress = await _postCodeChecker.IsValidPostcode(address.PostCode);

            //Save the address
            await _dataRepository.SaveChangesAsync();
        }
    }
}