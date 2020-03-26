﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Registration;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Registration.Classes;
using ModernSlavery.WebUI.Registration.Models;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Options;
using ModernSlavery.WebUI.Submission.Classes;

namespace ModernSlavery.WebUI.Registration.Controllers
{
    [Route("Register")]
    public partial class RegistrationController : BaseController
    {

        public readonly IRegistrationService RegistrationService;

        #region Constructors

        public RegistrationController(
            IRegistrationService registrationService,
            ILogger<RegistrationController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            RegistrationService = registrationService;
        }

        #endregion

        #region Initialisation

        /// <summary>
        ///     This action is only used to warm up this controller on initialisation
        /// </summary>
        [HttpGet("Init")]
        public IActionResult Init()
        {
            return new EmptyResult();
        }

        #endregion

        #region Session & Cache Properties

        protected async Task<DateTime> GetLastPasswordResetDateAsync()
        {
            return await Cache.GetAsync<DateTime>($"{UserHostAddress}:LastPasswordResetDate");
        }

        protected async Task SetLastPasswordResetDateAsync(DateTime value)
        {
            await Cache.RemoveAsync($"{UserHostAddress}:LastPasswordResetDate");
            if (value > DateTime.MinValue)
            {
                await Cache.AddAsync($"{UserHostAddress}:LastPasswordResetDate", value, value.AddMinutes(SharedBusinessLogic.SharedOptions.MinPasswordResetMinutes));
            }
        }


        private int LastPrivateSearchRemoteTotal => Session["LastPrivateSearchRemoteTotal"].ToInt32();

        private int CompaniesHouseFailures
        {
            get => Session["CompaniesHouseFailures"].ToInt32();
            set
            {
                Session.Remove("CompaniesHouseFailures");
                if (value > 0)
                {
                    Session["CompaniesHouseFailures"] = value;
                }
            }
        }

        #endregion

        #region Home

        #endregion 

        #region PINSent

        private async Task<IActionResult> GetSendPINAsync()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null)
            {
                return checkResult;
            }

