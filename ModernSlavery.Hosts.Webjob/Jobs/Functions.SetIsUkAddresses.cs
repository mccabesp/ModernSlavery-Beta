﻿using System;
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
        public async Task SetIsUkAddressesAsync([TimerTrigger("01:00:00:00")] TimerInfo timer, ILogger log)
        {
            try
            {
                await SetIsUkAddressesAsync().ConfigureAwait(false);
                log.LogDebug($"Executed {nameof(SetIsUkAddressesAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(SetIsUkAddressesAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message).ConfigureAwait(false);
                //Rethrow the error
                throw;
            }
        }

        private async Task SetIsUkAddressesAsync()
        {
            if (RunningJobs.Contains(nameof(SetIsUkAddressesAsync))) return;

            RunningJobs.Add(nameof(SetIsUkAddressesAsync));

            try
            {
                var addresses = _SharedBusinessLogic.DataRepository.GetAll<OrganisationAddress>().Where(a => a.IsUkAddress == null);
                foreach (var org in addresses) await SetIsUkAddressAsync(org);
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
            address.IsUkAddress = await _PostCodeChecker.IsValidPostcode(address.PostCode);

            //Save the address
            await _SharedBusinessLogic.DataRepository.SaveChangesAsync();
        }
    }
}