using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Options;
using ModernSlavery.WebUI.Submission.Classes;
using ModernSlavery.WebUI.Submission.Models;

namespace ModernSlavery.WebUI.Submission.Controllers
{
    [Area("Submission")]
    [Route("scope")]
    public class ScopeController : BaseController
    {
        #region Constructors

        public ScopeController(
            ISubmissionService submissionService,
            IScopePresenter scopeUI,
            ILogger<ScopeController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(
            logger, webService, sharedBusinessLogic)
        {
            SubmissionService = submissionService;
            _sharedBusinessLogic = sharedBusinessLogic;
            ScopePresentation = scopeUI;
        }

        #endregion

        [HttpGet("out")]
        public async Task<IActionResult> OutOfScope()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && _sharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser))
                return RedirectToActionArea("Home", "Admin", "Admin");

            var currentStateModel = UnstashModel<ScopingViewModel>();
            var model = currentStateModel?.EnterCodes ?? new EnterCodesViewModel();

            // when spamlocked then return a CustomError view
            var remainingTime =
                await GetRetryLockRemainingTimeAsync("lastScopeCode", SharedBusinessLogic.SharedOptions.LockoutMinutes);
            if (remainingTime > TimeSpan.Zero)
                return View("CustomError",
                    WebService.ErrorViewModelFactory.Create(1125,
                        new { remainingTime = remainingTime.ToFriendly(maxParts: 2) }));

            PendingFasttrackCodes = null;

