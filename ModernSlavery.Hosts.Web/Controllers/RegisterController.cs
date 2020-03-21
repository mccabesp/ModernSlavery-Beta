using System;
using System.Threading.Tasks;
using System.Web;
using ModernSlavery.BusinessLogic;
using ModernSlavery.Extensions;
using ModernSlavery.WebUI.Models.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessLogic.Register;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.Entities;
using ModernSlavery.Entities.Enums;
using ModernSlavery.WebUI.Presenters;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Controllers
{
    [Route("Register")]
    public partial class RegisterController : BaseController
    {

        public readonly IRegistrationService RegistrationService;

        #region Constructors

        public RegisterController(
            IRegistrationService registrationService,
            IScopePresenter scopePresentation,
            ILogger<RegisterController> logger, IWebService webService, ICommonBusinessLogic commonBusinessLogic) : base(logger, webService, commonBusinessLogic)
        {
            RegistrationService = registrationService;
            ScopePresentation = scopePresentation;
        }

        #endregion

        #region Dependencies
        public IScopePresenter ScopePresentation { get; }

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
                await Cache.AddAsync($"{UserHostAddress}:LastPasswordResetDate", value, value.AddMinutes(CommonBusinessLogic.GlobalOptions.MinPasswordResetMinutes));
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
        
        #region PINSent

        private async Task<IActionResult> GetSendPINAsync()
        {
            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Get the user organisation
            UserOrganisation userOrg = await CommonBusinessLogic.DataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.UserId == currentUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            //If a pin has never been sent or resend button submitted then send one immediately
            if (string.IsNullOrWhiteSpace(userOrg.PIN) && string.IsNullOrWhiteSpace(userOrg.PINHash)
                || userOrg.PINSentDate.EqualsI(null, DateTime.MinValue)
                || userOrg.PINSentDate.Value.AddDays(CommonBusinessLogic.GlobalOptions.PinInPostExpiryDays) < VirtualDateTime.Now)
            {
                try
                {
                    DateTime now = VirtualDateTime.Now;

                    // Check if we are a test user (for load testing)
                    bool thisIsATestUser = userOrg.User.EmailAddress.StartsWithI(CommonBusinessLogic.GlobalOptions.TestPrefix);
                    bool pinInPostTestMode = CommonBusinessLogic.GlobalOptions.PinInPostTestMode;

                    // Generate a new pin
                    string pin = RegistrationService.OrganisationBusinessLogic.GeneratePINCode(thisIsATestUser);

                    // Save the PIN and confirm code
                    userOrg.PIN = pin;
                    userOrg.PINHash = null;
                    userOrg.PINSentDate = now;
                    userOrg.Method = RegistrationMethods.PinInPost;
                    await CommonBusinessLogic.DataRepository.SaveChangesAsync();

                    if (thisIsATestUser || pinInPostTestMode)
                    {
                        ViewBag.PinCode = pin;
                    }
                    else 
                    {
                        // Try and send the PIN in post
                        string returnUrl = Url.Action(nameof(OrganisationController.ManageOrganisations),"Organisation", null, "https");
                        if (RegistrationService.PinInThePostService.SendPinInThePost(userOrg, pin,returnUrl, out string letterId))
                        {
                            userOrg.PITPNotifyLetterId = letterId;
                            await CommonBusinessLogic.DataRepository.SaveChangesAsync();
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
                        $"Name {currentUser.Fullname}, Email:{currentUser.EmailAddress}, IP:{UserHostAddress}, Address:{userOrg?.Address.GetAddressString()}");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                    // TODO: maybe change this?
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(3014));
                }
            }

            //Prepare view parameters
            ViewBag.UserFullName = currentUser.Fullname;
            ViewBag.UserJobTitle = currentUser.JobTitle;
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
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Get the user organisation
            UserOrganisation userOrg = await CommonBusinessLogic.DataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.UserId == currentUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            //Prepare view parameters
            ViewBag.UserFullName = currentUser.Fullname;
            ViewBag.UserJobTitle = currentUser.JobTitle;
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
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult != null)
            {
                return checkResult;
            }

            //Get the user organisation
            UserOrganisation userOrg = await CommonBusinessLogic.DataRepository.GetAll<UserOrganisation>()
                .FirstOrDefaultAsync(uo => uo.UserId == currentUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            //Mark the user org as ready to send a pin
            userOrg.PIN = null;
            userOrg.PINHash = null;
            userOrg.PINSentDate = null;
            await CommonBusinessLogic.DataRepository.SaveChangesAsync();

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
                : lastPasswordResetDate.AddMinutes(CommonBusinessLogic.GlobalOptions.MinPasswordResetMinutes) - VirtualDateTime.Now;
            if (!CommonBusinessLogic.GlobalOptions.SkipSpamProtection && remainingTime > TimeSpan.Zero)
            {
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1133, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
            }

            User currentUser;
            //Ensure user has not completed the registration process
            IActionResult result = CheckUserRegisteredOk(out currentUser);
            if (result != null)
            {
                return result;
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
                : lastPasswordResetDate.AddMinutes(CommonBusinessLogic.GlobalOptions.MinPasswordResetMinutes) - VirtualDateTime.Now;
            if (!CommonBusinessLogic.GlobalOptions.SkipSpamProtection && remainingTime > TimeSpan.Zero)
            {
                ModelState.AddModelError(3026, null, new {remainingTime = remainingTime.ToFriendly(maxParts: 2)});
            }

            if (!ModelState.IsValid)
            {
                this.CleanModelErrors<RegisterViewModel>();
                return View("PasswordReset", model);
            }

            //Ensure user has not completed the registration process
            IActionResult result = CheckUserRegisteredOk(out User currentUser);
            if (result != null)
            {
                return result;
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
                model.EmailAddress.StartsWithI(CommonBusinessLogic.GlobalOptions.TestPrefix) ? DateTime.MinValue : VirtualDateTime.Now);

            // find the latest active user by email
            currentUser = await RegistrationService.UserRepository.FindByEmailAsync(model.EmailAddress, UserStatuses.Active);
            if (currentUser == null)
            {
                Logger.LogWarning(
                    "Password reset requested for unknown email address",
                    $"Email:{model.EmailAddress}, IP:{UserHostAddress}");
                return View("PasswordResetSent");
            }

            if (!await ResendPasswordResetAsync(currentUser))
            {
                AddModelError(1122);
                this.CleanModelErrors<ResetViewModel>();
                return View("PasswordReset", model);
            }

            currentUser.ResetAttempts = 0;
            currentUser.ResetSendDate = VirtualDateTime.Now;
            await CommonBusinessLogic.DataRepository.SaveChangesAsync();

            //show confirmation
            ViewBag.EmailAddress = currentUser.EmailAddress;
            if (currentUser.EmailAddress.StartsWithI(CommonBusinessLogic.GlobalOptions.TestPrefix))
            {
                ViewBag.TestUrl = Url.Action(
                    "NewPassword",
                    "Register",
                    new {code = Encryption.EncryptQuerystring(currentUser.UserId + ":" + VirtualDateTime.Now.ToSmallDateTime())},
                    "https");
            }

            return View("PasswordResetSent");
        }

        private async Task<bool> ResendPasswordResetAsync(User currentUser)
        {
            //Send a password reset link to the email address
            string resetCode = null;
            try
            {
                resetCode = Encryption.EncryptQuerystring(currentUser.UserId + ":" + VirtualDateTime.Now.ToSmallDateTime());
                string resetUrl = Url.Action("NewPassword", "Register", new { code = resetCode }, "https");
                if (!await RegistrationService.CommonBusinessLogic.SendEmailService.SendResetPasswordNotificationAsync(resetUrl, currentUser.EmailAddress))
                    return false;

                Logger.LogInformation(
                    "Password reset sent",
                    $"Name {currentUser.Fullname}, Email:{currentUser.EmailAddress}, IP:{UserHostAddress}");
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
        public IActionResult NewPassword(string code = null)
        {
            User currentUser;
            //Ensure user has not completed the registration process
            IActionResult result = CheckUserRegisteredOk(out currentUser);
            if (result != null)
            {
                return result;
            }

            result = UnwrapPasswordReset(code, out currentUser);
            if (result != null)
            {
                return result;
            }

            var model = new ResetViewModel();
            model.Resetcode = code;
            this.StashModel(model);

            //Start new user registration
            return View("NewPassword", model);
        }

        private ActionResult UnwrapPasswordReset(string code, out User user)
        {
            user = null;

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
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1123));
            }

            //Get the user oganisation
            user = CommonBusinessLogic.DataRepository.Get<User>(userId);

            if (user == null)
            {
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1124));
            }

            if (resetDate.AddDays(1) < VirtualDateTime.Now)
            {
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1126));
            }

            return null;
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("enter-new-password")]
        public async Task<IActionResult> NewPassword(ResetViewModel model)
        {
            User currentUser;
            //Ensure user has not completed the registration process
            IActionResult result = CheckUserRegisteredOk(out currentUser);
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
            if (m == null || string.IsNullOrWhiteSpace(m.Resetcode))
            {
                return View("CustomError", WebService.ErrorViewModelFactory.Create(0));
            }

            result = UnwrapPasswordReset(m.Resetcode, out currentUser);
            if (result != null)
            {
                return result;
            }

            this.ClearStash();

            //Save the user to ensure UserId>0 for new status
            RegistrationService.UserRepository.UpdateUserPasswordUsingPBKDF2(currentUser, model.Password);

            currentUser.ResetAttempts = 0;
            currentUser.ResetSendDate = null;
            await CommonBusinessLogic.DataRepository.SaveChangesAsync();

            //Send completed notification email
            await RegistrationService.CommonBusinessLogic.SendEmailService.SendResetPasswordCompletedAsync(currentUser.EmailAddress);

            //Send the verification code and showconfirmation
            return View("CustomError", WebService.ErrorViewModelFactory.Create(1127));
        }

        #endregion

    }
}
