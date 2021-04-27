using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Hosts.Webjob.Classes;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public class SetIsUkAddressesWebJob : WebJob
    {
        #region Dependencies
        private readonly ISmtpMessenger _messenger;
        private readonly IDataRepository _dataRepository;
        private readonly IPostcodeChecker _postCodeChecker;
        #endregion

        public SetIsUkAddressesWebJob(
            ISmtpMessenger messenger,
            IDataRepository dataRepository,
            IPostcodeChecker postCodeChecker)
        {
            _messenger = messenger;
            _dataRepository = dataRepository;
            _postCodeChecker = postCodeChecker;
        }

        //Ensure all addresses have are known to be UK or not
        [Disable(typeof(DisableWebJobProvider))]
        public async Task SetIsUkAddressesAsync([TimerTrigger("%SetIsUkAddresses%")] TimerInfo timer, ILogger log)
        {
            if (RunningJobs.Contains(nameof(SetIsUkAddressesAsync))) return;
            RunningJobs.Add(nameof(SetIsUkAddressesAsync));
            try
            {
                var addresses = _dataRepository.GetAll<OrganisationAddress>().Where(a => a.IsUkAddress == null && a.PostCode != null && a.PostCode != "").ToList();

                foreach (var address in addresses) await SetIsUkAddressAsync(address).ConfigureAwait(false);

                await _dataRepository.BulkUpdateAsync(addresses);

                log.LogDebug($"Executed WebJob {nameof(SetIsUkAddressesAsync)} successfully");
            }
            catch (Exception ex)
            {
                var message = $"Failed webjob ({nameof(SetIsUkAddressesAsync)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _messenger.SendMsuMessageAsync("MSU - WEBJOBS ERROR", message).ConfigureAwait(false);
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
            address.IsUkAddress = await _postCodeChecker.CheckPostcodeAsync(address.PostCode).ConfigureAwait(false);
        }
    }
}