using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Classes.UrlHelper;
using ModernSlavery.WebUI.Shared.Interfaces;
using Newtonsoft.Json;
using Extensions = ModernSlavery.WebUI.Shared.Classes.Extensions.Extensions;

namespace ModernSlavery.WebUI.Shared.Controllers
{
    [NoCache]
    [ControllerExceptionFilter]
    public class BaseController : Controller
    {
        #region Dependencies
        public readonly IWebService WebService;
        protected readonly ILogger Logger;
        public readonly IHttpCache Cache;
        public readonly IHttpSession Session;
        public readonly ISharedBusinessLogic SharedBusinessLogic;
        public readonly IWebTracker WebTracker;
        protected readonly IMapper AutoMapper;
        #endregion

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

    
        #endregion

        public string UserHostAddress => HttpContext.GetUserHostAddress();

        public bool BrowserIsIE11 => Regex.IsMatch(Request.Headers["User-Agent"].ToString(), @"Trident/7.*rv:11");

        [NonAction]
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            # region logic before action goes here

            //Pass the controller object into the ViewData 
            HttpContext.Items["Controller"] = this;

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
            // only save on get request
            if (Request.Method.EqualsI("GET"))
                SaveHistory();

            LastAction = ActionName;
            LastController = ControllerName;

            #endregion
        }

