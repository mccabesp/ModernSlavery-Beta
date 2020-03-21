using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Extensions;
using ModernSlavery.WebUI.Views.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.Entities;
using ModernSlavery.WebUI.Presenters;
using ModernSlavery.BusinessLogic;
using ModernSlavery.WebUI.Shared.Classes.Cookies;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Controllers
{
    public class HomeController : BaseController
    {

        #region Constructors

        public HomeController(
            IEventLogger customLogger,
            IScopePresenter scopeUIService,
            IShortCodesRepository shortCodesRepository,
            ILogger<HomeController> logger, IWebService webService, ICommonBusinessLogic commonBusinessLogic) : base(logger, webService, commonBusinessLogic)
        {
            CustomLogger = customLogger;
            ScopePresentation = scopeUIService;
            ShortCodesRepository = shortCodesRepository;
        }

        #endregion

        #region Dependencies
        private readonly IEventLogger CustomLogger;

        public IScopePresenter ScopePresentation { get; }
        public IShortCodesRepository ShortCodesRepository { get; }

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
                List<ShortCodeModel> allShortCodes = await ShortCodesRepository.GetAllShortCodesAsync();
                ShortCodeModel code = allShortCodes?.FirstOrDefault(sc => sc.ShortCode.EqualsI(shortCode));

                if (code != null && !string.IsNullOrWhiteSpace(code.Path))
                {
                    if (code.ExpiryDate.HasValue && code.ExpiryDate.Value < VirtualDateTime.Now)
                    {
                        return View(
                            "CustomError",
                            WebService.ErrorViewModelFactory.Create(1135, new {expiryDate = code.ExpiryDate.Value.ToString("d MMMM yyyy")}));
                    }

                    await TrackPageViewAsync();

                    string url = $"{code.Path.TrimI(@" .~\")}{RequestUrl.Query}";
                    return new RedirectResult(url);
                }
            }

            return View("CustomError", WebService.ErrorViewModelFactory.Create(1136));
        }

        [HttpGet("~/sign-out")]
        public IActionResult SignOut()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("SignOut");
            }

            string returnUrl = Url.Action("SignOut", "Home", null, "https");

            return LogoutUser(returnUrl);
        }

        [HttpGet("~/session-expired")]
        public IActionResult SessionExpired()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("SessionExpired");
            }

            return LogoutUser(Url.Action("SessionExpired", "Home", null, "https"));
        }

        #region Contact Us

        [HttpGet("~/contact-us")]
        public IActionResult ContactUs()
        {
            return View("ContactUs");
        }

        #endregion

        #region Initialisation

        /// <summary>
        ///     This action is only used to warm up this controller on initialisation
        /// </summary>
        /// <returns></returns>
        [HttpGet("Init")]
        public IActionResult Init()
        {
            if (!CommonBusinessLogic.GlobalOptions.IsProduction())
            {
                Logger.LogInformation("Home Controller Initialised");
            }

            return new EmptyResult();
        }

        #endregion

        [HttpGet("~/report-concerns")]
        public IActionResult ReportConcerns()
        {
            return View(nameof(ReportConcerns));
        }

        [HttpGet("~/cookies")]
        public IActionResult CookieSettingsGet()
        {
            CookieSettings cookieSettings = CookieHelper.GetCookieSettingsCookie(Request);

            var cookieSettingsViewModel = new CookieSettingsViewModel {
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
            var cookieSettings = new CookieSettings {
                GoogleAnalyticsGpg = cookieSettingsViewModel != null && cookieSettingsViewModel.GoogleAnalyticsGpg == "On",
                GoogleAnalyticsGovUk = cookieSettingsViewModel != null && cookieSettingsViewModel.GoogleAnalyticsGovUk == "On",
                ApplicationInsights = cookieSettingsViewModel != null && cookieSettingsViewModel.ApplicationInsights == "On",
                RememberSettings = cookieSettingsViewModel != null && cookieSettingsViewModel.RememberSettings == "On"
            };

            CookieHelper.SetCookieSettingsCookie(Response, cookieSettings);
            CookieHelper.SetSeenCookieMessageCookie(Response);

            cookieSettingsViewModel.ChangesHaveBeenSaved = true;

            CustomLogger.Information(
                "Updated cookie settings",
                new {
                    CookieSettings = cookieSettings,
                    HttpRequestMethod = HttpContext.Request.Method,
                    HttpRequestPath = HttpContext.Request.Path.Value
                });

            return View("CookieSettings", cookieSettingsViewModel);
        }

        [HttpPost("~/accept-all-cookies")]
        public IActionResult AcceptAllCookies()
        {
            var cookieSettings = new CookieSettings {
                GoogleAnalyticsGpg = true, GoogleAnalyticsGovUk = true, ApplicationInsights = true, RememberSettings = true
            };

            CookieHelper.SetCookieSettingsCookie(Response, cookieSettings);
            CookieHelper.SetSeenCookieMessageCookie(Response);

            return RedirectToAction("Index", "Viewing");
        }

        [HttpGet("~/cookie-details")]
        public IActionResult CookieDetails()
        {
            return View("CookieDetails");
        }

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
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("PrivacyPolicy");
            }

            //Ensure user has completed the registration process
            User currentUser;
            IActionResult checkResult = CheckUserRegisteredOk(out currentUser);
            if (checkResult == null)
            {
                return checkResult;
            }

            if (!IsImpersonatingUser && !CurrentUser.IsAdministrator())
            {
                // check if the user has accepted the privacy statement
                DateTime? hasReadPrivacy = currentUser.AcceptedPrivacyStatement;
                if (hasReadPrivacy == null || hasReadPrivacy.ToDateTime() < CommonBusinessLogic.GlobalOptions.PrivacyChangedDate)
                {
                    currentUser.AcceptedPrivacyStatement = VirtualDateTime.Now;
                    await CommonBusinessLogic.DataRepository.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(OrganisationController.ManageOrganisations), "Organisation");
        }

        #endregion

    }
}
