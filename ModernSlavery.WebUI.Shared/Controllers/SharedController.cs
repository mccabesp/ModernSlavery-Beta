using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Parsers;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Cookies;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Models;

namespace ModernSlavery.WebUI.Shared.Controllers
{
    public class SharedController : BaseController
    {
        public SharedController(ILogger<SharedController> logger, IWebService webService,
            ISharedBusinessLogic sharedBusinessLogic) : base(logger, webService, sharedBusinessLogic)
        {
        }

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
                                new { expiryDate = code.ExpiryDate.Value.ToString("d MMMM yyyy") }));

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

        #region Feedback
        [HttpGet("send-feedback")]
        public IActionResult SendFeedback()
        {
            var model = new FeedbackViewModel();

            PrePopulateEmailAndPhoneNumberFromLoggedInUser(model);

            return View("SendFeedback", model);
        }

        private void PrePopulateEmailAndPhoneNumberFromLoggedInUser(FeedbackViewModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = SharedBusinessLogic.DataRepository.FindUser(User);

                model.EmailAddress =
                    !string.IsNullOrWhiteSpace(user.ContactEmailAddress)
                        ? user.ContactEmailAddress
                        : user.EmailAddress;

                model.PhoneNumber = user.ContactPhoneNumber;
            }
        }

        [HttpPost("send-feedback")]
        public async Task<IActionResult> SendFeedback(FeedbackViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.HowEasyIsThisServiceToUse);
            viewModel.ParseAndValidateParameters(Request, m => m.HowDidYouHearAboutGpg);
            viewModel.ParseAndValidateParameters(Request, m => m.OtherSourceText);
            viewModel.ParseAndValidateParameters(Request, m => m.WhyVisitGpgSite);
            viewModel.ParseAndValidateParameters(Request, m => m.OtherReasonText);
            viewModel.ParseAndValidateParameters(Request, m => m.WhoAreYou);
            viewModel.ParseAndValidateParameters(Request, m => m.OtherPersonText);
            viewModel.ParseAndValidateParameters(Request, m => m.Details);

            if (viewModel.HasAnyErrors())
                // If there are any errors, return the user back to the same page to correct the mistakes
                return View("SendFeedback", viewModel);

            WebService.CustomLogger.Information("Feedback has been received", viewModel);

            var feedbackDatabaseModel = ConvertFeedbackViewModelIntoFeedbackDatabaseModel(viewModel);

            SharedBusinessLogic.DataRepository.Insert(feedbackDatabaseModel);
            await SharedBusinessLogic.DataRepository.SaveChangesAsync();

            return View("FeedbackSent");
        }

        private Feedback ConvertFeedbackViewModelIntoFeedbackDatabaseModel(FeedbackViewModel feedbackViewModel)
        {
            return new Feedback
            {
                Difficulty = feedbackViewModel.HowEasyIsThisServiceToUse.HasValue
                    ? (DifficultyTypes) (int) feedbackViewModel.HowEasyIsThisServiceToUse.Value
                    : (DifficultyTypes?) null,

                NewsArticle = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.NewsArticle),
                SocialMedia = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.SocialMedia),
                CompanyIntranet =
                    feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.CompanyIntranet),
                EmployerUnion = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.EmployerUnion),
                InternetSearch =
                    feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.InternetSearch),
                Charity = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.SocialMedia),
                LobbyGroup = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.LobbyGroup),
                Report = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.Report),
                OtherSource = feedbackViewModel.HowDidYouHearAboutGpg?.Contains(HowDidYouHearAboutGpg.Other),
                OtherSourceText = feedbackViewModel.OtherSourceText,

                FindOutAboutGpg = feedbackViewModel.WhyVisitGpgSite?.Contains(WhyVisitGpgSite.FindOutAboutGpg),
                ReportOrganisationGpgData =
                    feedbackViewModel.WhyVisitGpgSite?.Contains(WhyVisitGpgSite.ReportOrganisationGpgData),
                CloseOrganisationGpg =
                    feedbackViewModel.WhyVisitGpgSite?.Contains(WhyVisitGpgSite.CloseOrganisationGpg),
                ViewSpecificOrganisationGpg =
                    feedbackViewModel.WhyVisitGpgSite?.Contains(WhyVisitGpgSite.ViewSpecificOrganisationGpg),
                ActionsToCloseGpg = feedbackViewModel.WhyVisitGpgSite?.Contains(WhyVisitGpgSite.ActionsToCloseGpg),
                OtherReason = feedbackViewModel.WhyVisitGpgSite?.Contains(WhyVisitGpgSite.Other),
                OtherReasonText = feedbackViewModel.OtherReasonText,

                EmployeeInterestedInOrganisationData =
                    feedbackViewModel.WhoAreYou?.Contains(WhoAreYou.EmployeeInterestedInOrganisationData),
                ManagerInvolvedInGpgReport =
                    feedbackViewModel.WhoAreYou?.Contains(WhoAreYou.ManagerInvolvedInGpgReport),
                ResponsibleForReportingGpg =
                    feedbackViewModel.WhoAreYou?.Contains(WhoAreYou.ResponsibleForReportingGpg),
                PersonInterestedInGeneralGpg =
                    feedbackViewModel.WhoAreYou?.Contains(WhoAreYou.PersonInterestedInGeneralGpg),
                PersonInterestedInSpecificOrganisationGpg =
                    feedbackViewModel.WhoAreYou?.Contains(WhoAreYou.PersonInterestedInSpecificOrganisationGpg),
                OtherPerson = feedbackViewModel.WhoAreYou?.Contains(WhoAreYou.Other),
                OtherPersonText = feedbackViewModel.OtherPersonText,

                Details = TruncateDetails(feedbackViewModel.Details),
                EmailAddress = feedbackViewModel.EmailAddress,
                PhoneNumber = feedbackViewModel.PhoneNumber
            };
        }

        private string TruncateDetails(string details)
        {
            var truncatingLength = typeof(Feedback)
                .GetProperty(nameof(Feedback.Details))
                ?.GetCustomAttributes<MaxLengthAttribute>()
                .FirstOrDefault()
                ?.Length;

            return !string.IsNullOrEmpty(details) && details.Length >= truncatingLength
                ? details.Substring(0, truncatingLength ?? 2000)
                : details;
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

            return RedirectToActionArea("ManageOrganisations", "Submission", "Submission");
        }

        #endregion

        #region Cookies

        [HttpGet("~/cookies")]
        public IActionResult CookieSettings()
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
        public IActionResult CookieSettings(CookieSettingsViewModel cookieSettingsViewModel)
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

            return RedirectToActionArea("Index", "Viewing", "Viewing");
        }

        [HttpGet("~/cookie-details")]
        public IActionResult c()
        {
            return View("CookieDetails");
        }

        #endregion
    }
}