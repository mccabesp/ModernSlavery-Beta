using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Registration.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Registration.Controllers
{
    public partial class RegistrationController : BaseController
    {
        [Authorize]
        [HttpGet("~/remove-organisation/{orgId}/{userId}")]
        public async Task<IActionResult> RemoveOrganisation([Obfuscated] string orgId, [Obfuscated] string userId)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // Decrypt org id
            long organisationId = SharedBusinessLogic.Obfuscator.DeObfuscate(orgId);
            if (organisationId == 0)
                return new HttpBadRequestResult($"Cannot decrypt organisation id {orgId}");

            // Check the current user has remove permission for this organisation
            var userOrg = VirtualUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
                return new HttpForbiddenResult(
                    $"User {VirtualUser?.EmailAddress} is not registered for organisation id {organisationId}");

            // Decrypt user id
            long userIdToRemove = SharedBusinessLogic.Obfuscator.DeObfuscate(userId);
            if (userIdToRemove == 0) return new HttpBadRequestResult($"Cannot decrypt user id {userId}");

            var userToRemove = VirtualUser;
            if (VirtualUser.UserId != userIdToRemove)
            {
                // Check the other user has permission to see this organisation
                var otherUserOrg =
                    userOrg.Organisation.UserOrganisations.FirstOrDefault(
                        uo => uo.OrganisationId == organisationId && uo.UserId == userIdToRemove);
                if (otherUserOrg == null)
                    return new HttpForbiddenResult(
                        $"User {userIdToRemove} is not registered for organisation id {organisationId}");

                userToRemove = otherUserOrg.User;
            }

            //Make sure they are fully registered for one before requesting another
            if (userOrg.PINConfirmedDate == null && userOrg.PINSentDate != null)
            {
                var remainingTime =
                    userOrg.PINSentDate.Value.AddMinutes(SharedBusinessLogic.SharedOptions.LockoutMinutes) -
                    VirtualDateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                    return View("CustomError",
                        WebService.ErrorViewModelFactory.Create(3023,
                            new { remainingTime = remainingTime.ToFriendly(maxParts: 2) }));
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
        [HttpPost("~/remove-organisation/{orgId}/{userId}")]
        public async Task<IActionResult> RemoveOrganisation(RemoveOrganisationModel model)
        {
            // Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // Decrypt org id
            var organisationId = SharedBusinessLogic.Obfuscator.DeObfuscate(model.EncOrganisationId);
            if (organisationId == 0) return new HttpBadRequestResult($"Cannot decrypt organisation id {model.EncOrganisationId}");

            // Check the current user has permission for this organisation
            var userOrgToUnregister = VirtualUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrgToUnregister == null)return new HttpForbiddenResult($"User {VirtualUser?.EmailAddress} is not registered for organisation id {organisationId}");

            // Decrypt user id
            long userIdToRemove = SharedBusinessLogic.Obfuscator.DeObfuscate(model.EncUserId);
            if (userIdToRemove == 0)
                return new HttpBadRequestResult($"Cannot decrypt user id '{model.EncUserId}'");

            var sourceOrg = userOrgToUnregister.Organisation;
            var userToUnregister = VirtualUser;
            if (VirtualUser.UserId != userIdToRemove)
            {
                // Ensure the other user has registered this organisation
                var otherUserOrg =
                    sourceOrg.UserOrganisations.FirstOrDefault(uo =>
                        uo.OrganisationId == organisationId && uo.UserId == userIdToRemove);
                if (otherUserOrg == null)
                    return new HttpForbiddenResult(
                        $"User {userIdToRemove} is not registered for organisation id {organisationId}");

                userToUnregister = otherUserOrg.User;
                userOrgToUnregister = otherUserOrg;
            }

            // Remove the registration
            var actionByUser = IsImpersonatingUser == false ? VirtualUser : OriginalUser;
            var orgToRemove = userOrgToUnregister.Organisation;

            await _registrationService.RegistrationBusinessLogic.RemoveRegistrationAsync(userOrgToUnregister,actionByUser);

            // Email the removed user 
            await SharedBusinessLogic.NotificationService.SendRemovedUserFromOrganisationEmailAsync(
                userToUnregister.EmailAddress,
                orgToRemove.OrganisationName,
                userToUnregister.Fullname,
                VirtualUser.Fullname);

            // Send the notification to GEO for each newly orphaned organisation
            if (_registrationService.OrganisationBusinessLogic.GetOrganisationIsOrphan(orgToRemove))
                await SharedBusinessLogic.SendEmailService.SendMsuOrphanOrganisationNotificationAsync(orgToRemove.OrganisationName);

            //Make sure this organisation is no longer selected
            if (ReportingOrganisationId == organisationId) ReportingOrganisationId = 0;

            // set fields before stashing
            model.OrganisationName = orgToRemove.OrganisationName;
            model.UserName = userToUnregister.Fullname;
            StashModel(model);

            return RedirectToAction("RemoveOrganisationCompleted");
        }

        [Authorize]
        [HttpGet("~/remove-organisation-completed")]
        public IActionResult RemoveOrganisationCompleted()
        {
            // Unstash and clear the remove model
            var model = UnstashModel<RemoveOrganisationModel>(true);

            // When model is null then return session expired view
            if (model == null) return SessionExpiredView();

            return View(model);
        }
    }
}