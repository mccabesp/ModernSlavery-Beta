using System;
using System.Threading.Tasks;
using ModernSlavery.Core;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Infrastructure.Storage.MessageQueues;

namespace ModernSlavery.Infrastructure.Logging
{
    public class RegistrationAuditLogger : AuditLogger, IRegistrationLogger
    {
        public RegistrationAuditLogger(SharedOptions sharedOptions,
            LogRecordQueue queue)
            : base(sharedOptions, queue,null,Filenames.RegistrationLog)
        {
        }

        public async Task LogUnregisteredAsync(UserOrganisation unregisteredUserOrg, string actionByEmailAddress)
        {
            await LogAsync(unregisteredUserOrg, "Unregistered", actionByEmailAddress);
        }

        public async Task LogUnregisteredSelfAsync(UserOrganisation unregisteredUserOrg, string actionByEmailAddress)
        {
            await LogAsync(unregisteredUserOrg, "Unregistered self", actionByEmailAddress);
        }

        public async Task LogUserAccountClosedAsync(UserOrganisation retiredUserOrg, string actionByEmailAddress)
        {
            await LogAsync(retiredUserOrg, "Unregistered closed account", actionByEmailAddress);
        }

        private async Task LogAsync(UserOrganisation logUserOrg, string status, string actionByEmailAddress)
        {
            var logOrg = logUserOrg.Organisation;
            var logUser = logUserOrg.User;
            var logAddress = logUserOrg.Address;

            if (logUser.EmailAddress.StartsWithI(SharedOptions.TestPrefix)) return;

            await WriteAsync(
                new RegisterLogModel
                {
                    StatusDate = VirtualDateTime.Now,
                    Status = status,
                    ActionBy = actionByEmailAddress,
                    Details = "",
                    Sector = logOrg.SectorType,
                    Organisation = logOrg.OrganisationName,
                    CompanyNo = logOrg.CompanyNumber,
                    Address = logAddress.GetAddressString(),
                    SicCodes = logOrg.GetLatestSicCodeIdsString(),
                    UserFirstname = logUser.Firstname,
                    UserLastname = logUser.Lastname,
                    UserJobtitle = logUser.JobTitle,
                    UserEmail = logUser.EmailAddress,
                    ContactFirstName = logUser.ContactFirstName,
                    ContactLastName = logUser.ContactLastName,
                    ContactJobTitle = logUser.ContactJobTitle,
                    ContactOrganisation = logUser.ContactOrganisation,
                    ContactPhoneNumber = logUser.ContactPhoneNumber
                });
        }
    }
}