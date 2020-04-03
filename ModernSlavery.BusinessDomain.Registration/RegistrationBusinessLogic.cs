using System;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.BusinessDomain.Registration
{
    public class RegistrationBusinessLogic : IRegistrationBusinessLogic
    {
        private readonly IDataRepository DataRepository;

        private readonly IRegistrationLogger RegistrationLog;

        public RegistrationBusinessLogic(
            IDataRepository dataRepository, 
            IRegistrationLogger registrationLog)
        {
            DataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            RegistrationLog = registrationLog ?? throw new ArgumentNullException(nameof(registrationLog));
        }

        public async Task RemoveRetiredUserRegistrationsAsync(User userToRetire, User actionByUser)
        {
            // remove all the user registrations associated with the organisation
            userToRetire.UserOrganisations.ForEach(uo => uo.Organisation.UserOrganisations.Remove(uo));

            // get all latest registrations assigned to this user or latest registration is null
            var latestRegistrationsByUser = userToRetire.UserOrganisations
                .Where(
                    uo => uo.Organisation.LatestRegistration == null ||
                          uo.Organisation.LatestRegistration.UserId == userToRetire.UserId);

            // update those latest registrations
            latestRegistrationsByUser.ForEach(
                async userOrgToUnregister =>
                {
                    var sourceOrg = userOrgToUnregister.Organisation;

                    // update latest registration (if one exists)
                    var newLatestReg = sourceOrg.GetLatestRegistration();
                    if (newLatestReg != null) sourceOrg.LatestRegistration = newLatestReg;

                    // log unregistered via closed account
                    await RegistrationLog.LogUserAccountClosedAsync(userOrgToUnregister, actionByUser.EmailAddress);

                    // Remove user organisation
                    DataRepository.Delete(userOrgToUnregister);
                });

            // save changes to database
            await DataRepository.SaveChangesAsync();
        }

        public async Task RemoveRegistrationAsync(UserOrganisation userOrgToUnregister, User actionByUser)
        {
            if (userOrgToUnregister is null) throw new ArgumentNullException(nameof(userOrgToUnregister));

            if (actionByUser is null) throw new ArgumentNullException(nameof(actionByUser));

            var sourceOrg = userOrgToUnregister.Organisation;

            // Remove the user registration from the organisation
            sourceOrg.UserOrganisations.Remove(userOrgToUnregister);

            // update latest registration (if one exists)
            if (sourceOrg.LatestRegistration == null ||
                sourceOrg.LatestRegistration.UserId == userOrgToUnregister.UserId)
            {
                var newLatestReg = sourceOrg.GetLatestRegistration();
                if (newLatestReg != null) sourceOrg.LatestRegistration = newLatestReg;
            }

            // log record
            if (userOrgToUnregister.UserId == actionByUser.UserId)
                // unregistered self
                await RegistrationLog.LogUnregisteredSelfAsync(userOrgToUnregister, actionByUser.EmailAddress);
            else
                // unregistered by someone else
                await RegistrationLog.LogUnregisteredAsync(userOrgToUnregister, actionByUser.EmailAddress);

            // Remove user organisation
            DataRepository.Delete(userOrgToUnregister);

            // Save changes to database
            await DataRepository.SaveChangesAsync();
        }
    }
}