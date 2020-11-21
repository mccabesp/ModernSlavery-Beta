using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Registration.Models;
using ModernSlavery.WebUI.Registration.Presenters;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.UrlHelper;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Options;

namespace ModernSlavery.WebUI.Registration.Controllers
{
    [Area("Registration")]
    [Route("Register")]
    public partial class RegistrationController : BaseController
    {
        private readonly IRegistrationService _registrationService;
        private readonly IRegistrationPresenter _registrationPresenter;
        private readonly IPostcodeChecker _postcodeChecker;

        #region Constructors

        public RegistrationController(
            IRegistrationService registrationService,
            IRegistrationPresenter registrationPresenter,
            IPostcodeChecker postcodeChecker,
            ILogger<RegistrationController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) :base(logger, webService, sharedBusinessLogic)
        {
            _registrationService = registrationService;
            _registrationPresenter = registrationPresenter;
            _postcodeChecker = postcodeChecker;
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

        private int LastPrivateSearchRemoteTotal => Session["LastPrivateSearchRemoteTotal"].ToInt32();

        private int CompaniesHouseFailures
        {
            get => Session["CompaniesHouseFailures"].ToInt32();
            set
            {
                Session.Remove("CompaniesHouseFailures");
                if (value > 0) Session["CompaniesHouseFailures"] = value;
            }
        }

        #endregion

        #region Home
        [HttpGet]
        public async Task<IActionResult> Redirect()
        {
            await TrackPageViewAsync();

            return RedirectToActionAreaPermanent("AboutYou","Account", "Account");
        }

        #endregion

        #region PINSent

        private async Task<IActionResult> GetSendPINAsync()
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Get the user organisation
            var userOrg = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo =>
                uo.UserId == VirtualUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            //If a pin has never been sent or resend button submitted then send one immediately
            if (string.IsNullOrWhiteSpace(userOrg.PIN) && string.IsNullOrWhiteSpace(userOrg.PINHash)
                || userOrg.PINSentDate.EqualsI(null, DateTime.MinValue)
                || userOrg.PINSentDate.Value.AddDays(SharedBusinessLogic.SharedOptions.PinInPostExpiryDays) <
                VirtualDateTime.Now)
                try
                {
                    var now = VirtualDateTime.Now;

                    // Generate a new pin
                    var pin = _registrationService.OrganisationBusinessLogic.GeneratePINCode();

                    // Save the PIN and confirm code
                    userOrg.PIN = pin;
                    userOrg.PINHash = Crypto.GetSHA512Checksum(pin);
                    userOrg.PINSentDate = now;
                    userOrg.Method = RegistrationMethods.PinInPost;
                    await SharedBusinessLogic.DataRepository.SaveChangesAsync();

                    // Check if we are a test user (for load testing)
                    if (SharedBusinessLogic.TestOptions.PinInPostTestMode || userOrg.User.EmailAddress.StartsWithI(SharedBusinessLogic.TestOptions.TestPrefix))
                    {
                        ViewBag.PinCode = pin;
                        ViewBag.OrganisationIdentifier = SharedBusinessLogic.Obfuscator.Obfuscate(userOrg.OrganisationId);
                    }
                    else
                    {
                        // Try and send the PIN in post
                        var returnUrl = Url.ActionArea("ManageOrganisations", "Submission", "Submission", null, "https");
                        if (_registrationService.PinInThePostService.SendPinInThePost(userOrg, pin, returnUrl,
                            out var letterId))
                        {
                            userOrg.PITPNotifyLetterId = letterId;
                            await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                        }
                        else
                        {
                            // Show "Notify is down" error message
                            return View(
                                "PinFailedToSend",
                                new PinFailedToSendViewModel
                                    {OrganisationName = userOrg.Organisation.OrganisationName});
                        }
                    }

                    Logger.LogInformation($"Send Pin-in-post. Name {VirtualUser.Fullname}, Email:{VirtualUser.EmailAddress}, IP:{UserHostAddress}, Address:{userOrg?.Address.GetAddressString()}");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                    // TODO: maybe change this?
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(3014));
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
            ClearStash();

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
            if (checkResult != null) return checkResult;

            //Get the user organisation
            var userOrg = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo =>
                uo.UserId == VirtualUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            //Prepare view parameters
            ViewBag.UserFullName = VirtualUser.Fullname;
            ViewBag.UserJobTitle = VirtualUser.JobTitle;
            ViewBag.Organisation = userOrg.Organisation.OrganisationName;
            ViewBag.Address = userOrg?.Address.GetAddressString(",<br/>");

            var encOrgId = SharedBusinessLogic.Obfuscator.Obfuscate(userOrg.OrganisationId.ToString());
            //Show the PIN textbox and button
            return View("RequestPIN", encOrgId);
        }

        [PreventDuplicatePost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("request-pin")]
        public async Task<IActionResult> RequestPIN(CompleteViewModel model)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            //Get the user organisation
            var userOrg = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo =>
                uo.UserId == VirtualUser.UserId && uo.OrganisationId == ReportingOrganisationId);

            //Mark the user org as ready to send a pin
            userOrg.PIN = null;
            userOrg.PINHash = null;
            userOrg.PINSentDate = null;
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            var encOrgId = SharedBusinessLogic.Obfuscator.Obfuscate(userOrg.OrganisationId.ToString());
            return RedirectToAction("PINSent", encOrgId);
        }

        #endregion
    }
}