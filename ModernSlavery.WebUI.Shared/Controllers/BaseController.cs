using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Cookies;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Shared.Options;
using Newtonsoft.Json;
using Extensions = ModernSlavery.WebUI.Shared.Classes.Extensions.Extensions;

namespace ModernSlavery.WebUI.Shared.Controllers
{
    public class BaseController : Controller
    {
        public string UserHostAddress => HttpContext.GetUserHostAddress();

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            # region logic before action goes here

            //Pass the controller object into the ViewData 
            var controller = context.Controller as Controller;
            controller.ViewData["Controller"] = controller;

            if (SharedBusinessLogic.SharedOptions.DisablePageCaching)
                //Disable page caching
                context.HttpContext.DisableResponseCache();

            #endregion

            //Ensure the session is loaded
            await Session.LoadAsync();

            try
            {
                await base.OnActionExecutionAsync(context, next);
            }
            finally
            {
                //Ensure the session data is saved
                await Session.SaveAsync();
            }

            #region logic after the action goes here

            //Save the history and action/controller names
            SaveHistory();

            LastAction = ActionName;
            LastController = ControllerName;

            #endregion
        }

        /// <summary>
        ///     returns true if previous action
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        protected bool WasAction(string actionName, string controllerName = null, object routeValues = null)
        {
            if (string.IsNullOrWhiteSpace(controllerName)) controllerName = ControllerName;

            return !(UrlReferrer == null) &&
                   UrlReferrer.PathAndQuery.EqualsI(Url.Action(actionName, controllerName, routeValues));
        }

        protected bool WasAnyAction(params string[] actionUrls)
        {
            for (var i = 0; i < actionUrls.Length; i++)
            {
                var actionUrl = actionUrls[i].TrimI(@" /\");
                var actionName = actionUrl.AfterFirst("/");
                var controllerName = actionUrl.BeforeFirst("/", includeWhenNoSeparator: false);
                if (WasAction(actionName, controllerName)) return true;
            }

            return false;
        }

        protected bool IsAction(string actionName, string controllerName = null)
        {
            return actionName.EqualsI(ActionName) &&
                   (controllerName.EqualsI(ControllerName) || string.IsNullOrWhiteSpace(controllerName));
        }

        protected bool IsAnyAction(params string[] actionUrls)
        {
            for (var i = 0; i < actionUrls.Length; i++)
            {
                var actionUrl = actionUrls[i].TrimI(@" /\");
                var actionName = actionUrl.AfterFirst("/");
                var controllerName = actionUrl.BeforeFirst("/", includeWhenNoSeparator: false);
                if (IsAction(actionName, controllerName)) return true;
            }

            return false;
        }

        #region Contact Us

        [HttpGet("~/contact-us")]
        public IActionResult ContactUs()
        {
            return View("ContactUs");
        }

        #endregion

        [HttpGet("~/ping")]
        public IActionResult Ping()
        {
            return new OkResult(); // OK = 200
        }

        [HttpGet("~/Go/{shortCode?}", Order = 1)]
        public async Task<IActionResult> Go(string shortCode)
        {
            if (!string.IsNullOrWhiteSpace(shortCode))
            {
                var allShortCodes = await WebService.ShortCodesRepository.GetAllShortCodesAsync();
                var code = allShortCodes?.FirstOrDefault(sc => sc.ShortCode.EqualsI(shortCode));

                if (code != null && !string.IsNullOrWhiteSpace(code.Path))
                {
                    if (code.ExpiryDate.HasValue && code.ExpiryDate.Value < VirtualDateTime.Now)
                        return View(
                            "CustomError",
                            WebService.ErrorViewModelFactory.Create(1135,
                                new {expiryDate = code.ExpiryDate.Value.ToString("d MMMM yyyy")}));

                    await TrackPageViewAsync();

                    var url = $"{code.Path.TrimI(@" .~\")}{RequestUrl.Query}";
                    return new RedirectResult(url);
                }
            }

            return View("CustomError", WebService.ErrorViewModelFactory.Create(1136));
        }


        [HttpGet("~/session-expired")]
        public async Task<IActionResult> SessionExpired()
        {
            if (!User.Identity.IsAuthenticated) return View("SessionExpired");

            return await LogoutUser(Url.Action(nameof(SessionExpired), null, null, "https"));
        }

        [HttpGet("~/report-concerns")]
        public IActionResult ReportConcerns()
        {
            return View(nameof(ReportConcerns));
        }

        [NonAction]
        public async Task<IActionResult> LogoutUser(string redirectUrl = null)
        {
            //If impersonating then stop
            if (ImpersonatedUserId > 0)
            {
                ImpersonatedUserId = 0;
                OriginalUser = null;
                return Redirect(WebService.RouteHelper.Get(UrlRouteOptions.Routes.SubmissionHome));
            }

            //otherwise actually logout
            if (string.IsNullOrWhiteSpace(redirectUrl)) return SignOut("Cookies", "oidc");

            var properties = new AuthenticationProperties {RedirectUri = redirectUrl};
            return SignOut(properties, "Cookies", "oidc");
        }


        protected async Task IncrementRetryCountAsync(string retryLockKey, int expiryMinutes)
        {
            var count = await Cache.GetAsync<int>($"{UserHostAddress}:{retryLockKey}:Count");
            count++;
            if (count >= 3) await CreateRetryLockAsync(retryLockKey, expiryMinutes);

            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}:Count");
            await Cache.AddAsync($"{UserHostAddress}:{retryLockKey}:Count", count,
                VirtualDateTime.Now.AddMinutes(expiryMinutes));
        }

        protected async Task CreateRetryLockAsync(string retryLockKey, int expiryMinutes)
        {
            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}");
            await Cache.AddAsync($"{UserHostAddress}:{retryLockKey}", VirtualDateTime.Now,
                VirtualDateTime.Now.AddMinutes(expiryMinutes));
        }

