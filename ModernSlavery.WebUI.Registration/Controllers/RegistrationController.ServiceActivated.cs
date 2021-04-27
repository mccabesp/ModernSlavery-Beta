using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.WebUI.Registration.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Registration.Controllers
{
    public partial class RegistrationController : BaseController
    {
        [Authorize]
        [HttpGet("activate-service/{id}")]
        public async Task<IActionResult> ActivateService([Obfuscated] long id)
        {
            //Ensure user has completed the registration process
            ReportingOrganisationId = id;
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // Check the user has permission for this organisation
            var userOrg = VirtualUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == id);
            if (userOrg == null)return new HttpForbiddenResult($"User {VirtualUser?.EmailAddress} is not registered for organisation id {id}");
            if (userOrg.Organisation.SectorType != SectorTypes.Private)return new HttpBadRequestResult($"Cannot active public organisation {userOrg.Organisation.OrganisationName} using PIN code");

            // Ensure this organisation needs activation on the users account
            if (userOrg.IsRegisteredOK)throw new Exception($"Attempt to activate organisation {userOrg.OrganisationId}:'{userOrg.Organisation.OrganisationName}' for {VirtualUser.EmailAddress} by '{(OriginalUser == null ? VirtualUser.EmailAddress : OriginalUser.EmailAddress)}' which has already been activated");

            //Check the user has not exceeded their pin attempts
            if (userOrg.IsPINAttemptsExceeded(SharedBusinessLogic.SharedOptions.MaxPinAttempts))
            {
                var remaining = userOrg.GetTimeToNextPINAttempt(SharedBusinessLogic.SharedOptions.LockoutMinutes);
                if (remaining > TimeSpan.Zero) return View("CustomError", WebService.ErrorViewModelFactory.Create(1113, new { remainingTime = remaining.ToFriendly(maxParts: 2) }));
            }

            //Create the model
            var timeToNextPINResend = userOrg.GetTimeToNextPINResend(SharedBusinessLogic.SharedOptions.PinInPostMinRepostDays);
            var model = new CompleteViewModel();
            model.PIN = null;
            model.AllowResend = timeToNextPINResend <= TimeSpan.Zero;
            model.Remaining = timeToNextPINResend.ToFriendly(maxParts: 2);
            model.OrganisationId = userOrg.OrganisationId;

            //Show the PIN textbox and button
            return View("ActivateService", model);
        }

        [PreventDuplicatePost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("activate-service/{id}")]
        public async Task<IActionResult> ActivateService(CompleteViewModel model, [Obfuscated] long id)
        {
            //Ensure user has completed the registration process
            ReportingOrganisationId = id;
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Ensure they have entered a PIN
            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<CompleteViewModel>();
                return View("ActivateService", model);
            }

            // Check the user has permission for this organisation
            var userOrg = VirtualUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == id);
            if (userOrg == null) return new HttpForbiddenResult($"User {VirtualUser?.EmailAddress} is not registered for organisation id {id}");

            //Check the user has not exceeded his pin attempts
            if (userOrg.IsPINAttemptsExceeded(SharedBusinessLogic.SharedOptions.MaxPinAttempts))
            {
                var remaining = userOrg.GetTimeToNextPINAttempt(SharedBusinessLogic.SharedOptions.LockoutMinutes);
                if (remaining > TimeSpan.Zero)return View("CustomError",WebService.ErrorViewModelFactory.Create(1113,new { remainingTime = remaining.ToFriendly(maxParts: 2) }));
            }

            var updateSearchIndex = false;
            ActionResult result1;
            if (userOrg.IsCorrectPin(model.PIN))
            {
                //Set the user org as confirmed
                userOrg.PINConfirmedDate = VirtualDateTime.Now;

                //Set the pending organisation to active
                //Make sure the found organisation is active or pending
                if (userOrg.Organisation.Status.IsAny(OrganisationStatuses.Pending, OrganisationStatuses.Active))
                {
                    userOrg.Organisation.SetStatus(OrganisationStatuses.Active,OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId,"PIN Confirmed");
                    updateSearchIndex = true;
                }
                else
                {
                    Logger.LogWarning($"Attempt to PIN activate a {userOrg.Organisation.Status} organisation",$"Organisation: '{userOrg.Organisation.OrganisationName}' Reference: '{userOrg.Organisation.OrganisationReference}' User: '{VirtualUser.EmailAddress}'");
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1149));
                }

                //Set the latest registration
                userOrg.Organisation.LatestRegistration = userOrg;

                //Retire the old address 
                var latestAddress = userOrg.Organisation.LatestAddress ?? userOrg.Organisation.GetLatestAddress();
                if (latestAddress != null && latestAddress.AddressId != userOrg.Address.AddressId)
                {
                    latestAddress.SetStatus(AddressStatuses.Retired,OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId,"Replaced by PIN in post");
                    updateSearchIndex = true;
                }

                //Activate the address the pin was sent to
                userOrg.Address.SetStatus(AddressStatuses.Active,OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId,"PIN Confirmed");
                userOrg.Organisation.LatestAddress = userOrg.Address;
                userOrg.ConfirmAttempts = 0;

                model.OrganisationId = userOrg.OrganisationId;
                model.OrganisationName = userOrg.Organisation.OrganisationName;
                StashModel(model);

                result1 = RedirectToAction("ServiceActivated");
            }
            else
            {
                userOrg.ConfirmAttempts++;
                AddModelError(3015, "PIN", new { OrganisationIdentifier=SharedBusinessLogic.Obfuscator.Obfuscate(id) });
                result1 = View("ActivateService", model);
            }

            userOrg.ConfirmAttemptDate = VirtualDateTime.Now;

            //Save the changes
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Log the registration
            await _registrationService.RegistrationLog.WriteAsync(
                new RegisterLogModel {
                    StatusDate = VirtualDateTime.Now,
                    Status = "PIN Confirmed",
                    ActionBy = VirtualUser.EmailAddress,
                    Details = "",
                    Sector = userOrg.Organisation.SectorType.ToString(),
                    Organisation = userOrg.Organisation.OrganisationName,
                    CompanyNo = userOrg.Organisation.CompanyNumber,
                    Address = userOrg?.Address.GetAddressString(),
                    SicCodes = userOrg.Organisation.GetLatestSicCodeIdsString(),
                    UserFirstname = userOrg.User.Firstname,
                    UserLastname = userOrg.User.Lastname,
                    UserJobtitle = userOrg.User.JobTitle,
                    UserEmail = userOrg.User.EmailAddress,
                    ContactFirstName = userOrg.User.ContactFirstName,
                    ContactLastName = userOrg.User.ContactLastName,
                    ContactJobTitle = userOrg.User.ContactJobTitle,
                    ContactOrganisation = userOrg.User.ContactOrganisation,
                    ContactPhoneNumber = userOrg.User.ContactPhoneNumber
                });

            //Add this organisation to the search index
            if (updateSearchIndex && !_registrationService.SearchBusinessLogic.SearchOptions.Disabled)
                await _registrationService.SearchBusinessLogic.RefreshSearchDocumentsAsync(userOrg.Organisation);

            //Prompt the user with confirmation
            return result1;
        }

        [Authorize]
        [HttpGet("service-activated")]
        public async Task<IActionResult> ServiceActivated()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var model = UnstashModel<CompleteViewModel>(true);
            if (model == null) return SessionExpiredView();

            //Ensure the stash is cleared
            ClearStash();

            //Show the confirmation view
            return View("ServiceActivated", model);
        }
    }
}