        [NonAction]
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }

        /// <summary>
        ///     returns true if previous action
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="controllerName"></param>
        [NonAction]
        protected bool WasAction(string actionName, string controllerName = null, object routeValues = null)
        {
            if (string.IsNullOrWhiteSpace(controllerName)) controllerName = ControllerName;

            return !string.IsNullOrWhiteSpace(LastControllerAction) && LastControllerAction.EqualsI($"{(!string.IsNullOrWhiteSpace(controllerName) ? $"{controllerName}/" : "")}{actionName}");
        }

        [NonAction]
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

        [NonAction]
        protected bool IsAction(string actionName, string controllerName = null)
        {
            return actionName.EqualsI(ActionName) &&
                   (controllerName.EqualsI(ControllerName) || string.IsNullOrWhiteSpace(controllerName));
        }

        [NonAction]
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

        [AllowAnonymous]
        [HttpGet("/[controller]/Init")]
        public IActionResult Init()
        {
            //Dont save in history
            SkipSaveHistory = true;

            if (!SharedBusinessLogic.SharedOptions.IsProduction())
                Logger.LogInformation($"Controller {AreaName}/{ControllerName} Initialised");
            
            return new OkResult();
        }

        [NonAction]
        public async Task<IActionResult> LogoutUser(string redirectUrl = null)
        {
            //If impersonating then stop
            if (ImpersonatedUserId > 0)
            {
                ImpersonatedUserId = 0;
                OriginalUser = null;
                return RedirectToActionArea("ManageOrganisations", "Submission", "Submission");
            }

            //otherwise actually logout
            if (string.IsNullOrWhiteSpace(redirectUrl)) redirectUrl = Url.ActionArea("SignOut", "Account", "Account", null, "https");

            var properties = new AuthenticationProperties {RedirectUri = redirectUrl};
            return SignOut(properties, "Cookies", "oidc");
        }

        [NonAction]
        protected async Task IncrementRetryCountAsync(string retryLockKey, int expiryMinutes)
        {
            var count = await Cache.GetAsync<int>($"{UserHostAddress}:{retryLockKey}:Count");
            count++;
            if (count >= 3) await CreateRetryLockAsync(retryLockKey, expiryMinutes);

            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}:Count");
            await Cache.AddAsync($"{UserHostAddress}:{retryLockKey}:Count", count,
                VirtualDateTime.Now.AddMinutes(expiryMinutes));
        }

        [NonAction]
        protected async Task CreateRetryLockAsync(string retryLockKey, int expiryMinutes)
        {
            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}");
            await Cache.AddAsync($"{UserHostAddress}:{retryLockKey}", VirtualDateTime.Now,
                VirtualDateTime.Now.AddMinutes(expiryMinutes));
        }

        [NonAction]
        protected async Task<TimeSpan> GetRetryLockRemainingTimeAsync(string retryLockKey, int expiryMinutes)
        {
            var lockDate = await Cache.GetAsync<DateTime>($"{UserHostAddress}:{retryLockKey}");
            var remainingTime =
                lockDate == DateTime.MinValue
                    ? TimeSpan.Zero
                    : lockDate.AddMinutes(expiryMinutes) - VirtualDateTime.Now;
            return remainingTime;
        }

        [NonAction]
        protected async Task ClearRetryLocksAsync(string retryLockKey)
        {
            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}");
            await Cache.RemoveAsync($"{UserHostAddress}:{retryLockKey}:Count");
        }

        [NonAction]
        protected async Task TrackPageViewAsync(string pageTitle = null, string pageUrl = null)
        {
            if (string.IsNullOrWhiteSpace(pageTitle)) pageTitle = RouteData.Values["action"].ToString();

            if (string.IsNullOrWhiteSpace(pageUrl))
                pageUrl = HttpContext.GetUri().ToString();
            else if (!pageUrl.IsUrl())
                pageUrl = Core.Extensions.Url.RelativeToAbsoluteUrl(pageUrl, HttpContext.GetUri());


            await WebTracker.SendPageViewTrackingAsync(pageTitle, pageUrl);
        }

        #region Navigation

        public Uri RequestUrl => HttpContext.GetUri();
        public Uri UrlReferrer => HttpContext.GetUrlReferrer();


        public string OrganisationBackUrl
        {
            get => Session["OrganisationBackUrl"] as string;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    Session.Remove("OrganisationBackUrl");
                else
                    Session["OrganisationBackUrl"] = value;
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
                    Session["PageHistory"] = Core.Extensions.Json.SerializeObject(value);
            }
        }

        public string LandingUrl => User.Identity.IsAuthenticated ? Url.ActionArea("ManageOrganisations", "Submission", "Submission") : Url.ActionArea("Landing", "Viewing", "Viewing");

        public string BackUrl 
        {
            get 
            {
                //Get the page called before the current page
                var history = PageHistory;
                var currentIndex = history.IndexOf(RequestUrl.PathAndQuery);
                if (currentIndex == -1 && history.Count > 0) return history[0];
                if (currentIndex > -1 && (currentIndex+1) < history.Count) return history[currentIndex + 1];

                //If never been to current page then use the landing page as the previous page
                return LandingUrl;
            }
        }
        private void SaveHistory()
        {
            if (!Request.Method.EqualsI("GET") || Response.StatusCode!=(int)System.Net.HttpStatusCode.OK || SkipSaveHistory) return;

            var history = PageHistory;
            try
            {
                //Whenever on a landing url then clear the history
                if (RequestUrl.PathAndQuery.EqualsI(LandingUrl))history.Clear();

                //Get previous and current page
                var previousPage = UrlReferrer == null || !RequestUrl.Host.Equals(UrlReferrer.Host) ? null : UrlReferrer.PathAndQuery;
                var currentPage = RequestUrl.PathAndQuery;

                // get previous and current page position in history
                var previousIndex = string.IsNullOrWhiteSpace(previousPage) ? -2 : history.IndexOf(previousPage);
                var currentIndex = history.IndexOf(currentPage);

                //If returning from referrer page all pages in queue back to current page
                if (previousIndex == 0 && currentIndex > 0)
                    history.RemoveRange(0, currentIndex);
                else if (currentIndex != 0)
                    //If not on page then add to queue
                    history.Insert(0, currentPage);
            }
            finally
            {
                PageHistory = history;
            }
        }

        protected bool SkipSaveHistory = false;
        
        [NonAction]
        public void SetBackUrl(string backUrl)
        {
            var history = PageHistory;

            try
            {
                var currentIndex = history.IndexOf(backUrl);
                if (currentIndex > 0)
                    history.RemoveRange(0, currentIndex);
                else if (currentIndex != 0)
                    //If not on page then add to queue
                    history.Insert(0, backUrl);

                SkipSaveHistory = true;
            }
            finally
            {
                PageHistory = history;
            }

        }

        [NonAction]
        public IActionResult RedirectToAction<TController>(string actionName) where TController : BaseController
        {
            var result = RedirectToAction(actionName, Extensions.GetControllerFriendlyName<TController>());

            var areaAttr = Extensions.GetControllerArea<TController>();
            if (areaAttr != null)
                result.RouteValues = new RouteValueDictionary {{areaAttr.RouteKey, areaAttr.RouteValue}};

            return result;
        }

        [NonAction]
        public IActionResult RedirectToActionArea(string actionName, string controllerName, string areaName, object routeValues=null, string fragment=null)
        {
            var url = Url.ActionArea(actionName, controllerName, areaName, routeValues, fragment: fragment);
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException($"Cannot resolve route to url: Action: {actionName}, Controller: {controllerName}, Area: {areaName}, RouteValues: {(routeValues==null ? null : JsonConvert.SerializeObject(routeValues))}");

            return Redirect(url);
        }

        [NonAction]
        public IActionResult RedirectToActionAreaPermanent(string actionName, string controllerName, string areaName, object routeValues = null, string fragment = null)
        {
            var url = Url.ActionArea(actionName, controllerName, areaName, routeValues, fragment: fragment);
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException($"Cannot resolve route to url: Action: {actionName}, Controller: {controllerName}, Area: {areaName}, RouteValues: {(routeValues == null ? null : JsonConvert.SerializeObject(routeValues))}");

            return RedirectPermanent(url);
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

        public bool IsTrustedIP => SharedBusinessLogic.AuthorisationBusinessLogic.IsTrustedAddress(UserHostAddress);

        public bool IsSubmitter => SharedBusinessLogic.AuthorisationBusinessLogic.IsSubmitter(VirtualUser);
        
        [NonAction]
        public bool IsSubmitterEmail(string emailAddress) => SharedBusinessLogic.AuthorisationBusinessLogic.IsSubmitter(emailAddress);

        public bool IsAdministrator => SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(VirtualUser);
        
        [NonAction] 
        public bool IsAdministratorEmail(string emailAddress) => SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(emailAddress);
        public bool IsSuperAdministrator => IsTrustedIP && SharedBusinessLogic.AuthorisationBusinessLogic.IsSuperAdministrator(VirtualUser);
        public bool IsDatabaseAdministrator => IsTrustedIP && SharedBusinessLogic.AuthorisationBusinessLogic.IsDatabaseAdministrator(VirtualUser);
        public bool IsDevOpsAdministrator => IsTrustedIP && SharedBusinessLogic.AuthorisationBusinessLogic.IsDevOpsAdministrator(VirtualUser);

        public bool IsImpersonatingUser => OriginalUser != null && SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(VirtualUser);

        public User VirtualUser =>
            User.Identity.IsAuthenticated
                ? ImpersonatedUserId > 0 ? SharedBusinessLogic.DataRepository.Get<User>(ImpersonatedUserId) :
                SharedBusinessLogic.DataRepository.FindUser(User)
                : null;

        #endregion

        #region Authorisation Properties

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

        [NonAction]
        protected async Task<IActionResult> CheckUserRegisteredOkAsync()
        {
            //Ensure user is logged in submit or rest of registration
            if (!User.Identity.IsAuthenticated)
            {
                //Allow anonymous users when starting registration
                if (IsAnyAction("NewAccount/AboutYou", "NewAccount/VerifyEmail", "NewAccount/VerifyEmailPost")) return null;

                //Allow anonymous users when resetting password
                if (IsAnyAction("Account/PasswordReset", "Account/NewPassword")) return null;

                //Otherwise ask the user to login
                return new ChallengeResult();
            }

            //Always allow the viewing controller
            if (ControllerName.EqualsI("Viewing", "Shared")) return null;

            //Ensure we get a valid user from the database
            if (VirtualUser == null) throw new IdentityNotMappedException();

            // When user status is retired
            if (VirtualUser.Status == UserStatuses.Retired) return new ChallengeResult();

            //When email not verified
            if (!VirtualUser.IsVerifiedEmail)
            {
                //If email not sent
                if (!VirtualUser.IsVerifyEmailSent)
                {
                    if (IsAnyAction("NewAccount/VerifyEmail", "NewAccount/VerifyEmailPost")) return null;

                    //Tell them to verify email
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1100));
                }

                //If verification code has expired
                if (VirtualUser.IsVerificationCodeExpired(SharedBusinessLogic.SharedOptions.EmailVerificationExpiryHours))
                {
                    if (IsAnyAction("NewAccount/VerifyEmail", "NewAccount/VerifyEmailPost")) return null;

                    //prompt user to click to request a new one
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1101));
                }

                //If code min time hasnt elapsed 
                var remainingTime = VirtualUser.GetTimeToNextVerificationResend(SharedBusinessLogic.SharedOptions.EmailVerificationMinResendHours);
                if (remainingTime > TimeSpan.Zero)
                {
                    //Process the code if there is one
                    if (IsAnyAction("NewAccount/VerifyEmail", "NewAccount/VerifyEmailPost") && RouteData.Values.ContainsKey("code") && !string.IsNullOrWhiteSpace(RouteData.Values["code"].ToString()))
                        return null;

                    //tell them to wait
                    return View("CustomError",
                        WebService.ErrorViewModelFactory.Create(1102,
                            new {remainingTime = remainingTime.ToFriendly(maxParts: 2)}));
                }

                //if the code is still valid but min sent time has elapsed
                if (IsAnyAction("NewAccount/VerifyEmail", "NewAccount/VerifyEmailPost", "NewAccount/EmailConfirmed")) return null;

                //Prompt user to request a new verification code
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1103));
            }

            //Always allow users to manage their account
            if (IsAnyAction(
                    "NewAccount/VerifyEmail", 
                    "NewAccount/VerifyEmailPost",
                    "NewAccount/EmailConfirmed",
                    "Account/ManageAccount",
                    "ChangeEmail/ChangeEmail",
                    "ChangeEmail/ChangeEmailPending",
                    "ChangeEmail/VerifyChangeEmail",
                    "ChangeEmail/CompleteChangeEmail",
                    "ChangePassword/ChangePassword",
                    "ChangeDetails/ChangeDetails",
                    "CloseAccount/CloseAccount",
                    "CloseAccount/CloseAccountCompleted"))
                return null;

            //Ensure admins always routed to their home page
            if (SharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(VirtualUser))
            {
                if (AreaName.EqualsI("Admin", "DevOps"))
                    return null;

                return RedirectToActionArea("Home", "Admin", "Admin");
                //return View("CustomError", WebService.ErrorViewModelFactory.Create(1117));
            }

            //Ensure admin/devops pages only available to administrators
            if (AreaName.EqualsI("Admin", "DevOps"))
                return RedirectToActionArea("ManageOrganisations", "Submission", "Submission");

            //Allow all steps from email confirmed to organisation chosen
            if (IsAnyAction(
                "Registration/OrganisationType",
                "Registration/OrganisationSearch",
                "Registration/ChooseOrganisation",
                "Registration/AddOrganisation",
                "Registration/SelectOrganisation",
                "Registration/AddAddress",
                "Registration/AddContact",
                "Registration/ConfirmOrganisation",
                "Registration/RequestReceived",
                "Registration/FastTrack"))
                return null;

            //Always allow user home or remove registration page 
            if (IsAnyAction(
                "Submission/ManageOrganisations",
                "Registration/RemoveOrganisation",
                "Registration/RemoveOrganisationPost",
                "Submission/ReportForOrganisation",
                "Submission/ManageOrganisation",
                "Scope/ChangeOrganisationScope",
                "Scope/DeclareScope"
                ))
                return null;

            // allow statement
            if (ControllerName == "Statement")return null;

            // if the user doesn't have a selected an organisation then go to the ManageOrgs page
            var userOrg = VirtualUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == ReportingOrganisationId);
            if (userOrg == null)
            {
                Logger.LogWarning($"Cannot find UserOrganisation for user {VirtualUser.UserId} and organisation {ReportingOrganisationId}");
                return RedirectToActionArea("ManageOrganisations", "Submission", "Submission");
            }

            if (userOrg.Organisation.SectorType == SectorTypes.Private)
                if (!userOrg.IsRegisteredOK)
                {
                    //If pin never sent then go to resend point
                    if (!userOrg.IsPINCodeSent)
                    {
                        if (IsAnyAction("Registration/PINSent", "Registration/RequestPIN", "Registration/RequestPINPost")) return null;

                        return RedirectToActionArea("PINSent","Registration", "Registration", new { id = SharedBusinessLogic.Obfuscator.Obfuscate(userOrg.OrganisationId) });
                    }

                    //If PIN sent and expired then prompt to request a new pin
                    if (userOrg.IsPINCodeExpired(SharedBusinessLogic.SharedOptions.PinInPostExpiryDays))
                    {
                        if (IsAnyAction("Registration/PINSent", "Registration/RequestPIN", "Registration/RequestPINPost")) return null;

                        return View("CustomError", WebService.ErrorViewModelFactory.Create(1106, new { OrganisationIdentifier = SharedBusinessLogic.Obfuscator.Obfuscate(userOrg.OrganisationId) }));
                    }

                    //If PIN resends are allowed and currently on PIN send page then allow it to continue
                    var remainingTime = userOrg.GetTimeToNextPINResend(SharedBusinessLogic.SharedOptions.PinInPostMinRepostDays);
                    if (remainingTime <= TimeSpan.Zero &&
                        IsAnyAction("Registration/PINSent", "Registration/RequestPIN", "Registration/RequestPINPost")) return null;

                    //If PIN Not expired redirect to ActivateService where they can either enter the same pin or request a new one 
                    if (IsAnyAction("Registration/RequestPIN", "Registration/RequestPINPost"))
                        return View("CustomError",WebService.ErrorViewModelFactory.Create(1120,new {remainingTime = remainingTime.ToFriendly(maxParts: 2), OrganisationIdentifier = SharedBusinessLogic.Obfuscator.Obfuscate(userOrg.OrganisationId) }));

                    if (IsAnyAction("Registration/ActivateService")) return null;

                    return RedirectToActionArea("ActivateService", "Registration", "Registration", new { id = SharedBusinessLogic.Obfuscator.Obfuscate(userOrg.OrganisationId) });
                }

            //Ensure pending manual registrations always redirected back to home
            if (!userOrg.IsRegisteredOK)
            {
                Logger.LogWarning($"UserOrganisation for user {userOrg.UserId} and organisation {userOrg.OrganisationId} PIN is not confirmed");
                return RedirectToActionArea("ManageOrganisations", "Submission", "Submission");
            }
            else if (ControllerName.EqualsI("Registration"))
            {
                if (IsAnyAction("Registration/RequestReceived")) return null;

                if (IsAnyAction("Registration/ServiceActivated") && WasAnyAction("Registration/ActivateService", "Registration/ConfirmOrganisation")) return null;

                //Report user is already registered
                return View("CustomError", WebService.ErrorViewModelFactory.Create(1109));
            }

            return null;
        }

        #endregion

        #region Public fields

        public string ActionName => ControllerContext.RouteData.Values["action"].ToString();

        public string ControllerName => ControllerContext.RouteData.Values["controller"].ToString();

        public string AreaName => ControllerContext.RouteData.Values.ContainsKey("area") ? ControllerContext.RouteData.Values["area"].ToString() : null;

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

        public string LastControllerAction => $"{(!string.IsNullOrWhiteSpace(LastController) ? $"{LastController}/" : "")}{LastAction}";
        #endregion

        #region Exception handling methods

        [NonAction]
        protected void AddModelError(int errorCode, string propertyName = null, object parameters = null)
        {
            ModelState.AddModelError(errorCode, propertyName, parameters);
        }

        [NonAction]
        protected void AddDisplayMessage(string displayMessage, bool addToLog=false)
        {
            if (string.IsNullOrWhiteSpace(displayMessage)) throw new ArgumentNullException(nameof(displayMessage));
            var messages = new HashSet<string>(Session.Get<List<string>>("DisplayMessages") ?? new List<string>(),StringComparer.OrdinalIgnoreCase);
            messages.Add(displayMessage);
            if (addToLog) Logger.LogInformation($"Message: {displayMessage}{Environment.NewLine}User: {CurrentUser.EmailAddress}");
            Session["DisplayMessages"] = messages.ToList();
        }

        [NonAction]
        protected void SetSuccessMessage(string successMessage)
        {
            if (string.IsNullOrWhiteSpace(successMessage)) throw new ArgumentNullException(nameof(successMessage));
            Session["SuccessMessage"] = successMessage;
        }

        [NonAction]
        protected ActionResult SessionExpiredView()
        {
            // create the session expired error model
            var errorModel = WebService.ErrorViewModelFactory.Create(1134);

            // return the custom error view
            return View("CustomError", errorModel);
        }

        #endregion

        #region Session Handling

        [NonAction]
        protected void StashModel<TModel>(TModel model)
        {
            var controllerType = this.GetType();
            var modelType = typeof(TModel);

            var keyName = $"{controllerType}:{modelType}:Model";
            Session[keyName] = Core.Extensions.Json.SerializeObject(model);
        }

        [NonAction]
        protected void StashModel<TModel>(Controller controller, TModel model)
        {
            var controllerType = controller.GetType();

            var modelType = typeof(TModel);

            var keyName = $"{controllerType}:{modelType}:Model";
            Session[keyName] = Core.Extensions.Json.SerializeObject(model);
        }

        [NonAction]
        protected void ClearStash()
        {
            var controllerType = this.GetType();
            
            foreach (var key in Session.Keys.ToList())
                if (key.IsMatch($"{controllerType}:[0-9A-Za-z/./_]*:Model$"))
                    Session.Remove(key);
        }

        [NonAction]
        public void ClearStash<TController>(Controller controller=null)where TController : Controller
        {
            var controllerType = controller?.GetType() ?? typeof(TController);

            foreach (var key in Session.Keys.ToList())
                if (key.IsMatch($"{controllerType}:[0-9A-Za-z/./_]*:Model$"))
                    Session.Remove(key);
        }

        [NonAction]
        protected void ClearAllStashes()
        {
            foreach (var key in Session.Keys.ToList())
                if (key.IsMatch(":[0-9A-Za-z/./_]*:Model$"))
                    Session.Remove(key);
        }

        [NonAction]
        protected TModel UnstashModel<TModel>(bool delete = false) where TModel : class
        {
            var controllerType = this.GetType();
            var modelType = typeof(TModel);

            var keyName = $"{controllerType}:{modelType}:Model";
            var json = Session[keyName].ToStringOrNull();
            var result = string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<TModel>(json);
            if (delete) Session.Remove(keyName);
            return result;
        }

        [NonAction]
        protected TModel UnstashModel<TModel>(Controller controller, bool delete = false) where TModel : class
        {
            var controllerType = controller.GetType();

            var modelType = typeof(TModel);

            var keyName = $"{controllerType}:{modelType}:Model";

            var json = Session[keyName].ToStringOrNull();
            var result = string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<TModel>(json);
            if (delete) Session.Remove(keyName);

            return result;
        }

        #endregion
    }
}