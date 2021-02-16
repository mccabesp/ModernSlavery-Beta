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
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Classes.UrlHelper;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;

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
            ILogger<RegistrationController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
            _registrationService = registrationService;
            _registrationPresenter = registrationPresenter;
            _postcodeChecker = postcodeChecker;
        }

        #endregion

        #region Session & Cache Properties

        private int LastPrivateSearchRemoteTotal => Session["LastPrivateSearchRemoteTotal"].ToInt32();

        private int CompaniesHouseFailures {
            get => Session["CompaniesHouseFailures"].ToInt32();
            set {
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

            return RedirectToActionAreaPermanent("AboutYou", "Account", "Account");
        }

        #endregion

        #region PINSent

        private async Task<IActionResult> GetSendPINAsync(long organisationId)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var result = await ValidatePinAccess(organisationId);

            if (!result.success)
                return result.actionResult;

            var userOrg = result.userOrg;

            //If a pin has never been sent or resend button submitted then send one immediately
            if (!userOrg.HasPINCode || !userOrg.IsPINCodeSent || userOrg.IsPINCodeExpired(SharedBusinessLogic.SharedOptions.PinInPostExpiryDays))
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
                    if (SharedBusinessLogic.TestOptions.ShowPinInPost)
                    {
                        ViewBag.PinCode = pin;
                        ViewBag.OrganisationIdentifier = SharedBusinessLogic.Obfuscator.Obfuscate(userOrg.OrganisationId);
                    }
                    else
                    {
                        // Try and send the PIN in post
                        var returnUrl = Url.ActionArea("ManageOrganisations", "Submission", "Submission", null, "https");
                        try
                        {
                            var response = await _registrationService.PinInThePostService.SendPinInThePostAsync(userOrg, pin, returnUrl);
                            userOrg.PITPNotifyLetterId = response.LetterId;
                        }
                        catch
                        {
                            // Show "Notify is down" error message
                            return View("PinFailedToSend", new PinFailedToSendViewModel { OrganisationName = userOrg.Organisation.OrganisationName });
                        }

                        await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                        Logger.LogInformation($"Send Pin-in-post. Name {VirtualUser.Fullname}, Email:{VirtualUser.EmailAddress}, IP:{UserHostAddress}, Address:{userOrg?.Address.GetAddressString()}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                    // TODO: maybe change this?
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(3014, new { OrganisationIdentifier = SharedBusinessLogic.Obfuscator.Obfuscate(organisationId) }));
                }

            //Prepare view parameters
            ViewBag.UserFullName = VirtualUser.Fullname;
            ViewBag.UserJobTitle = VirtualUser.JobTitle;
            ViewBag.Organisation = userOrg.Organisation.OrganisationName;
            ViewBag.Address = userOrg?.Address.GetAddressString(Environment.NewLine);
            return View("PINSent");
        }

        [Authorize]
        [HttpGet("pin-sent/{id}")]
        public async Task<IActionResult> PINSent([Obfuscated] long id)
        {
            //Clear the stash
            ClearStash();

            return await GetSendPINAsync(id);
        }

        #endregion

        #region RequestPIN

        [HttpGet("request-pin/{id}")]
        [Authorize]
        public async Task<IActionResult> RequestPIN([Obfuscated] long id)
        {
            //Ensure user has completed the registration process
            ReportingOrganisationId = id;
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var result = await ValidatePinAccess(id);

            if (!result.success)
                return result.actionResult;

            var vm = new RequestPinViewModel {
                OrganisationId = id,
                UserFullName = VirtualUser.Fullname,
                UserJobTitle = VirtualUser.JobTitle,
                OrganisationName = result.userOrg.Organisation.OrganisationName,
                Address = result.userOrg.Address.GetAddressString(Environment.NewLine),
            };

            //Show the PIN textbox and button
            return View("RequestPIN", vm);
        }

        [PreventDuplicatePost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("request-pin/{id}")]
        public async Task<IActionResult> RequestPINPost([Obfuscated] long id)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            var result = await ValidatePinAccess(id);

            if (!result.success)
                return result.actionResult;

            var userOrg = result.userOrg;

            // check if requesting is allowed
            var remainingTime = userOrg.GetTimeToNextPINAttempt(SharedBusinessLogic.SharedOptions.PinInPostMinRepostDays);
            if (remainingTime > TimeSpan.Zero)
            {
                AddModelError(1156, parameters: new { remainingTime = remainingTime.ToFriendly(maxParts: 2) });

                var vm = new RequestPinViewModel {
                    OrganisationId = id,
                    UserFullName = VirtualUser.Fullname,
                    UserJobTitle = VirtualUser.JobTitle,
                    OrganisationName = result.userOrg.Organisation.OrganisationName,
                    Address = result.userOrg.Address.GetAddressString(Environment.NewLine),
                };

                return View("RequestPIN", vm);
            }

            //Mark the user org as ready to send a pin
            userOrg.PIN = null;
            userOrg.PINHash = null;
            userOrg.PINSentDate = null;
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            return RedirectToAction(nameof(PINSent), new
            {
                id=SharedBusinessLogic.Obfuscator.Obfuscate(id)
            });
        }

        #endregion

        private async Task<(bool success, UserOrganisation userOrg, IActionResult actionResult)> ValidatePinAccess(long organisationId)
        {
            //Get the user organisation
            var userOrg = await SharedBusinessLogic.DataRepository.FirstOrDefaultAsync<UserOrganisation>(uo =>uo.UserId == VirtualUser.UserId && uo.OrganisationId == organisationId);

            // user has no association to the org
            if (userOrg == null)
                return (false, userOrg, new HttpForbiddenResult($"User {VirtualUser?.EmailAddress} is not registered for organisation id {organisationId}"));

            // only private orgs follow PIN workflow
            if (userOrg.Organisation.SectorType != SectorTypes.Private)
                return (false, userOrg, new HttpForbiddenResult($"Non-private organisation {userOrg.Organisation.OrganisationName} unable to request PIN for user {VirtualUser.UserId}"));

            // pin is confirmed then no need to request new pin, send to manage org
            if (userOrg.PINConfirmedDate.HasValue)
                return (false, userOrg, RedirectToActionArea("ManageOrganisations", "Submission", "Submission"));

            // pin pages are generally accessible
            return (true, userOrg, null);
        }
    }
}