            // show the view
            return View("EnterCodes", model);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("out")]
        public async Task<IActionResult> OutOfScope(EnterCodesViewModel model)
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && _sharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser))
                return RedirectToActionArea("Home", "Admin", "Admin");

            // When Spamlocked then return a CustomError view
            var remainingTime =
                await GetRetryLockRemainingTimeAsync("lastScopeCode", SharedBusinessLogic.SharedOptions.LockoutMinutes);
            if (remainingTime > TimeSpan.Zero)
                return View("CustomError",
                    WebService.ErrorViewModelFactory.Create(1125,
                        new { remainingTime = remainingTime.ToFriendly(maxParts: 2) }));

            // the following fields are validatable at this stage
            ModelState.Include(
                nameof(EnterCodesViewModel.OrganisationReference),
                nameof(EnterCodesViewModel.SecurityToken));

            // When ModelState is Not Valid Then Return the EnterCodes View
            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<EnterCodesViewModel>();
                return View("EnterCodes", model);
            }

            // Generate the state model
            var stateModel = await ScopePresentation.CreateScopingViewModelAsync(model, CurrentUser);

            if (stateModel == null)
            {
                await IncrementRetryCountAsync("lastScopeCode", SharedBusinessLogic.SharedOptions.LockoutMinutes);
                ModelState.AddModelError(3027);
                this.SetModelCustomErrors<EnterCodesViewModel>();
                return View("EnterCodes", model);
            }


            //Clear the retry locks
            await ClearRetryLocksAsync("lastScopeCode");

            // set the back link
            stateModel.StartUrl = Url.ActionArea("OutOfScope", "Scope", "Submission");

            // set the journey to out-of-scope
            stateModel.IsOutOfScopeJourney = true;

            // save the state to the session cache
            StashModel(stateModel);

            // when security code has expired then redirect to the CodeExpired action
            if (stateModel.IsSecurityCodeExpired) return View("CodeExpired", stateModel);

            // redirect to next step
            return RedirectToAction("ConfirmOutOfScopeDetails");
        }

        [Authorize]
        [HttpGet("in")]
        public async Task<IActionResult> InScope()
        {
            await TrackPageViewAsync();

            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && _sharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser))
                return RedirectToActionArea("Home", "Admin", "Admin");

            return RedirectToAction(Url.Action(nameof(SubmissionController.ManageOrganisations)));
        }

        [Authorize]
        [HttpGet("in/confirm")]
        public async Task<IActionResult> ConfirmInScope()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && _sharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser))
                return RedirectToActionArea("Home", "Admin", "Admin");

            var stateModel = UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null) return SessionExpiredView();

            // else redirect to ConfirmDetails action
            return View("ConfirmInScope", stateModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost("in/confirm")]
        public async Task<IActionResult> ConfirmInScope(string command)
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && _sharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser))
                return RedirectToActionArea("Home", "Admin", "Admin");

            var stateModel = UnstashModel<ScopingViewModel>(true);
            // when model is null then return session expired view
            if (stateModel == null) return SessionExpiredView();

            ApplyUserContactDetails(CurrentUser, stateModel);

            // Save user as in scope
            var years = new HashSet<int> { stateModel.DeadlineDate.Year };
            await ScopePresentation.SaveScopesAsync(stateModel, years);

            var organisation = SharedBusinessLogic.DataRepository.Get<Organisation>(stateModel.OrganisationId);
            var currentDeadlineDate = _sharedBusinessLogic.GetReportingDeadline(organisation.SectorType);
            if (stateModel.DeadlineDate == currentDeadlineDate)
            {
                var emailAddressesForOrganisation = organisation.UserOrganisations.Select(uo => uo.User.EmailAddress);
                foreach (var emailAddress in emailAddressesForOrganisation)
                    _sharedBusinessLogic.NotificationService.SendScopeChangeInEmail(emailAddress,
                        organisation.OrganisationName);
            }

            //Start new user registration
            return RedirectToAction("ManageOrganisation", "Submission",
                new { organisationIdentifier = SharedBusinessLogic.Obfuscator.Obfuscate(stateModel.OrganisationId.ToString()) });
        }


        [HttpGet("out/confirm-organisation")]
        public async Task<IActionResult> ConfirmOutOfScopeDetails()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && _sharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser))
                return RedirectToActionArea("Home", "Admin", "Admin");

            var stateModel = UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null) return SessionExpiredView();

            // else redirect to ConfirmDetails action
            return View("ConfirmOutOfScopeDetails", stateModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("out/confirm-organisation")]
        public async Task<IActionResult> ConfirmOutOfScopeDetails(string command)
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && _sharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser))
                return RedirectToActionArea("Home", "Admin", "Admin");

            var stateModel = UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null) return SessionExpiredView();

            //When on out-of-scope journey and any previous explicit scope then tell user scope is known
            if (!stateModel.IsChangeJourney
                && (
                    stateModel.LastScope != null &&
                    stateModel.LastScope.ScopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope)
                    || stateModel.ThisScope != null
                    && stateModel.ThisScope.ScopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope)
                )
            )
                return View("ScopeKnown", stateModel);

            //when organisation is already registered then tell user 
            if (!stateModel.IsChangeJourney
                && (
                    stateModel.ThisScope != null || stateModel.LastScope != null
                    ))
                return View("AlreadyRegistered", stateModel);


            // else redirect to EnterAnswers action
            return RedirectToAction("EnterOutOfScopeAnswers");
        }

        [HttpGet("out/questions")]
        public async Task<IActionResult> EnterOutOfScopeAnswers()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && _sharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser))
                return RedirectToActionArea("Home", "Admin", "Admin");

            var stateModel = UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null) return SessionExpiredView();

            return View("EnterOutOfScopeAnswers", stateModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("out/questions")]
        public async Task<IActionResult> EnterOutOfScopeAnswers(EnterAnswersViewModel enterAnswersModel)
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && _sharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser))
                return RedirectToActionArea("Home", "Admin", "Admin");

            // update the state model
            var stateModel = UnstashModel<ScopingViewModel>();
            if (stateModel == null) return SessionExpiredView();

            // update the state
            stateModel.EnterAnswers = enterAnswersModel;
            StashModel(stateModel);
            var fields = new List<string>();

            // when the user is not logged in then validate the contact details
            if (CurrentUser == null)
            {
                fields.Add(nameof(EnterAnswersViewModel.FirstName));
                fields.Add(nameof(EnterAnswersViewModel.LastName));
                fields.Add(nameof(EnterAnswersViewModel.EmailAddress));
            }

            // the following fields are validatable at this stage
            fields.Add(nameof(EnterAnswersViewModel.Reason));
            if (enterAnswersModel.Reason == "Other") fields.Add(nameof(EnterAnswersViewModel.OtherReason));

            fields.Add(nameof(EnterAnswersViewModel.TurnOver));
            //fields.Add(nameof(EnterAnswersViewModel.ReadGuidance));

            ModelState.Include(fields.ToArray());

            // validate the details
            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<ScopingViewModel>();
                return View("EnterOutOfScopeAnswers", stateModel);
            }

            //Ensure email is always lower case
            if (!string.IsNullOrEmpty(enterAnswersModel.EmailAddress))
                enterAnswersModel.EmailAddress = enterAnswersModel.EmailAddress.ToLower();

            StashModel(stateModel);

            //Start new user registration
            return RedirectToAction("ConfirmOutOfScopeAnswers");
        }

        [HttpGet("out/confirm-answers")]
        public async Task<IActionResult> ConfirmOutOfScopeAnswers()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && _sharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser))
                return RedirectToActionArea("Home", "Admin", "Admin");

            var stateModel = UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null) return SessionExpiredView();

            return View("ConfirmOutOfScopeAnswers", stateModel);
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("out/confirm-answers")]
        public async Task<IActionResult> ConfirmOutOfScopeAnswers(string command)
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && _sharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser))
                return RedirectToActionArea("Home", "Admin", "Admin");

            var stateModel = UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null) return SessionExpiredView();

            ApplyUserContactDetails(CurrentUser, stateModel);

            // Save user as out of scope
            // TODO James - review snapshot year, it will be submission deadlines how does this impact?
            var years = new HashSet<int> { stateModel.DeadlineDate.Year };
            if (!stateModel.IsChangeJourney) years.Add(stateModel.DeadlineDate.Year - 1);

            await ScopePresentation.SaveScopesAsync(stateModel, years);

            StashModel(stateModel);

            var organisation = SharedBusinessLogic.DataRepository.Get<Organisation>(stateModel.OrganisationId);
            var currentDeadlineDate = _sharedBusinessLogic.GetReportingDeadline(organisation.SectorType);
            if (stateModel.DeadlineDate == currentDeadlineDate)
            {
                var emailAddressesForOrganisation = organisation.UserOrganisations.Select(uo => uo.User.EmailAddress);
                foreach (var emailAddress in emailAddressesForOrganisation)
                    _sharedBusinessLogic.NotificationService.SendScopeChangeOutEmail(emailAddress,
                        organisation.OrganisationName);
            }
            if (stateModel.EnterAnswers.RequiresEmailConfirmation)
            {
                //TODO: confirm this email logic - should it be a new template or same as one above?
                _sharedBusinessLogic.NotificationService.SendScopeChangeOutEmail(stateModel.EnterAnswers.EmailAddress, organisation.OrganisationName);
            }

            //Start new user registration
            return RedirectToAction("FinishOutOfScope", "Scope");
        }

        [HttpGet("out/finish")]
        public async Task<IActionResult> FinishOutOfScope()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && _sharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser))
                return RedirectToActionArea("Home", "Admin", "Admin");

            var stateModel = UnstashModel<ScopingViewModel>();
            // when model is null then return session expired view
            if (stateModel == null) return SessionExpiredView();

            StashModel(stateModel);

            //Complete
            return View("FinishOutOfScope", stateModel);
        }

        [HttpGet("register")]
        public async Task<IActionResult> RegisterOrManage()
        {
            // When User is Admin then redirect to Admin\Home
            if (CurrentUser != null && _sharedBusinessLogic.AuthorisationBusinessLogic.IsAdministrator(CurrentUser))
                return RedirectToActionArea("Home", "Admin", "Admin");

            var stateModel = UnstashModel<ScopingViewModel>(true);
            // when model is null then return session expired view
            if (stateModel == null) return SessionExpiredView();

            //if user has already registered then manage that organisation
            if (stateModel.UserIsRegistered)
                return RedirectToAction(
                    "ManageOrganisation",
                    "Submission",
                    new { id = SharedBusinessLogic.Obfuscator.Obfuscate(stateModel.OrganisationId.ToString()) });

            // when not auth then save codes and return ManageOrganisations redirect
            if (!stateModel.IsSecurityCodeExpired)
                PendingFasttrackCodes =
                    $"{stateModel.EnterCodes.OrganisationReference}:{stateModel.EnterCodes.SecurityToken}:{stateModel.EnterAnswers?.FirstName}:{stateModel.EnterAnswers?.LastName}:{stateModel.EnterAnswers?.EmailAddress}";

            return RedirectToAction(Url.Action(nameof(SubmissionController.ManageOrganisations)));
        }

        [Authorize]
        [HttpGet("~/declare-scope/{id}")]
        public async Task<IActionResult> DeclareScope(string id)
        {
            //Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // Decrypt org id
            long organisationId = SharedBusinessLogic.Obfuscator.DeObfuscate(id);
            if (organisationId == 0)
                return new HttpBadRequestResult($"Cannot decrypt organisation id {id}");

            // Check the user has permission for this organisation
            var userOrg = VirtualUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
                return new HttpForbiddenResult(
                    $"User {VirtualUser?.EmailAddress} is not registered for organisation id {organisationId}");

            // Ensure this user is registered fully for this organisation
            if (userOrg.PINConfirmedDate == null)
                return new HttpForbiddenResult(
                    $"User {VirtualUser?.EmailAddress} has not completed registration for organisation {userOrg.Organisation.OrganisationReference}");

            //Get the current snapshot date
            var reportingDeadline = SharedBusinessLogic.GetReportingDeadline(userOrg.Organisation.SectorType).AddYears(-2);
            if (reportingDeadline.Year < SharedBusinessLogic.SharedOptions.FirstReportingDeadlineYear)
                return new HttpBadRequestResult($"Snapshot year {reportingDeadline} is invalid");

            var scopeStatus =
                await SubmissionService.ScopeBusinessLogic.GetScopeStatusByReportingDeadlineOrLatestAsync(organisationId,
                    reportingDeadline);
            if (scopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope))
                return new HttpBadRequestResult("Explicit scope is already set");

            // build the view model
            var model = new DeclareScopeModel
            { OrganisationName = userOrg.Organisation.OrganisationName, ReportingDeadline = reportingDeadline };

            return View(model);
        }

        [PreventDuplicatePost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost("~/declare-scope/{id}")]
        public async Task<IActionResult> DeclareScope(DeclareScopeModel model, string id)
        {
            // Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // Decrypt org id
            long organisationId = SharedBusinessLogic.Obfuscator.DeObfuscate(id);
            if (organisationId == 0)
                return new HttpBadRequestResult($"Cannot decrypt organisation id {id}");


            // Check the user has permission for this organisation
            var userOrg = VirtualUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
                return new HttpForbiddenResult(
                    $"User {VirtualUser?.EmailAddress} is not registered for organisation id {organisationId}");

            // Ensure this user is registered fully for this organisation
            if (userOrg.PINConfirmedDate == null)
                return new HttpForbiddenResult(
                    $"User {VirtualUser?.EmailAddress} has not completed registeration for organisation {userOrg.Organisation.OrganisationReference}");

            //Check the year parameters
            if (model.ReportingDeadline.Year < SharedBusinessLogic.SharedOptions.FirstReportingDeadlineYear ||
                model.ReportingDeadline.Year > VirtualDateTime.Now.Year)
                return new HttpBadRequestResult($"Snapshot year {model.ReportingDeadline.Year} is invalid");

            //Check if we need the current years scope
            var scopeStatus =
                await SubmissionService.ScopeBusinessLogic.GetScopeStatusByReportingDeadlineOrLatestAsync(organisationId,
                    model.ReportingDeadline);
            if (scopeStatus.IsAny(ScopeStatuses.InScope, ScopeStatuses.OutOfScope))
                return new HttpBadRequestResult("Explicit scope is already set");

            //Validate the submitted fields
            ModelState.Clear();

            if (model.ScopeStatus == null || model.ScopeStatus == ScopeStatuses.Unknown)
                AddModelError(3032, "ScopeStatus");

            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors<DeclareScopeModel>();
                return View("DeclareScope", model);
            }

            //Create last years declared scope
            var newScope = new OrganisationScope
            {
                OrganisationId = userOrg.OrganisationId,
                Organisation = userOrg.Organisation,
                ContactEmailAddress = VirtualUser.EmailAddress,
                ContactFirstname = VirtualUser.Firstname,
                ContactLastname = VirtualUser.Lastname,
                ContactJobTitle = VirtualUser.JobTitle,
                ScopeStatus = model.ScopeStatus.Value,
                Status = ScopeRowStatuses.Active,
                ScopeStatusDate = VirtualDateTime.Now,
                SubmissionDeadline = model.ReportingDeadline
            };

            //Save the new declared scopes
            await SubmissionService.ScopeBusinessLogic.SaveScopeAsync(userOrg.Organisation, true, newScope);
            return View("ScopeDeclared", model);
        }

        [Authorize]
        [HttpGet("~/change-organisation-scope/{organisationIdentifier}/{reportingDeadlineYear}")]
        public async Task<IActionResult> ChangeOrganisationScope(string organisationIdentifier, int reportingDeadlineYear)
        {
            // Ensure user has completed the registration process
            var checkResult = await CheckUserRegisteredOkAsync();
            if (checkResult != null) return checkResult;

            // Extract the request vars
            long organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);

            // Check the user has permission for this organisation
            var userOrg = VirtualUser.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrg == null)
                return new HttpForbiddenResult(
                    $"User {VirtualUser?.EmailAddress} is not registered for organisation id {organisationId}");

            // Generate the scope state model
            var stateModel = ScopePresentation.CreateScopingViewModel(userOrg.Organisation, CurrentUser);

            // Get the latest scope for the reporting year
            var latestScope = stateModel.ThisScope?.DeadlineDate.Year == reportingDeadlineYear ? stateModel.ThisScope :
                stateModel.LastScope;
            //TODO: check logic for this one
            // stateModel.LastScope.SnapshotDate.Year == reportingDeadlineYear ? stateModel.LastScope : null;

            // Set the return url
            stateModel.StartUrl = Url.Action("ManageOrganisation", "Submission",
                new { organisationIdentifier = SharedBusinessLogic.Obfuscator.Obfuscate(organisationId.ToString()) });
            stateModel.IsChangeJourney = true;
            stateModel.DeadlineDate = latestScope.DeadlineDate;

            //Set the in/out journey type
            stateModel.IsOutOfScopeJourney =
                latestScope.ScopeStatus.IsAny(ScopeStatuses.PresumedInScope, ScopeStatuses.InScope);

            // Stash the model for the scope controller
            StashModel(typeof(ScopeController), stateModel);

            if (stateModel.IsOutOfScopeJourney) return RedirectToAction("EnterOutOfScopeAnswers", "Scope");

            return RedirectToAction("ConfirmInScope", "Scope");
        }

        private void ApplyUserContactDetails(User VirtualUser, ScopingViewModel model)
        {
            // when logged in then override contact details
            if (VirtualUser != null)
            {
                model.EnterAnswers.FirstName = VirtualUser.Firstname;
                model.EnterAnswers.LastName = VirtualUser.Lastname;
                model.EnterAnswers.EmailAddress = VirtualUser.EmailAddress;
            }
        }

        #region Dependencies

        private readonly ISubmissionService SubmissionService;
        public ISharedBusinessLogic _sharedBusinessLogic { get; set; }

        public IScopePresenter ScopePresentation { get; }

        #endregion
    }
}