            //Get the user organisation
            UserOrganisation userOrg = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo => uo.UserId == VirtualUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            //If a pin has never been sent or resend button submitted then send one immediately
            if (string.IsNullOrWhiteSpace(userOrg.PIN) && string.IsNullOrWhiteSpace(userOrg.PINHash)
                || userOrg.PINSentDate.EqualsI(null, DateTime.MinValue)
                || userOrg.PINSentDate.Value.AddDays(SharedBusinessLogic.SharedOptions.PinInPostExpiryDays) < VirtualDateTime.Now)
            {
                try
                {
                    DateTime now = VirtualDateTime.Now;

                    // Check if we are a test user (for load testing)
                    bool thisIsATestUser = userOrg.User.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix);
                    bool pinInPostTestMode = SharedBusinessLogic.SharedOptions.PinInPostTestMode;

                    // Generate a new pin
                    string pin = RegistrationService.OrganisationBusinessLogic.GeneratePINCode(thisIsATestUser);

                    // Save the PIN and confirm code
                    userOrg.PIN = pin;
                    userOrg.PINHash = null;
                    userOrg.PINSentDate = now;
                    userOrg.Method = RegistrationMethods.PinInPost;
                    await SharedBusinessLogic.DataRepository.SaveChangesAsync();

                    if (thisIsATestUser || pinInPostTestMode)
                    {
                        ViewBag.PinCode = pin;
                    }
                    else 
                    {
                        // Try and send the PIN in post
                        string returnUrl = await WebService.RouteHelper.Get(UrlRouteOptions.Routes.SubmissionHome);
                        if (RegistrationService.PinInThePostService.SendPinInThePost(userOrg, pin,returnUrl, out string letterId))
                        {
                            userOrg.PITPNotifyLetterId = letterId;
                            await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                        }
                        else
                        {
                            // Show "Notify is down" error message
                            return View(
                                "PinFailedToSend",
                                new PinFailedToSendViewModel {OrganisationName = userOrg.Organisation.OrganisationName});
                        }
                    }

                    Logger.LogInformation(
                        "Send Pin-in-post",
                        $"Name {VirtualUser.Fullname}, Email:{VirtualUser.EmailAddress}, IP:{UserHostAddress}, Address:{userOrg?.Address.GetAddressString()}");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                    // TODO: maybe change this?
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(3014));
                }
            }

            //Prepare view parameters
            ViewBag.UserFullName = VirtualUser.Fullname;
            ViewBag.UserJobTitle = VirtualUser.JobTitle;
            ViewBag.Organisation = userOrg.Organisation.OrganisationName;
            ViewBag.Address = userOrg?.Address.GetAddressString(",<br/>");
            return View("PINSent");
        }

        [Authorize]
        [HttpGet("pin-sent")]
        public async Task<IActionResult> PINSent()
        {
            //Clear the stash
            this.ClearStash();

            return await GetSendPINAsync();
        }

        #endregion

        #region RequestPIN

        [HttpGet("request-pin")]
        [Authorize]
        public async Task<IActionResult> RequestPIN()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null)
            {
                return checkResult;
            }

            //Get the user organisation
            UserOrganisation userOrg = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo => uo.UserId == VirtualUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            //Prepare view parameters
            ViewBag.UserFullName = VirtualUser.Fullname;
            ViewBag.UserJobTitle = VirtualUser.JobTitle;
            ViewBag.Organisation = userOrg.Organisation.OrganisationName;
            ViewBag.Address = userOrg?.Address.GetAddressString(",<br/>");
            //Show the PIN textbox and button
            return View("RequestPIN");
        }

        [PreventDuplicatePost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("request-pin")]
        public async Task<IActionResult> RequestPIN(CompleteViewModel model)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null)
            {
                return checkResult;
            }

            //Get the user organisation
            UserOrganisation userOrg = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo => uo.UserId == VirtualUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            //Mark the user org as ready to send a pin
            userOrg.PIN = null;
            userOrg.PINHash = null;
            userOrg.PINSentDate = null;
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            return RedirectToAction("PINSent");
        }

        #endregion

        #region password-reset

        [HttpGet("password-reset")]
        public async Task<IActionResult> PasswordReset()
        {
            //Ensure IP address hasnt signed up recently
            DateTime lastPasswordResetDate = await GetLastPasswordResetDateAsync();
            TimeSpan remainingTime = lastPasswordResetDate == DateTime.MinValue
                ? TimeSpan.Zero
                : lastPasswordResetDate.AddMinutes(SharedBusinessLogic.SharedOptions.MinPasswordResetMinutes) - VirtualDateTime.Now;
            if (!SharedBusinessLogic.SharedOptions.SkipSpamProtection && remainingTime > TimeSpan.Zero)
            {
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1133, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
            }

            //Ensure user has not completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null)
            {
                return checkResult;
            }

            //Clear the stash
            this.ClearStash();

            //Start new user registration
            return View("PasswordReset", new ResetViewModel());
        }

        [SpamProtection(5)]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("password-reset")]
        public async Task<IActionResult> PasswordReset(ResetViewModel model)
        {
            ModelState.Remove(nameof(model.Password));
            ModelState.Remove(nameof(model.ConfirmPassword));

            //Ensure IP address hasnt signed up recently
            DateTime lastPasswordResetDate = await GetLastPasswordResetDateAsync();
            TimeSpan remainingTime = lastPasswordResetDate == DateTime.MinValue
                ? TimeSpan.Zero
                : lastPasswordResetDate.AddMinutes(SharedBusinessLogic.SharedOptions.MinPasswordResetMinutes) - VirtualDateTime.Now;
            if (!SharedBusinessLogic.SharedOptions.SkipSpamProtection && remainingTime > TimeSpan.Zero)
            {
                ModelState.AddModelError(3026, null, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)});
            }

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<RegisterViewModel>();
                return View("PasswordReset", model);
            }

            //Ensure user has not completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null)
            {
                return checkResult;
            }

            //Validate the submitted fields
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ResetViewModel>();
                return View("PasswordReset", model);
            }

            //Ensure email is always lower case
            model.EmailAddress = model.EmailAddress.ToLower();
            ViewBag.EmailAddress = model.EmailAddress;

            //Ensure signup is restricted to every 10 mins
            await SetLastPasswordResetDateAsync(
                model.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix) ? DateTime.MinValue : VirtualDateTime.Now);

            // find the latest active user by email
            var user= await RegistrationService.UserRepository.FindByEmailAsync(model.EmailAddress, UserStatuses.Active);
            if (user == null)
            {
                Logger.LogWarning(
                    "Password reset requested for unknown email address",
                    $"Email:{model.EmailAddress}, IP:{UserHostAddress}");
                return View("PasswordResetSent");
            }

            if (!await ResendPasswordResetAsync(user))
            {
                AddModelError(1122);
                this.CleanModelErrors<ResetViewModel>();
                return View("PasswordReset", model);
            }

            VirtualUser.ResetAttempts = 0;
            VirtualUser.ResetSendDate = VirtualDateTime.Now;
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //show confirmation
            ViewBag.EmailAddress = VirtualUser.EmailAddress;
            if (VirtualUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix))
            {
                ViewBag.TestUrl = Url.Action(
                    "NewPassword",
                    "Registration",
                    new {code = Encryption.EncryptQuerystring(VirtualUser.UserId + ":" + VirtualDateTime.Now.ToSmallDateTime())},
                    "https");
            }

            return View("PasswordResetSent");
        }

        private async Task<bool> ResendPasswordResetAsync(User VirtualUser)
        {
            //Send a password reset link to the email address
            string resetCode = null;
            try
            {
                resetCode = Encryption.EncryptQuerystring(VirtualUser.UserId + ":" + VirtualDateTime.Now.ToSmallDateTime());
                string resetUrl = Url.Action("NewPassword", "Account", new { code = resetCode }, "https");
                if (!await RegistrationService.SharedBusinessLogic.SendEmailService.SendResetPasswordNotificationAsync(resetUrl, VirtualUser.EmailAddress))
                    return false;

                Logger.LogInformation(
                    "Password reset sent",
                    $"Name {VirtualUser.Fullname}, Email:{VirtualUser.EmailAddress}, IP:{UserHostAddress}");
            }
            catch (Exception ex)
            {
                //Log the exception
                Logger.LogError(ex, ex.Message);
                return false;
            }

            return true;
        }

        [HttpGet("enter-new-password")]
        public async Task<IActionResult> NewPassword(string code = null)
        {
            //Ensure user has not completed the registration process
            var result = await CheckUserRegisteredOkAsync();
            if (result != null)
            {
                return result;
            }

            var passwordResult = UnwrapPasswordReset(code);
            if (passwordResult.Result != null)return passwordResult.Result;

            var model = new ResetViewModel();
            model.Resetcode = code;
            this.StashModel(model);

            //Start new user registration
            return View("NewPassword", model);
        }

        private (ActionResult Result, User User) UnwrapPasswordReset(string code)
        {
            User user = null;
            ActionResult result = null;
            long userId = 0;
            DateTime resetDate;
            try
            {
                code = Encryption.DecryptQuerystring(code);
                code = HttpUtility.UrlDecode(code);
                string[] args = code.SplitI(":");
                if (args.Length != 2)
                {
                    throw new ArgumentException("Too few parameters in password reset code");
                }

                userId = args[0].ToLong();
                if (userId == 0)
                {
                    throw new ArgumentException("Invalid user id in password reset code");
                }

                resetDate = args[1].FromSmallDateTime();
                if (resetDate == DateTime.MinValue)
                {
                    throw new ArgumentException("Invalid password reset date in password reset code");
                }
            }
            catch
            {
                return (View("CustomError", WebService.ErrorViewModelFactory.Create(1123)),null);
            }

            //Get the user oganisation
            user = SharedBusinessLogic.DataRepository.Get<User>(userId);

            if (user == null)
            {
                return (View("CustomError", WebService.ErrorViewModelFactory.Create(1124)),null);
            }

            if (resetDate.AddDays(1) < VirtualDateTime.Now)
            {
                return (View("CustomError", WebService.ErrorViewModelFactory.Create(1126)),null);
            }

            return (null,user);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("enter-new-password")]
        public async Task<IActionResult> NewPassword(ResetViewModel model)
        {
            //Ensure user has not completed the registration process
            var result = await CheckUserRegisteredOkAsync();
            if (result != null)
            {
                return result;
            }

            ModelState.Remove(nameof(model.EmailAddress));
            ModelState.Remove(nameof(model.ConfirmEmailAddress));

            //Validate the submitted fields
            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<ResetViewModel>();
                return View("NewPassword", model);
            }

            var m = this.UnstashModel<ResetViewModel>();
            if (m == null || string.IsNullOrWhiteSpace(m.Resetcode))return View("CustomError", WebService.ErrorViewModelFactory.Create(0));

            var passwordResult = UnwrapPasswordReset(m.Resetcode);
            if (passwordResult.Result != null)return passwordResult.Result;

            this.ClearStash();

            //Save the user to ensure UserId>0 for new status
            RegistrationService.UserRepository.UpdateUserPasswordUsingPBKDF2(passwordResult.User, model.Password);

            VirtualUser.ResetAttempts = 0;
            VirtualUser.ResetSendDate = null;
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            //Send completed notification email
            await RegistrationService.SharedBusinessLogic.SendEmailService.SendResetPasswordCompletedAsync(passwordResult.User.EmailAddress);

            //Send the verification code and showconfirmation
            return View("CustomError", WebService.ErrorViewModelFactory.Create(1127));
        }

        #endregion

    }
}
