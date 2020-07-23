using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Submission.Models;
using ModernSlavery.WebUI.Submission.Models.Statement;
using ModernSlavery.WebUI.Submission.Presenters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.WebUI.Submission.Controllers
{
    [Area("Submission")]
    [Route("statement")]
    [Authorize]
    public class StatementController : BaseController
    {
        readonly IStatementPresenter SubmissionPresenter;

        public StatementController(
            IStatementPresenter submissionPresenter,
            ILogger<StatementController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic)
            : base(logger, webService, sharedBusinessLogic)
        {
            SubmissionPresenter = submissionPresenter;

        }

        #region General notes

        // I feel that the pattern will be the same for all the steps apart from the reivew
        // Is that correct?

        // Is an identfier parameter nessesary
        // is the current user enough to find the relevent submission?
        // might also require organisation and (maybe) year, any other parameters?

        // Authorization will be almost identical for all actions
        // except submit review, which might need some extra

        // organisation has URL slug - find and use

        // Year might tie to accounting date
        // Year being reported will be different to the deadline of the report

        #endregion

        private string ReturnUrl => Url.Action("ManageOrganisation", new { organisationIdentifier=RouteData.Values["organisationIdentifier"].ToString() });
        private string CancelUrl => Url.Action("Cancel", new { organisationIdentifier = RouteData.Values["organisationIdentifier"].ToString(), year= RouteData.Values["year"].ToString() });


        /// <summary>
        /// Returns an ActionResult to handle any StatementErrors
        /// </summary>
        /// <param name="errors">A List of Statement Errors and their description</param>
        /// <returns>The IActionResult to execute</returns>
        private IActionResult HandleStatementErrors(IEnumerable<(StatementErrors Error, string Message)> errors)
        {
            //Return full page errors which return to the ManageOrganisation page
            var error = errors.FirstOrDefault();
            switch (error.Error)
            {
                case StatementErrors.NotFound:
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1152, error));
                case StatementErrors.Unauthorised:
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1153));
                case StatementErrors.Locked:
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1154, error));
                case StatementErrors.TooLate:
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1155));
                default:
                    throw new NotImplementedException($"{nameof(StatementErrors)} type '{error.Error}' is not recognised");
            }
        }

        private object GetOrgAndYearRouteData() => new { OrganisationIdentifier, year = ReportingDeadlineYear };

        [BindProperty]
        public string OrganisationIdentifier { get; set; }

        [BindProperty(Name ="year")]
        public string ReportingDeadlineYear { get; set; }

        private void SetNavigationUrl<TViewModel>(TViewModel viewModel)
        {
            switch (viewModel)
            {
                case YourStatementPageViewModel vm:
                    vm.BackUrl = ReturnUrl;
                    vm.CancelUrl = vm.CanRevertToBackup ? CancelUrl : ReturnUrl;
                    vm.ContinueUrl = Url.Action(nameof(this.Compliance), GetOrgAndYearRouteData());
                    break;
                case CompliancePageViewModel vm:
                    vm.BackUrl = Url.Action(nameof(this.YourStatement), GetOrgAndYearRouteData());
                    vm.CancelUrl = vm.CanRevertToBackup ? CancelUrl : ReturnUrl;
                    vm.ContinueUrl = Url.Action(nameof(this.YourOrganisation), GetOrgAndYearRouteData());
                    break;
                case OrganisationPageViewModel vm:
                    vm.BackUrl = Url.Action(nameof(this.Compliance), GetOrgAndYearRouteData());
                    vm.CancelUrl = vm.CanRevertToBackup ? CancelUrl : ReturnUrl;
                    vm.ContinueUrl = Url.Action(nameof(this.Policies), GetOrgAndYearRouteData());
                    break;
                case PoliciesPageViewModel vm:
                    vm.BackUrl = Url.Action(nameof(this.YourOrganisation), GetOrgAndYearRouteData());
                    vm.CancelUrl = vm.CanRevertToBackup ? CancelUrl : ReturnUrl;
                    vm.ContinueUrl = Url.Action(nameof(this.SupplyChainRisks), GetOrgAndYearRouteData());
                    break;
                case RisksPageViewModel vm:
                    vm.BackUrl = Url.Action(nameof(this.Policies), GetOrgAndYearRouteData());
                    vm.CancelUrl = vm.CanRevertToBackup ? CancelUrl : ReturnUrl;
                    vm.ContinueUrl = Url.Action(nameof(this.DueDiligence), GetOrgAndYearRouteData());
                    break;
                case DueDiligencePageViewModel vm:
                    vm.BackUrl = Url.Action(nameof(this.SupplyChainRisks), GetOrgAndYearRouteData());
                    vm.CancelUrl = vm.CanRevertToBackup ? CancelUrl : ReturnUrl;
                    vm.ContinueUrl = Url.Action(nameof(this.Training), GetOrgAndYearRouteData());
                    break;
                case TrainingPageViewModel vm:
                    vm.BackUrl = Url.Action(nameof(this.DueDiligence), GetOrgAndYearRouteData());
                    vm.CancelUrl = vm.CanRevertToBackup ? CancelUrl : ReturnUrl;
                    vm.ContinueUrl = Url.Action(nameof(this.MonitoringProgress), GetOrgAndYearRouteData());
                    break;
                case ProgressPageViewModel vm:
                    vm.BackUrl = Url.Action(nameof(this.Training), GetOrgAndYearRouteData());
                    vm.CancelUrl = vm.CanRevertToBackup ? CancelUrl : ReturnUrl;
                    vm.ContinueUrl = Url.Action(nameof(this.Review), GetOrgAndYearRouteData());
                    break;
                case ReviewPageViewModel vm:
                    vm.BackUrl = Url.Action(nameof(this.MonitoringProgress), GetOrgAndYearRouteData());
                    vm.CancelUrl = vm.CanRevertToBackup ? CancelUrl : ReturnUrl;
                    vm.ContinueUrl = ReturnUrl;
                    break;
                case CancelPageViewModel vm:
                    vm.BackUrl = vm.CancelUrl = HttpContext.GetUrlReferrer().PathAndQuery;
                    vm.ContinueUrl = ReturnUrl;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private async Task<IActionResult> GetAsync<TViewModel>(string organisationIdentifier, int year) where TViewModel : BaseViewModel
        {
            //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
            var viewModelResult = await SubmissionPresenter.GetViewModelAsync<TViewModel>(organisationIdentifier, year, VirtualUser.UserId);

            //Handle any StatementErrors
            if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

            //Get the view model and set the navigation urls
            var viewModel = viewModelResult.Result;
            SetNavigationUrl(viewModel);

            //Otherwise return the view using the populated ViewModel
            return View(viewModel);
        }

        private async Task<IActionResult> PostAsync<TViewModel>(TViewModel viewModel, string organisationIdentifier, int year) where TViewModel:BaseViewModel
        {
            //Validate the submitted ViewModel data
            if (!ModelState.IsValid) return View(viewModel);

            //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
            var viewModelResult = await SubmissionPresenter.SaveViewModelAsync(viewModel, organisationIdentifier, year, VirtualUser.UserId);

            //Handle any StatementErrors
            if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

            //Redirect to the continue url
            return Redirect(viewModel.ContinueUrl);
        }


        #region Before You Start
        [HttpGet("{organisationIdentifier}/{year}/before-you-start")]
        public async Task<IActionResult> BeforeYouStart(string organisationIdentifier, int year)
        {
            //Check if the current user can open edit draft statement data for this organisation, reporting year
            var openResult = await SubmissionPresenter.OpenDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);
            if (openResult.Fail) return HandleStatementErrors(openResult.Errors);

            //Show the correct view
            return View(Url.Action("YourStatement",new { OrganisationIdentifier = organisationIdentifier, Year = year }));
        }
        #endregion

        #region Step 1 - Your statement

        [HttpGet("{organisationIdentifier}/{year}/your-statement")]
        public async Task<IActionResult> YourStatement(string organisationIdentifier, int year)
        {
            return await GetAsync<YourStatementPageViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/your-statement")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YourStatement(YourStatementPageViewModel viewModel, string organisationIdentifier, int year)
        {
            return await PostAsync(viewModel, organisationIdentifier, year);
        }

        #endregion

        #region Step 2 - Compliance

        [HttpGet("{organisationIdentifier}/{year}/compliance")]
        public async Task<IActionResult> Compliance(string organisationIdentifier, int year)
        {
            return await GetAsync<CompliancePageViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/compliance")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Compliance(CompliancePageViewModel viewModel,string organisationIdentifier, int year)
        {
            return await PostAsync(viewModel, organisationIdentifier, year);
        }

        #endregion

        #region Step 3 - Your organisation

        [HttpGet("{organisationIdentifier}/{year}/your-organisation")]
        public async Task<IActionResult> YourOrganisation(string organisationIdentifier, int year)
        {
            return await GetAsync<OrganisationPageViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/your-organisation")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YourOrganisation(OrganisationPageViewModel viewModel, string organisationIdentifier, int year)
        {
            return await PostAsync(viewModel, organisationIdentifier, year);
        }
        #endregion

        #region Step 4 - Policies

        [HttpGet("{organisationIdentifier}/{year}/policies")]
        public async Task<IActionResult> Policies(string organisationIdentifier, int year)
        {
            return await GetAsync<PoliciesPageViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/policies")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Policies(PoliciesPageViewModel viewModel, string organisationIdentifier, int year)
        {
            return await PostAsync(viewModel, organisationIdentifier, year);
        }

        #endregion

        #region Step 5 - Supply chain risks and due diligence

        [HttpGet("{organisationIdentifier}/{year}/supply-chain-risks")]
        public async Task<IActionResult> SupplyChainRisks(string organisationIdentifier, int year)
        {
            return await GetAsync<RisksPageViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/supply-chain-risks")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupplyChainRisks(RisksPageViewModel viewModel, string organisationIdentifier, int year)
        {
            return await PostAsync(viewModel, organisationIdentifier, year);
        }
        #endregion

        #region Step 6 - Due Diligence

        [HttpGet("{organisationIdentifier}/{year}/due-diligence")]
        public async Task<IActionResult> DueDiligence(string organisationIdentifier, int year)
        {
            return await GetAsync<DueDiligencePageViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/due-diligence")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DueDiligence(DueDiligencePageViewModel viewModel, string organisationIdentifier, int year)
        {
            return await PostAsync(viewModel, organisationIdentifier, year);
        }
        #endregion

        #region Step 7 - Training

        [HttpGet("{organisationIdentifier}/{year}/training")]
        public async Task<IActionResult> Training(string organisationIdentifier, int year)
        {
            return await GetAsync<TrainingPageViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/training")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Training(TrainingPageViewModel viewModel, string organisationIdentifier, int year)
        {
            return await PostAsync(viewModel, organisationIdentifier, year);
        }
        #endregion


        #region Step 8 - Monitoring progress

        [HttpGet("{organisationIdentifier}/{year}/monitoring-progress")]
        public async Task<IActionResult> MonitoringProgress(string organisationIdentifier, int year)
        {
            return await GetAsync<ProgressPageViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/monitoring-progress")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MonitoringProgress(ProgressPageViewModel viewModel, string organisationIdentifier, int year)
        {
            return await PostAsync(viewModel, organisationIdentifier, year);
        }
        #endregion

        #region Step 9 - Review

        [HttpGet("{organisationIdentifier}/{year}/review-statement")]
        public async Task<IActionResult> Review(string organisationIdentifier, int year)
        {
            //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
            var viewModelResult = await SubmissionPresenter.OpenDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

            //Handle any StatementErrors
            if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

            //Get the view model and set the navigation urls
            var viewModel = new ReviewPageViewModel();
            SetNavigationUrl(viewModel);

            //Otherwise return the view using the populated ViewModel
            return View(viewModelResult.Result);
        }


        [HttpPost("{organisationIdentifier}/{year}/review-statement")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(ReviewPageViewModel viewModel, string organisationIdentifier, int year)
        {
            //Validate the submitted ViewModel data
            if (!ModelState.IsValid) return View(viewModel);

            //Ensure all sections are complete
            if (viewModel.IsComplete()) throw new ValidationException("Submitting an incomplete statement is not permitted");

            //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
            var viewModelResult = await SubmissionPresenter.SubmitDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

            //Handle any StatementErrors
            if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

            //Redirect to the continue url
            return Redirect(viewModel.ContinueUrl);
        }

        #endregion

        #region Cancel

        [HttpGet("{organisationIdentifier}/{year}/cancel-statement")]
        public async Task<IActionResult> Cancel(string organisationIdentifier, int year)
        {
            //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
            var viewModelResult = await SubmissionPresenter.OpenDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

            //Handle any StatementErrors
            if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

            //Get the view model and set the navigation urls
            var viewModel = new CancelPageViewModel();
            SetNavigationUrl(viewModel);

            viewModel.CancelUrl = viewModel.BackUrl = HttpContext.GetUrlReferrer().ToString();
            viewModel.ContinueUrl = ReturnUrl;

            //Otherwise return the view using the populated ViewModel
            return View(viewModel);
        }


        [HttpPost("{organisationIdentifier}/{year}/cancel-statement")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(CancelPageViewModel viewModel, string organisationIdentifier, int year)
        {
            //Validate the submitted ViewModel data
            if (!ModelState.IsValid) return View(viewModel);

            //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
            var viewModelResult = await SubmissionPresenter.CancelDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

            //Handle any StatementErrors
            if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

            //Redirect to the continue url
            return Redirect(viewModel.ContinueUrl);
        }

        #endregion
    }
}
