﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Registration.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Registration.Controllers
{
    public partial class RegistrationController : BaseController
    {

        private async Task<DateTime> GetLastSignupDateAsync()
        {
            return await Cache.GetAsync<DateTime>($"{UserHostAddress}:LastSignupDate");
        }

        private async Task SetLastSignupDateAsync(DateTime value)
        {
            await Cache.RemoveAsync($"{UserHostAddress}:LastSignupDate");
            if (value > DateTime.MinValue)
            {
                await Cache.AddAsync($"{UserHostAddress}:LastSignupDate", value, value.AddMinutes(SharedBusinessLogic.SharedOptions.MinSignupMinutes));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Redirect()
        {
            await TrackPageViewAsync();

            return RedirectToActionPermanent("AboutYou");
        }

        [HttpGet("about-you")]
        public async Task<IActionResult> AboutYou()
        {
            DateTime lastSignupDate = await GetLastSignupDateAsync();
            TimeSpan remainingTime = lastSignupDate == DateTime.MinValue
                ? TimeSpan.Zero
                : lastSignupDate.AddMinutes(SharedBusinessLogic.SharedOptions.MinSignupMinutes) - VirtualDateTime.Now;
            if (!SharedBusinessLogic.SharedOptions.SkipSpamProtection && remainingTime > TimeSpan.Zero)
            {
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1125, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
            }

            //Ensure user has not completed the registration process
            var result = await CheckUserRegisteredOkAsync();
            if (result != null)
            {
                return result;
            }

            //Clear the stash
            this.ClearStash();

            var model = new RegisterViewModel();

            //Prepopulate with name and email saved during out-of-scope process
            string pendingFasttrackCodes = PendingFasttrackCodes;
            if (pendingFasttrackCodes != null)
            {
                string[] args = pendingFasttrackCodes?.SplitI(":");
                if (args.Length > 2)
                {
                    model.FirstName = args[2];
                }

                if (args.Length > 3)
                {
                    model.LastName = args[3];
                }

                if (args.Length > 4)
                {
                    model.EmailAddress = args[4];
                }

                model.ConfirmEmailAddress = model.EmailAddress;
            }

            //Start new user registration
            return View("AboutYou", model);
        }

        [HttpPost("about-you")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [SpamProtection]
        public async Task<IActionResult> AboutYou(RegisterViewModel model)
        {
            //Ensure IP address hasnt signed up recently
            DateTime lastSignupDate = await GetLastSignupDateAsync();
            TimeSpan remainingTime = lastSignupDate == DateTime.MinValue
                ? TimeSpan.Zero
                : lastSignupDate.AddMinutes(SharedBusinessLogic.SharedOptions.MinSignupMinutes) - VirtualDateTime.Now;
            if (!SharedBusinessLogic.SharedOptions.SkipSpamProtection && remainingTime > TimeSpan.Zero)
            {
                ModelState.AddModelError(3024, null, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)});
            }

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<RegisterViewModel>();
                return View("AboutYou", model);
            }

            //Validate the submitted fields
            if (model.Password.ContainsI("password"))
            {
                AddModelError(3000, "Password");
            }

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<RegisterViewModel>();
                return View("AboutYou", model);
            }

            //Ensure email is always lower case
            model.EmailAddress = model.EmailAddress.ToLower();

            //Check this email address isn't already assigned to another user
            User VirtualUser = await RegistrationService.UserRepository.FindByEmailAsync(model.EmailAddress, UserStatuses.New, UserStatuses.Active);
            if (VirtualUser != null)
            {
                if (VirtualUser.EmailVerifySendDate != null)
                {
                    if (VirtualUser.EmailVerifiedDate != null)
                    {
                        //A registered user with this email already exists.
                        AddModelError(3001, "EmailAddress");
                        this.CleanModelErrors<RegisterViewModel>();
                        return View("AboutYou", model);
                    }

                    remainingTime = VirtualUser.EmailVerifySendDate.Value.AddHours(SharedBusinessLogic.SharedOptions.EmailVerificationExpiryHours)
                                    - VirtualDateTime.Now;
                    if (remainingTime > TimeSpan.Zero)
                    {
                        AddModelError(3002, "EmailAddress", new {remainingTime = remainingTime.ToFriendly(maxParts: 2)});
                        this.CleanModelErrors<RegisterViewModel>();
                        return View("AboutYou", model);
                    }
                }

                //Delete the previous user org if there is one
                UserOrganisation userOrg = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo => uo.UserId == VirtualUser.UserId);
                if (userOrg != null)
                {
                    SharedBusinessLogic.DataRepository.Delete(userOrg);
                }

                //If from a previous user then delete the previous user
                SharedBusinessLogic.DataRepository.Delete(VirtualUser);
            }

            //Save the submitted fields
            VirtualUser = new User();
            VirtualUser.Created = VirtualDateTime.Now;
            VirtualUser.Modified = VirtualUser.Created;
            VirtualUser.Firstname = model.FirstName;
            VirtualUser.Lastname = model.LastName;
            VirtualUser.JobTitle = model.JobTitle;
            if (model.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
            {
                VirtualUser._EmailAddress = model.EmailAddress;
            }
            else
            {
                VirtualUser.EmailAddress = model.EmailAddress;
            }

            if (!VirtualUser.IsAdministrator())
            {
                VirtualUser.SetSetting(UserSettingKeys.AllowContact, model.AllowContact.ToString());
                VirtualUser.SetSetting(UserSettingKeys.SendUpdates, model.SendUpdates.ToString());
            }

            RegistrationService.UserRepository.UpdateUserPasswordUsingPBKDF2(VirtualUser, model.Password);

            VirtualUser.EmailVerifySendDate = null;
            VirtualUser.EmailVerifiedDate = null;
            VirtualUser.EmailVerifyHash = null;

            //Save the user with new status
            VirtualUser.SetStatus(UserStatuses.New, OriginalUser ?? VirtualUser);

            // save the current user
            SharedBusinessLogic.DataRepository.Insert(VirtualUser);
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Save pendingFasttrackCodes
            string pendingFasttrackCodes = PendingFasttrackCodes;
            if (pendingFasttrackCodes != null)
            {
                string[] args = pendingFasttrackCodes?.SplitI(":");
                pendingFasttrackCodes = $"{args[0]}:{args[1]}";
                VirtualUser.SetSetting(UserSettingKeys.PendingFasttrackCodes, pendingFasttrackCodes);
                await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                PendingFasttrackCodes = null;
            }

            //Send the verification code and show confirmation
            this.StashModel(model);

            //Ensure signup is restricted to every 10 min
            await SetLastSignupDateAsync(model.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix) ? DateTime.MinValue : VirtualDateTime.Now);

            return RedirectToAction("VerifyEmail");
        }

    }
}