        protected async Task<TimeSpan> GetRetryLockRemainingTimeAsync(string retryLockKey, int expiryMinutes)
        {
            if (SharedBusinessLogic.SharedOptions.SkipSpamProtection) return TimeSpan.Zero;

            var lockDate = await Cache.GetAsync<DateTime>($"{UserHostAddress}:{retryLockKey}");
            var remainingTime =
                lockDate == DateTime.MinValue
                    ? TimeSpan.Zero
                    : lockDate.AddMinutes(expiryMinutes) - VirtualDateTime.Now;
            return remainingTime;
        }

        protected async Task ClearRetryLocksAsync(string retryLockKey)
        {
            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}");
            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}:Count");
        }

        public async Task TrackPageViewAsync(string pageTitle = null, string pageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(pageTitle)) pageTitle = ViewBag.Title;

            if (string.IsNullOrWhiteSpace(pageTitle)) pageTitle = RouteData.Values["action"].ToString();

            if (string.IsNullOrWhiteSpace(pageUrl))
                pageUrl = HttpContext.GetUri().ToString();
            else if (!pageUrl.IsUrl())
                pageUrl = Core.Extensions.Url.RelativeToAbsoluteUrl(pageUrl, HttpContext.GetUri());


            await WebTracker.SendPageViewTrackingAsync(pageTitle, pageUrl);
        }

        #region Constructors

        public BaseController(
            ILogger logger,
            IWebService webService,
            ISharedBusinessLogic sharedBusinessLogic)
        {
            Logger = logger;
            WebService = webService;
            AutoMapper = webService.AutoMapper;
            Cache = webService.Cache;
            Session = webService.Session;
            WebTracker = webService.WebTracker;

            SharedBusinessLogic = sharedBusinessLogic;
        }

        public readonly IWebService WebService;
        protected readonly ILogger Logger;
        public readonly IHttpCache Cache;
        public readonly IHttpSession Session;
        public readonly ISharedBusinessLogic SharedBusinessLogic;
        public readonly IWebTracker WebTracker;
        protected readonly IMapper AutoMapper;

        #endregion

        #region Navigation

        public Uri RequestUrl => HttpContext.GetUri();
        public Uri UrlReferrer => HttpContext.GetUrlReferrer();


        public string EmployerBackUrl
        {
            get => Session["EmployerBackUrl"] as string;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    Session.Remove("EmployerBackUrl");
                else
                    Session["EmployerBackUrl"] = value;
            }
        }

        public string ReportBackUrl
        {
            get => Session["ReportBackUrl"] as string;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    Session.Remove("ReportBackUrl");
                else
                    Session["ReportBackUrl"] = value;
            }
        }

        public List<string> PageHistory
        {
            get
            {
                var pageHistory = Session["PageHistory"]?.ToString();
                return string.IsNullOrWhiteSpace(pageHistory)
                    ? new List<string>()
                    : JsonConvert.DeserializeObject<List<string>>(pageHistory);
            }
            set
            {
                if (value == null || !value.Any())
                    Session.Remove("PageHistory");
                else
                    Session["PageHistory"] = JsonConvert.SerializeObject(value);
            }
        }

        private void SaveHistory()
        {
            var history = PageHistory;
            try
            {
                var previousPage = UrlReferrer == null || !RequestUrl.Host.Equals(UrlReferrer.Host)
                    ? null
                    : UrlReferrer.PathAndQuery;
                var currentPage = RequestUrl.PathAndQuery;

                var currentIndex = history.IndexOf(currentPage);
                var previousIndex = string.IsNullOrWhiteSpace(previousPage) ? -2 : history.IndexOf(previousPage);

                if (previousIndex == -2)
                {
                    history.Clear();
                    history.Insert(0, currentPage);
                    return;
                }

                if (currentIndex == -1 && previousIndex == 0)
                {
                    history.Insert(0, currentPage);
                    return;
                }

                if (currentIndex == -1)
                {
                    history.Clear();
                    if (previousIndex == -1) history.Insert(0, previousPage);

                    history.Insert(0, currentPage);
                    return;
                }

                if (currentIndex == 0 && previousIndex == 1) return;

                if (currentIndex > previousIndex)
                    for (var i = currentIndex - 1; i >= 0; i--)
                        history.RemoveAt(i);
            }
            finally
            {
                PageHistory = history;
            }
        }

        public IActionResult RedirectToAction<TController>(string actionName) where TController : BaseController
        {
            var result = RedirectToAction(actionName, Extensions.GetControllerFriendlyName<TController>());

            var areaAttr = Extensions.GetControllerArea<TController>();
            if (areaAttr != null)
                result.RouteValues = new RouteValueDictionary {{areaAttr.RouteKey, areaAttr.RouteValue}};

            return result;
        }

        #endregion

        #region Cookies

        [HttpGet("~/cookies")]
        public IActionResult CookieSettingsGet()
        {
            var cookieSettings = CookieHelper.GetCookieSettingsCookie(Request);

            var cookieSettingsViewModel = new CookieSettingsViewModel
            {
                GoogleAnalyticsGpg = cookieSettings.GoogleAnalyticsGpg ? "On" : "Off",
                GoogleAnalyticsGovUk = cookieSettings.GoogleAnalyticsGovUk ? "On" : "Off",
                ApplicationInsights = cookieSettings.ApplicationInsights ? "On" : "Off",
                RememberSettings = cookieSettings.RememberSettings ? "On" : "Off"
            };

            return View("CookieSettings", cookieSettingsViewModel);
        }

        [HttpPost("~/cookies")]
        public IActionResult CookieSettingsPost(CookieSettingsViewModel cookieSettingsViewModel)
        {
            var cookieSettings = new CookieSettings
            {
                GoogleAnalyticsGpg =
                    cookieSettingsViewModel != null && cookieSettingsViewModel.GoogleAnalyticsGpg == "On",
                GoogleAnalyticsGovUk = cookieSettingsViewModel != null &&
                                       cookieSettingsViewModel.GoogleAnalyticsGovUk == "On",
                ApplicationInsights = cookieSettingsViewModel != null &&
                                      cookieSettingsViewModel.ApplicationInsights == "On",
                RememberSettings = cookieSettingsViewModel != null && cookieSettingsViewModel.RememberSettings == "On"
            };

            CookieHelper.SetCookieSettingsCookie(Response, cookieSettings);
            CookieHelper.SetSeenCookieMessageCookie(Response);

            cookieSettingsViewModel.ChangesHaveBeenSaved = true;

            WebService.CustomLogger.Information(
                "Updated cookie settings",
                new
                {
                    CookieSettings = cookieSettings,
                    HttpRequestMethod = HttpContext.Request.Method,
                    HttpRequestPath = HttpContext.Request.Path.Value
                });

            return View("CookieSettings", cookieSettingsViewModel);
        }

        [HttpPost("~/accept-all-cookies")]
        public async Task<IActionResult> AcceptAllCookies()
        {
            var cookieSettings = new CookieSettings
            {
                GoogleAnalyticsGpg = true,
                GoogleAnalyticsGovUk = true,
                ApplicationInsights = true,
                RememberSettings = true
            };

            CookieHelper.SetCookieSettingsCookie(Response, cookieSettings);
            CookieHelper.SetSeenCookieMessageCookie(Response);

            return Redirect(WebService.RouteHelper.Get(UrlRouteOptions.Routes.ViewingHome));
        }

        [HttpGet("~/cookie-details")]
        public IActionResult CookieDetails()
        {
            return View("CookieDetails");
        }

        #endregion

        #region PrivacyPolicy

        [HttpGet("~/privacy-policy")]
        public IActionResult PrivacyPolicy()
        {
            return View("PrivacyPolicy", null);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("~/privacy-policy")]
        public async Task<IActionResult> PrivacyPolicy(string command)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("PrivacyPolicy");

            //Ensure user has completed the registration process

            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult == null) return checkResult;

            if (!IsImpersonatingUser && !SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser))
            {
                // check if the user has accepted the privacy statement
                var hasReadPrivacy = VirtualUser.AcceptedPrivacyStatement;
                if (hasReadPrivacy == null ||
                    hasReadPrivacy.ToDateTime() < SharedBusinessLogic.SharedOptions.PrivacyChangedDate)
                {
                    VirtualUser.AcceptedPrivacyStatement = VirtualDateTime.Now;
                    await SharedBusinessLogic.DataRepository.SaveChangesAsync();
                }
            }

            return RedirectToAction(WebService.RouteHelper.Get(UrlRouteOptions.Routes.SubmissionHome));
        }

        #endregion

        #region Properties

        public long ReportingOrganisationId
        {
            get => Session["ReportingOrganisationId"].ToInt64();
            set
            {
                _ReportingOrganisation = null;
                ReportingOrganisationStartYear = null;
                Session["ReportingOrganisationId"] = value;
            }
        }

        public int? ReportingOrganisationStartYear
        {
            get => Session["ReportingOrganisationReportStartYear"].ToInt32();
            set => Session["ReportingOrganisationReportStartYear"] = value;
        }

        public string PendingFasttrackCodes
        {
            get => (string) Session["PendingFasttrackCodes"];
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    Session.Remove("PendingFasttrackCodes");
                else
                    Session["PendingFasttrackCodes"] = value;
            }
        }

        private Organisation _ReportingOrganisation;

        public Organisation ReportingOrganisation
        {
            get
            {
                if (_ReportingOrganisation == null && ReportingOrganisationId > 0)
                    _ReportingOrganisation = SharedBusinessLogic.DataRepository.GetAll<Organisation>()
                        .FirstOrDefault(o => o.OrganisationId == ReportingOrganisationId);

                return _ReportingOrganisation;
            }
            set
            {
                _ReportingOrganisation = value;
                ReportingOrganisationId = value == null ? 0 : value.OrganisationId;
            }
        }

        public virtual User CurrentUser => VirtualUser;

        public bool IsTrustedIP =>
            string.IsNullOrWhiteSpace(SharedBusinessLogic.SharedOptions.TrustedIpDomains) ||
            UserHostAddress.IsTrustedAddress(SharedBusinessLogic.SharedOptions.TrustedIpDomains.SplitI());

        public bool IsAdministrator => SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser);
        public bool IsSuperAdministrator => IsTrustedIP && SharedBusinessLogic.AuthorisationBusinessLogic.IsSuperAdministrator(CurrentUser);
        public bool IsDatabaseAdministrator => IsTrustedIP && SharedBusinessLogic.AuthorisationBusinessLogic.IsDatabaseAdministrator(CurrentUser);

        public bool IsTestUser => CurrentUser.EmailAddress.StartsWithI(SharedBusinessLogic.SharedOptions.TestPrefix);
        public bool IsImpersonatingUser => OriginalUser != null && SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(OriginalUser);

        protected User VirtualUser =>
            User.Identity.IsAuthenticated
                ? ImpersonatedUserId > 0 ? SharedBusinessLogic.DataRepository.Get<User>(ImpersonatedUserId) :
                SharedBusinessLogic.DataRepository.FindUser(User)
                : null;

        #endregion

        #region Authorisation Methods

        private User _OriginalUser;

        protected User OriginalUser
        {
            get
            {
                if (_OriginalUser == null)
                {
                    var userId = Session["OriginalUser"].ToInt64();
                    if (userId > 0) _OriginalUser = SharedBusinessLogic.DataRepository.Get<User>(userId);
                }

                return _OriginalUser;
            }

            set
            {
                if (value == null)
                    Session.Remove("OriginalUser");
                else
                    Session["OriginalUser"] = value.UserId;
            }
        }

        protected long ImpersonatedUserId
        {
            get => Session["ImpersonatedUserId"].ToInt64();
            set => Session["ImpersonatedUserId"] = value;
        }


        protected async Task<IActionResult> CheckUserRegisteredOkAsync()
        {
            //Ensure user is logged in submit or rest of registration
            if (!User.Identity.IsAuthenticated)
            {
                //Allow anonymous users when starting registration
                if (IsAnyAction("SignUp/AboutYou", "SignUp/VerifyEmail")) return null;

                //Allow anonymous users when resetting password
                if (IsAnyAction("Account/PasswordReset", "Account/NewPassword")) return null;

                //Otherwise ask the user to login
                return new ChallengeResult();
            }

            //Always allow the viewing controller
            if (ControllerName.EqualsI("Viewing")) return null;

            //Ensure we get a valid user from the database
            if (VirtualUser == null) throw new IdentityNotMappedException();

            // When user status is retired
            if (VirtualUser.Status == UserStatuses.Retired) return new ChallengeResult();

            //When email not verified
            if (VirtualUser.EmailVerifiedDate.EqualsI(null, DateTime.MinValue))
            {
                //If email not sent
                if (VirtualUser.EmailVerifySendDate.EqualsI(null, DateTime.MinValue))
                {
                    if (IsAnyAction("SignUp/VerifyEmail")) return null;

                    //Tell them to verify email
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1100));
                }

                //If verification code has expired
                if (VirtualUser.EmailVerifySendDate.Value.AddHours(SharedBusinessLogic.SharedOptions
                    .EmailVerificationExpiryHours) < VirtualDateTime.Now)
                {
                    if (IsAnyAction("SignUp/VerifyEmail")) return null;

                    //prompt user to click to request a new one
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1101));
                }

                //If code min time hasnt elapsed 
                var remainingTime =
                    VirtualUser.EmailVerifySendDate.Value.AddHours(SharedBusinessLogic.SharedOptions
                        .EmailVerificationMinResendHours)
                    - VirtualDateTime.Now;
                if (remainingTime > TimeSpan.Zero)
                {
                    //Process the code if there is one
                    if (IsAnyAction("SignUp/VerifyEmail") && !string.IsNullOrWhiteSpace(Request.Query["code"]))
                        return null;

                    //tell them to wait
                    return View("CustomError",
                        WebService.ErrorViewModelFactory.Create(1102,
                            new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
                }

                //if the code is still valid but min sent time has elapsed
                if (IsAnyAction("SignUp/VerifyEmail", "SignUp/EmailConfirmed")) return null;

                //Prompt user to request a new verification code
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1103));
            }

            //Ensure admins always routed to their home page
            if (SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(VirtualUser))
            {
                if (IsAnyAction(
                    "SignUp/VerifyEmail",
                    "Registration/EmailConfirmed",
                    "Registration/ReviewRequest",
                    "Registration/ConfirmCancellation",
                    "Registration/RequestAccepted",
                    "Registration/RequestCancelled",
                    "Registration/ReviewDUNSNumber"))
                    return null;

                return Redirect(WebService.RouteHelper.Get(UrlRouteOptions.Routes.AdminHome));
                //return View("CustomError", WebService.ErrorViewModelFactory.Create(1117));
            }

            //Ensure admin pages only available to administrators

            if (ControllerName.EqualsI("admin")
                || IsAnyAction(
                    "Registration/ReviewRequest",
                    "Registration/ReviewDUNSNumber",
                    "Registration /ConfirmCancellation",
                    "Registration/RequestAccepted",
                    "Registration/RequestCancelled"))
                return new HttpForbiddenResult($"User {CurrentUser?.EmailAddress} is not an administrator");

            //Allow all steps from email confirmed to organisation chosen
            if (IsAnyAction(
                "SignUp/EmailConfirmed",
                "Registration/OrganisationType",
                "Registration/OrganisationSearch",
                "Registration/ChooseOrganisation",
                "Registration/AddOrganisation",
                "Registration/SelectOrganisation",
                "Registration/AddAddress",
                "Registration/AddSector",
                "Registration/AddContact",
                "Registration/ConfirmOrganisation",
                "Registration/RequestReceived",
                "Registration/EnterFasttrackCodes"))
                return null;

            //Always allow users to manage their account
            if (IsAnyAction(
                "Account/ManageAccount",
                "ChangeEmail/ChangeEmail",
                "ChangeEmail/ChangeEmailPending",
                "ChangeEmail/VerifyChangeEmail",
                "ChangeEmail/ChangeEmailFailed",
                "ChangeEmail/CompleteChangeEmailAsync",
                "ChangeDetails/ChangeDetails",
                "ChangePassword/ChangePassword",
                "CloseAccount/CloseAccount"))
                return null;

            //Always allow user home or remove registration page 
            if (IsAnyAction(
                "Submission/ManageOrganisations",
                "Registration/RemoveOrganisation",
                "Registration/RemoveOrganisationPost",
                "Submission/ReportForOrganisation",
                "Submission/ManageOrganisation",
                "Scope/ChangeOrganisationScope",
                "Registration/ActivateService",
                "Scope/DeclareScope"))
                return null;

            // if the user doesn't have a selected an organisation then go to the ManageOrgs page
            var userOrg =
                VirtualUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == ReportingOrganisationId);
            if (userOrg == null)
            {
                Logger.LogWarning(
                    $"Cannot find UserOrganisation for user {VirtualUser.UserId} and organisation {ReportingOrganisationId}");

                return Redirect(WebService.RouteHelper.Get(UrlRouteOptions.Routes.SubmissionHome));
            }

            if (userOrg.Organisation.SectorType == SectorTypes.Private)
                if (userOrg.PINConfirmedDate.EqualsI(null, DateTime.MinValue))
                {
                    //If pin never sent then go to resend point
                    if (userOrg.PINSentDate.EqualsI(null, DateTime.MinValue))
                    {
                        if (IsAnyAction("Registration/PINSent", "Registration/RequestPIN")) return null;

                        return RedirectToAction(
                            WebService.RouteHelper.Get(UrlRouteOptions.Routes.RegistrationPINSent));
                    }

                    //If PIN sent and expired then prompt to request a new pin
                    if (userOrg.PINSentDate.Value.AddDays(SharedBusinessLogic.SharedOptions.PinInPostExpiryDays) <
                        VirtualDateTime.Now)
                    {
                        if (IsAnyAction("Registration/PINSent", "Registration/RequestPIN")) return null;

                        return View("CustomError", WebService.ErrorViewModelFactory.Create(1106));
                    }

                    //If PIN resends are allowed and currently on PIN send page then allow it to continue
                    var remainingTime =
                        userOrg.PINSentDate.Value.AddDays(SharedBusinessLogic.SharedOptions.PinInPostMinRepostDays) -
                        VirtualDateTime.Now;
                    if (remainingTime <= TimeSpan.Zero &&
                        IsAnyAction("Registration/PINSent", "Registration/RequestPIN")) return null;

                    //If PIN Not expired redirect to ActivateService where they can either enter the same pin or request a new one 
                    if (IsAnyAction("Registration/RequestPIN"))
                        return View("CustomError",
                            WebService.ErrorViewModelFactory.Create(1120,
                                new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));

                    if (IsAnyAction("Registration/ActivateService")) return null;

                    return RedirectToAction(
                        WebService.RouteHelper.Get(UrlRouteOptions.Routes.RegistrationActivate));
                }

            //Ensure user has completed the registration process
            //If user is fully registered then start submit process
            if (ControllerName.EqualsI("Registration"))
            {
                if (IsAnyAction("Registration/RequestReceived")) return null;

                if (IsAnyAction("Registration/ServiceActivated") && WasAnyAction("Registration/ActivateService",
                    "Registration/ConfirmOrganisation")) return null;

                return View("CustomError", WebService.ErrorViewModelFactory.Create(1109));
            }

            //Ensure pending manual registrations always redirected back to home
            if (userOrg.PINConfirmedDate == null)
            {
                Logger.LogWarning(
                    $"UserOrganisation for user {userOrg.UserId} and organisation {userOrg.OrganisationId} PIN is not confirmed");
                return Redirect(WebService.RouteHelper.Get(UrlRouteOptions.Routes.SubmissionHome));
            }

            return null;
        }

        #endregion


        #region Public fields

        public string ActionName => ControllerContext.RouteData.Values["action"].ToString();

        public string ControllerName => ControllerContext.RouteData.Values["controller"].ToString();

        public string LastAction
        {
            get => Session["LastAction"] as string;
            set => Session["LastAction"] = value;
        }

        public string LastController
        {
            get => Session["LastController"] as string;
            set => Session["LastController"] = value;
        }

        #endregion

        #region Exception handling methods

        [NonAction]
        public void AddModelError(int errorCode, string propertyName = null, object parameters = null)
        {
            ModelState.AddModelError(errorCode, propertyName, parameters);
        }

        protected ActionResult SessionExpiredView()
        {
            // create the session expired error model
            var errorModel = WebService.ErrorViewModelFactory.Create(1134);

            // return the custom error view
            return View("CustomError", errorModel);
        }

        #endregion

        #region Session Handling

        public void StashModel<T>(T model)
        {
            Session[this + ":Model"] = Core.Extensions.Json.SerializeObjectDisposed(model);
        }

        public void StashModel<KeyT, ModelT>(KeyT keyController, ModelT model)
        {
            Session[keyController + ":Model"] = Core.Extensions.Json.SerializeObjectDisposed(model);
        }

        public void ClearStash()
        {
            Session.Remove(this + ":Model");
        }

        public void ClearAllStashes()
        {
            foreach (var key in Session.Keys.ToList())
                if (key.EndsWithI(":Model"))
                    Session.Remove(key);
        }

        public T UnstashModel<T>(bool delete = false) where T : class
        {
            var json = Session[this + ":Model"].ToStringOrNull();
            var result = string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<T>(json);
            if (delete) Session.Remove(this + ":Model");

            return result;
        }

        public T UnstashModel<T>(Type keyController, bool delete = false) where T : class
        {
            var json = Session[this + ":Model"].ToStringOrNull();
            var result = string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<T>(json);
            if (delete) Session.Remove(keyController + ":Model");

            return result;
        }

        #endregion
    }
}