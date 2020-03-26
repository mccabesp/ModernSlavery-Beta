using System;
using System.Linq;
using System.Security.Authentication;
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
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Models.HttpResultModels;

namespace ModernSlavery.WebUI.Registration.Controllers
{
    public partial class RegistrationController : BaseController
    {

        [Authorize]
        [HttpGet("service-activated")]
        public async Task<IActionResult> ServiceActivated()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null)
            {
                return checkResult;
            }

            var model = this.UnstashModel<CompleteViewModel>(true);
            if (model == null)
            {
                return SessionExpiredView();
            }

            //Ensure the stash is cleared
            this.ClearStash();

            ReportingOrganisationId = model.OrganisationId;
            ViewBag.OrganisationName = ReportingOrganisation.OrganisationName;

            //Show the confirmation view
            return View("ServiceActivated");
        }

        [Authorize]
        [HttpGet("activate-service")]
        public async Task<IActionResult> ActivateService()
        {
            //Ensure user has completed the registration process
            
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null)
            {
                return checkResult;
            }

            //Get the user organisation
            UserOrganisation userOrg = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo => uo.UserId == VirtualUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            if (userOrg == null)
            {
                throw new AuthenticationException();
            }

            //Ensure they havent entered wrong pin too many times
            TimeSpan remaining = userOrg.ConfirmAttemptDate == null
                ? TimeSpan.Zero
                : userOrg.ConfirmAttemptDate.Value.AddMinutes(SharedBusinessLogic.SharedOptions.LockoutMinutes) - VirtualDateTime.Now;
            if (userOrg.ConfirmAttempts >= SharedBusinessLogic.SharedOptions.MaxPinAttempts && remaining > TimeSpan.Zero)
            {
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1113, new {remainingTime = remaining.ToFriendly(maxParts: 2)}));
            }

            remaining = userOrg.PINSentDate == null
                ? TimeSpan.Zero
                : userOrg.PINSentDate.Value.AddDays(SharedBusinessLogic.SharedOptions.PinInPostMinRepostDays) - VirtualDateTime.Now;
            var model = new CompleteViewModel();

            model.PIN = null;
            model.AllowResend = remaining <= TimeSpan.Zero;
            model.Remaining = remaining.ToFriendly(maxParts: 2);

            //If the email address is a test email then simulate sending
            if (userOrg.User.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
            {
                model.PIN = "ABCDEF";
            }

            //Show the PIN textbox and button
            return View("ActivateService", model);
        }

        [PreventDuplicatePost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("activate-service")]
        public async Task<IActionResult> ActivateService(CompleteViewModel model)
        {
            //Ensure user has completed the registration process
            
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null)
            {
                return checkResult;
            }

            //Ensure they have entered a PIN
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<CompleteViewModel>();
                return View("ActivateService", model);
            }

            //Get the user organisation
            UserOrganisation userOrg = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo => uo.UserId == VirtualUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            ActionResult result1;

            TimeSpan remaining = userOrg.ConfirmAttemptDate == null
                ? TimeSpan.Zero
                : userOrg.ConfirmAttemptDate.Value.AddMinutes(SharedBusinessLogic.SharedOptions.LockoutMinutes) - VirtualDateTime.Now;
            if (userOrg.ConfirmAttempts >= SharedBusinessLogic.SharedOptions.MaxPinAttempts && remaining > TimeSpan.Zero)
            {
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1113, new {remainingTime = remaining.ToFriendly(maxParts: 2)}));
            }

            var updateSearchIndex = false;
            if (PinMatchesPinInDatabase(userOrg, model.PIN))
            {
                //Set the user org as confirmed
                userOrg.PINConfirmedDate = VirtualDateTime.Now;

                //Set the pending organisation to active
                //Make sure the found organisation is active or pending

                if (userOrg.Organisation.Status.IsAny(OrganisationStatuses.Pending, OrganisationStatuses.Active))
                {
                    userOrg.Organisation.SetStatus(
                        OrganisationStatuses.Active,
                        OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId,
                        "PIN Confirmed");
                    updateSearchIndex = true;
                }
                else
                {
                    Logger.LogWarning(
                        $"Attempt to PIN activate a {userOrg.Organisation.Status} organisation",
                        $"Organisation: '{userOrg.Organisation.OrganisationName}' Reference: '{userOrg.Organisation.EmployerReference}' User: '{VirtualUser.EmailAddress}'");
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1149));
                }

                //Set the latest registration
                userOrg.Organisation.LatestRegistration = userOrg;

                //Retire the old address 
                if (userOrg.Organisation.LatestAddress != null && userOrg.Organisation.LatestAddress.AddressId != userOrg.Address.AddressId)
                {
                    userOrg.Organisation.LatestAddress.SetStatus(
                        AddressStatuses.Retired,
                        OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId,
                        "Replaced by PIN in post");
                    updateSearchIndex = true;
                }

                //Activate the address the pin was sent to
                userOrg.Address.SetStatus(
                    AddressStatuses.Active,
                    OriginalUser == null ? VirtualUser.UserId : OriginalUser.UserId,
                    "PIN Confirmed");
                userOrg.Organisation.LatestAddress = userOrg.Address;
                userOrg.ConfirmAttempts = 0;

                model.AccountingDate = RegistrationService.SharedBusinessLogic.GetAccountingStartDate(userOrg.Organisation.SectorType);
                model.OrganisationId = userOrg.OrganisationId;
                this.StashModel(model);

                result1 = RedirectToAction("ServiceActivated");

                //Send notification email to existing users 
                RegistrationService.SharedBusinessLogic.NotificationService.SendUserAddedEmailToExistingUsers(userOrg.Organisation, userOrg.User);
            }
            else
            {
                userOrg.ConfirmAttempts++;
                AddModelError(3015, "PIN");
                result1 = View("ActivateService", model);
            }

            userOrg.ConfirmAttemptDate = VirtualDateTime.Now;

            //Save the changes
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Log the registration
            if (!userOrg.User.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
            {
                await RegistrationService.RegistrationLog.WriteAsync(
                    new RegisterLogModel {
                        StatusDate = VirtualDateTime.Now,
                        Status = "PIN Confirmed",
                        ActionBy = VirtualUser.EmailAddress,
                        Details = "",
                        Sector = userOrg.Organisation.SectorType,
                        Organisation = userOrg.Organisation.OrganisationName,
                        CompanyNo = userOrg.Organisation.CompanyNumber,
                        Address = userOrg?.Address.GetAddressString(),
                        SicCodes = userOrg.Organisation.GetSicCodeIdsString(),
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
            }

            //Add this organisation to the search index
            if (updateSearchIndex)
            {
                await RegistrationService.SearchBusinessLogic.UpdateSearchIndexAsync(userOrg.Organisation);
            }

            //Prompt the user with confirmation
            return result1;
        }

        private static bool PinMatchesPinInDatabase(UserOrganisation userOrg, string modelPin)
        {
            if (modelPin == null)
            {
                return false;
            }

            string normalisedPin = modelPin.Trim().ToUpper();
            if (!string.IsNullOrWhiteSpace(userOrg.PIN) && userOrg.PIN == normalisedPin)
            {
                return true;
            }

            string hashedPin = Crypto.GetSHA512Checksum(normalisedPin);
            if (userOrg.PINHash == hashedPin)
            {
                return true;
            }

            return false;
        }

        [Authorize]
        [HttpGet("~/activate-organisation/{id}")]
        public async Task<IActionResult> ActivateOrganisation(string id)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null)
            {
                return checkResult;
            }

            // Decrypt org id
            if (!id.DecryptToId(out long organisationId))
            {
                return new HttpBadRequestResult($"Cannot decrypt organisation id {id}");
            }

            // Check the user has permission for this organisation
            UserOrganisation userOrg = VirtualUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {VirtualUser?.EmailAddress} is not registered for organisation id {organisationId}");
            }

            // Ensure this organisation needs activation on the users account
            if (userOrg.PINConfirmedDate != null)
            {
                throw new Exception(
                    $"Attempt to activate organisation {userOrg.OrganisationId}:'{userOrg.Organisation.OrganisationName}' for {VirtualUser.EmailAddress} by '{(OriginalUser == null ? VirtualUser.EmailAddress : OriginalUser.EmailAddress)}' which has already been activated");
            }

            // begin ActivateService journey
            ReportingOrganisationId = organisationId;
            return RedirectToAction("ActivateService", "Registration");
        }
    }
}
