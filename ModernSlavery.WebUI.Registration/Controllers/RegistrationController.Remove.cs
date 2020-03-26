using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.BusinessDomain.Registration;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
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
        [HttpGet("~/remove-organisation/{orgId}/{userId}")]
        public async Task<IActionResult> RemoveOrganisation(string orgId, string userId)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null)
            {
                return checkResult;
            }

            // Decrypt org id
            if (!orgId.DecryptToId(out long organisationId))
            {
                return new HttpBadRequestResult($"Cannot decrypt organisation id {orgId}");
            }

            // Check the current user has remove permission for this organisation
            UserOrganisation userOrg = VirtualUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
            {
                return new HttpForbiddenResult($"User {VirtualUser?.EmailAddress} is not registered for organisation id {organisationId}");
            }

            // Decrypt user id
            if (!userId.DecryptToId(out long userIdToRemove))
            {
                return new HttpBadRequestResult($"Cannot decrypt user id {userId}");
            }

            User userToRemove = VirtualUser;
            if (VirtualUser.UserId != userIdToRemove)
            {
                // Check the other user has permission to see this organisation
                UserOrganisation otherUserOrg =
                    userOrg.Organisation.UserOrganisations.FirstOrDefault(
                        uo => uo.OrganisationId == organisationId && uo.UserId == userIdToRemove);
                if (otherUserOrg == null)
                {
                    return new HttpForbiddenResult($"User {userIdToRemove} is not registered for organisation id {organisationId}");
                }

                userToRemove = otherUserOrg.User;
            }

            //Make sure they are fully registered for one before requesting another
            if (userOrg.PINConfirmedDate == null && userOrg.PINSentDate != null)
            {
                TimeSpan remainingTime = userOrg.PINSentDate.Value.AddMinutes(SharedBusinessLogic.SharedOptions.LockoutMinutes) - VirtualDateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                {
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(3023, new { remainingTime = remainingTime.ToFriendly(maxParts: 2) }));
                }
            }

            // build the view model
            var model = new RemoveOrganisationModel
            {
                EncOrganisationId = orgId,
                EncUserId = userId,
                OrganisationName = userOrg.Organisation.OrganisationName,
                OrganisationAddress = userOrg.Organisation.LatestAddress.GetAddressString(),
                UserName = userToRemove.Fullname
            };

            //Return the confirmation page
            return View("ConfirmRemove", model);
        }

        [PreventDuplicatePost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("~/remove-organisation")]
        public async Task<IActionResult> RemoveOrganisation(RemoveOrganisationModel model)
        {
            // Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null)
            {
                return checkResult;
            }

            // Decrypt org id
            if (!model.EncOrganisationId.DecryptToId(out long organisationId))
            {
                return new HttpBadRequestResult($"Cannot decrypt organisation id {model.EncOrganisationId}");
            }

            // Check the current user has permission for this organisation
            UserOrganisation userOrgToUnregister = VirtualUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrgToUnregister == null)
            {
                return new HttpForbiddenResult($"User {VirtualUser?.EmailAddress} is not registered for organisation id {organisationId}");
            }

            // Decrypt user id
            if (!model.EncUserId.DecryptToId(out long userIdToRemove))
            {
                return new HttpBadRequestResult($"Cannot decrypt user id '{model.EncUserId}'");
            }

            Organisation sourceOrg = userOrgToUnregister.Organisation;
            User userToUnregister = VirtualUser;
            if (VirtualUser.UserId != userIdToRemove)
            {
                // Ensure the other user has registered this organisation
                UserOrganisation otherUserOrg =
                    sourceOrg.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId && uo.UserId == userIdToRemove);
                if (otherUserOrg == null)
                {
                    return new HttpForbiddenResult($"User {userIdToRemove} is not registered for organisation id {organisationId}");
                }

                userToUnregister = otherUserOrg.User;
                userOrgToUnregister = otherUserOrg;
            }

            // Remove the registration
            User actionByUser = IsImpersonatingUser == false ? VirtualUser : OriginalUser;
            Organisation orgToRemove = userOrgToUnregister.Organisation;
            await RegistrationService.RegistrationBusinessLogic.RemoveRegistrationAsync(userOrgToUnregister, actionByUser);

            // Email user that has been unregistered
            SharedBusinessLogic.NotificationService.SendRemovedUserFromOrganisationEmail(
                userToUnregister.EmailAddress,
                orgToRemove.OrganisationName,
                userToUnregister.Fullname);

            // Email the other users of the organisation
            IEnumerable<string> emailAddressesForOrganisation = orgToRemove.UserOrganisations.Select(uo => uo.User.EmailAddress);
            foreach (string emailAddress in emailAddressesForOrganisation)
            {
                SharedBusinessLogic.NotificationService.SendRemovedUserFromOrganisationEmail(
                    emailAddress,
                    orgToRemove.OrganisationName,
                    userToUnregister.Fullname);
            }

            // Send the notification to GEO for each newly orphaned organisation
            if (!userToUnregister.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
            {
                var sendEmails = new List<Task>();
                bool testEmail = !SharedBusinessLogic.SharedOptions.IsProduction();
                if (orgToRemove.GetIsOrphan())
                {
                    sendEmails.Add(SharedBusinessLogic.SendEmailService.SendGEOOrphanOrganisationNotificationAsync(orgToRemove.OrganisationName, testEmail));
                }

                await Task.WhenAll(sendEmails);
            }

            //Make sure this organisation is no longer selected
            if (ReportingOrganisationId == organisationId)
            {
                ReportingOrganisationId = 0;
            }

            this.StashModel(model);

            return RedirectToAction("RemoveOrganisationCompleted");
        }

        [Authorize]
        [HttpGet("~/remove-organisation-completed")]
        public IActionResult RemoveOrganisationCompleted()
        {
            // Unstash and clear the remove model
            var model = this.UnstashModel<RemoveOrganisationModel>(typeof(RegistrationController), true);

            // When model is null then return session expired view
            if (model == null)
            {
                return SessionExpiredView();
            }

            return View(model);
        }
    }
}
