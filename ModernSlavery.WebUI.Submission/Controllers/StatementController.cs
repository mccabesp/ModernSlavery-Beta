using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Submission.Models.Statement;
using ModernSlavery.WebUI.Submission.Presenters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        #region Properties
        
        #endregion

        #region Url Methods
        public string GetOrganisationIdentifier() => RouteData.Values["OrganisationIdentifier"].ToString();

        public string GetReportingDeadlineYear() => RouteData.Values["Year"].ToString();

        private object GetOrgAndYearRouteData() => new { OrganisationIdentifier = GetOrganisationIdentifier(), year = GetReportingDeadlineYear() };

        private string GetReturnUrl() => Url.Action("ManageOrganisation", "Submission", new { organisationIdentifier = GetOrganisationIdentifier() });

        private string GetCancelUrl() => Url.Action(nameof(Cancel), new { organisationIdentifier = GetOrganisationIdentifier(), year = GetReportingDeadlineYear() });

        private string GetYourStatementUrl() => Url.Action(nameof(YourStatement), GetOrgAndYearRouteData());

        private string GetComplianceUrl() => Url.Action(nameof(Compliance), GetOrgAndYearRouteData());

        private string GetYourOrganisationUrl() => Url.Action(nameof(YourOrganisation), GetOrgAndYearRouteData());

        private string GetPoliciesUrl() => Url.Action(nameof(Policies), GetOrgAndYearRouteData());

        private string GetRisksUrl() => Url.Action(nameof(SupplyChainRisks), GetOrgAndYearRouteData());

        private string GetDueDiligenceUrl() => Url.Action(nameof(DueDiligence), GetOrgAndYearRouteData());

        private string GetTrainingUrl() => Url.Action(nameof(Training), GetOrgAndYearRouteData());

        private string GetProgressUrl() => Url.Action(nameof(MonitoringProgress), GetOrgAndYearRouteData());


        private string GetReviewUrl() => Url.Action(nameof(ReviewAndEdit), new { organisationIdentifier = GetOrganisationIdentifier(), year = GetReportingDeadlineYear() });

        /// <summary>
        /// Uses the type of cancelling viewmodel stored in sessionm to determine url to the page that is currently cancelling 
        /// </summary>
        /// <returns>the url to the page that is currently cancelling </returns>
        private string GetBackUrlFromCancellingViewModel()
        {
            //Get view model that is cancelling
            var pageViewModel = UnstashCancellingViewModel();

            //Return the page for the cancelling view model
            switch (pageViewModel)
            {
                case YourStatementPageViewModel vm:
                    return GetYourStatementUrl();
                case CompliancePageViewModel vm:
                    return GetComplianceUrl();
                case YourOrganisationPageViewModel vm:
                    return GetYourOrganisationUrl();
                case PoliciesPageViewModel vm:
                    return GetPoliciesUrl();
                case SupplyChainRisksPageViewModel vm:
                    return GetRisksUrl();
                case DueDiligencePageViewModel vm:
                    return GetDueDiligenceUrl();
                case TrainingPageViewModel vm:
                    return GetTrainingUrl();
                case MonitoringProgressPageViewModel vm:
                    return GetProgressUrl();
            }

            //Default to the review and edit page
            return GetReviewUrl();
        }

        /// <summary>
        /// Sets the Back, Cancel and continue url
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="viewModel"></param>
        private void SetNavigationUrl<TViewModel>(TViewModel viewModel)
        {
            switch (viewModel)
            {
                case YourStatementPageViewModel vm:
                    vm.BackUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetReturnUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetComplianceUrl();
                    break;
                case CompliancePageViewModel vm:
                    vm.BackUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetYourStatementUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetYourOrganisationUrl();
                    break;
                case YourOrganisationPageViewModel vm:
                    vm.BackUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetComplianceUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetPoliciesUrl();
                    break;
                case PoliciesPageViewModel vm:
                    vm.BackUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetYourOrganisationUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetRisksUrl();
                    break;
                case SupplyChainRisksPageViewModel vm:
                    vm.BackUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetPoliciesUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetDueDiligenceUrl();
                    break;
                case DueDiligencePageViewModel vm:
                    vm.BackUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetRisksUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetTrainingUrl();
                    break;
                case TrainingPageViewModel vm:
                    vm.BackUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetDueDiligenceUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetProgressUrl();
                    break;
                case MonitoringProgressPageViewModel vm:
                    vm.BackUrl = vm.CanRevertToOriginal ? GetReviewUrl() : GetTrainingUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = GetReviewUrl();
                    break;
                case ReviewAndEditPageViewModel vm:
                    vm.BackUrl = vm.CanRevertToOriginal ? null : GetProgressUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = GetReturnUrl();
                    vm.YourStatementUrl = GetYourStatementUrl();
                    vm.ComplianceUrl = GetComplianceUrl();
                    vm.OrganisationUrl = GetYourOrganisationUrl();
                    vm.PoliciesUrl = GetPoliciesUrl();
                    vm.RisksUrl = GetRisksUrl();
                    vm.DueDiligenceUrl = GetDueDiligenceUrl();
                    vm.TrainingUrl = GetTrainingUrl();
                    vm.ProgressUrl = GetProgressUrl();
                    break;
                case CancelPageViewModel vm:
                    var referrer = HttpContext.GetUrlReferrer()?.ToString();
                    vm.CancelUrl = vm.BackUrl = string.IsNullOrWhiteSpace(referrer) ? GetBackUrlFromCancellingViewModel() : referrer;
                    vm.ContinueUrl = GetReturnUrl();
                    break;
                case SubmissionCompleteViewModel vm:
                    vm.ContinueUrl = GetReturnUrl();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region Session Stack Methods
        private void StashCancellingViewModel<T>(T model)
        {
            if (model == null)
                Session.Remove(this + ":CancellingViewModel");
            else
                Session[this + ":CancellingViewModel"] = JsonConvert.SerializeObject(model, SubmissionPresenter.JsonSettings);
        }

        private T UnstashCancellingViewModel<T>(bool delete = false) where T : class
        {
            var json = Session[this + ":CancellingViewModel"].ToStringOrNull();
            var result = string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject<T>(json, SubmissionPresenter.JsonSettings);
            if (delete) Session.Remove(this + ":CancellingViewModel");

            return result;
        }

        private object UnstashCancellingViewModel(bool delete = false)
        {
            var json = Session[this + ":CancellingViewModel"].ToStringOrNull();
            var result = string.IsNullOrWhiteSpace(json) ? null : JsonConvert.DeserializeObject(json, SubmissionPresenter.JsonSettings);
            if (delete) Session.Remove(this + ":CancellingViewModel");
            return result;
        }
        #endregion

        #region Page Handling Methods
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
        
        private async Task<IActionResult> GetAsync<TViewModel>(string organisationIdentifier, int year) where TViewModel : BaseViewModel
        {
            //Try and get the viewmodel from session
            var viewModel = UnstashCancellingViewModel<TViewModel>(true);
            if (viewModel != null)
            {
                //Make sure we show any validation errors
                TryValidateModel(viewModel);
                return View(viewModel);
            }

            //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
            var viewModelResult = await SubmissionPresenter.GetViewModelAsync<TViewModel>(organisationIdentifier, year, VirtualUser.UserId);

            //Handle any StatementErrors
            if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

            //Get the view model and set the navigation urls
            viewModel = viewModelResult.Result;
            SetNavigationUrl(viewModel);

            //Otherwise return the view using the populated ViewModel
            return View(viewModel);
        }

        private async Task<IActionResult> PostAsync<TViewModel>(TViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command) where TViewModel : BaseViewModel
        {
            SetNavigationUrl(viewModel);

            switch (command)
            {
                case BaseViewModel.CommandType.Cancel:
                    //Save the viewmodel to session
                    StashCancellingViewModel(viewModel);

                    //Redirect to the cancel page
                    return Redirect(viewModel.CancelUrl);
                case BaseViewModel.CommandType.Continue:
                    //Validate the submitted ViewModel data
                    if (!ModelState.IsValid)
                    {
                        this.CleanModelErrors<TViewModel>();
                        return View(viewModel);
                    }

                    //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
                    var viewModelResult = await SubmissionPresenter.SaveViewModelAsync(viewModel, organisationIdentifier, year, VirtualUser.UserId);

                    //Handle any StatementErrors
                    if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

                    //Redirect to the continue url
                    return Redirect(viewModel.ContinueUrl);
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), $"CommandType {command} is not valid here");
            }
        }
        #endregion

        #region Before You Start
        [HttpGet("{organisationIdentifier}/{year}/before-you-start")]
        public async Task<IActionResult> BeforeYouStart(string organisationIdentifier, int year)
        {
            //Check if the current user can open edit draft statement data for this organisation, reporting year
            var openResult = await SubmissionPresenter.OpenDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);
            if (openResult.Fail) return HandleStatementErrors(openResult.Errors);

            //Show the correct view
            return View("BeforeYouStart",GetYourStatementUrl());
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
        public async Task<IActionResult> YourStatement(YourStatementPageViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
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
        public async Task<IActionResult> Compliance(CompliancePageViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }

        #endregion

        #region Step 3 - Your organisation

        [HttpGet("{organisationIdentifier}/{year}/your-organisation")]
        public async Task<IActionResult> YourOrganisation(string organisationIdentifier, int year)
        {
            return await GetAsync<YourOrganisationPageViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/your-organisation")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YourOrganisation(YourOrganisationPageViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
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
        public async Task<IActionResult> Policies(PoliciesPageViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }

        #endregion

        #region Step 5 - Supply chain risks and due diligence

        [HttpGet("{organisationIdentifier}/{year}/supply-chain-risks")]
        public async Task<IActionResult> SupplyChainRisks(string organisationIdentifier, int year)
        {
            return await GetAsync<SupplyChainRisksPageViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/supply-chain-risks")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupplyChainRisks(SupplyChainRisksPageViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
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
        public async Task<IActionResult> DueDiligence(DueDiligencePageViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
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
        public async Task<IActionResult> Training(TrainingPageViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }
        #endregion

        #region Step 8 - Monitoring progress

        [HttpGet("{organisationIdentifier}/{year}/monitoring-progress")]
        public async Task<IActionResult> MonitoringProgress(string organisationIdentifier, int year)
        {
            return await GetAsync<MonitoringProgressPageViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/monitoring-progress")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MonitoringProgress(MonitoringProgressPageViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }
        #endregion

        #region Step 9 - ReviewAndEdit

        [HttpGet("{organisationIdentifier}/{year}/review-statement")]
        public async Task<IActionResult> ReviewAndEdit(string organisationIdentifier, int year)
        {
            //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
            var viewModelResult = await SubmissionPresenter.OpenDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

            //Handle any StatementErrors
            if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

            //Create the view model
            var viewModel = await CreateReviewPageViewModelAsync(viewModelResult.Result);

            //set the navigation urls
            SetNavigationUrl(viewModel);

            //Otherwise return the view using the populated ViewModel
            return View(viewModel);
        }


        [HttpPost("{organisationIdentifier}/{year}/review-statement")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewAndEditPost(string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            //Create the view model
            var viewModel = new ReviewAndEditPageViewModel();

            switch (command)
            {
                case BaseViewModel.CommandType.Submit:
                    //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
                    var openResult = await SubmissionPresenter.OpenDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

                    //Handle any StatementErrors
                    if (openResult.Fail) return HandleStatementErrors(openResult.Errors);

                    //Create the view model
                    viewModel = await CreateReviewPageViewModelAsync(openResult.Result);

                    //set the navigation urls
                    SetNavigationUrl(viewModel);

                    //Validate the view model
                    TryValidateModel(viewModel);

                    //Validate the submitted ViewModel data
                    if (!ModelState.IsValid) return View("ReviewAndEdit", viewModel);

                    //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
                    var viewModelSubmitResult = await SubmissionPresenter.SubmitDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

                    //Handle any StatementErrors
                    if (viewModelSubmitResult.Fail) return HandleStatementErrors(openResult.Errors);

                    //Redirect to the continue url
                    return Redirect(viewModel.ContinueUrl);
                case BaseViewModel.CommandType.DiscardAndExit:
                    //set the navigation urls
                    SetNavigationUrl(viewModel);

                    //Close the draft and release the user lock
                    var cancelResult = await SubmissionPresenter.CancelDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

                    //Handle any StatementErrors
                    if (cancelResult.Fail) return HandleStatementErrors(cancelResult.Errors);

                    //Redirect to the continue url
                    return Redirect(viewModel.ContinueUrl);
                case BaseViewModel.CommandType.SaveAndExit:
                    //set the navigation urls
                    SetNavigationUrl(viewModel);

                    //Close the draft and release the user lock
                    var closeResult = await SubmissionPresenter.CloseDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

                    //Handle any StatementErrors
                    if (closeResult.Fail) return HandleStatementErrors(closeResult.Errors);

                    //Redirect to the continue url
                    return Redirect(viewModel.ContinueUrl);
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), $"CommandType {command} is not valid here");
            }
        }

        private async Task<ReviewAndEditPageViewModel> CreateReviewPageViewModelAsync(StatementModel statementModel)
        {
            //Create the view model
            var viewModel = SubmissionPresenter.GetViewModelFromStatementModel<ReviewAndEditPageViewModel>(statementModel);

            viewModel.YourStatement = SubmissionPresenter.GetViewModelFromStatementModel<YourStatementPageViewModel>(statementModel);
            viewModel.Compliance = SubmissionPresenter.GetViewModelFromStatementModel<CompliancePageViewModel>(statementModel);
            viewModel.Organisation = SubmissionPresenter.GetViewModelFromStatementModel<YourOrganisationPageViewModel>(statementModel);
            viewModel.Policies = SubmissionPresenter.GetViewModelFromStatementModel<PoliciesPageViewModel>(statementModel);
            viewModel.Risks = SubmissionPresenter.GetViewModelFromStatementModel<SupplyChainRisksPageViewModel>(statementModel);
            viewModel.DueDiligence = SubmissionPresenter.GetViewModelFromStatementModel<DueDiligencePageViewModel>(statementModel);
            viewModel.Training = SubmissionPresenter.GetViewModelFromStatementModel<TrainingPageViewModel>(statementModel);
            viewModel.Progress = SubmissionPresenter.GetViewModelFromStatementModel<MonitoringProgressPageViewModel>(statementModel);
            viewModel.Modifications=await SubmissionPresenter.GetDraftModifications(statementModel);

            //Otherwise return the view using the populated ViewModel
            return viewModel;
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

            //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
            var pageViewModel = UnstashCancellingViewModel();

            //Get the modifications
            var statementModel = SubmissionPresenter.SetViewModelToStatementModel(pageViewModel, viewModelResult.Result);
            viewModel.Modifications = await SubmissionPresenter.GetDraftModifications(statementModel);

            //Ensure the viewmodel is valid before saving
            ModelState.Clear();
            if (!TryValidateModel(pageViewModel))
            {
                viewModel.ErrorCount = ModelState.ErrorCount;
                return View(viewModel);
            }

            //Cancel immediately if no changes
            if (!viewModel.HasChanged())
            {
                //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
                var cancelModelResult = await SubmissionPresenter.CancelDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

                //Handle any StatementErrors
                if (cancelModelResult.Fail) return HandleStatementErrors(cancelModelResult.Errors);

                //Delete the stashed viewModel
                UnstashCancellingViewModel(true);

                //Redirect to the continue url
                return Redirect(viewModel.ContinueUrl);
            }

            //Otherwise return the view using the populated ViewModel
            return View(viewModel);
        }


        [HttpPost("{organisationIdentifier}/{year}/cancel-statement")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(CancelPageViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            //Get the cancel urls
            SetNavigationUrl(viewModel);

            Outcome<StatementErrors> viewModelResult = null;
            switch (command)
            {
                case BaseViewModel.CommandType.DiscardAndExit:
                    //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
                    viewModelResult = await SubmissionPresenter.CancelDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);
                    break;
                case BaseViewModel.CommandType.SaveAndExit:
                    //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
                    var pageViewModel = UnstashCancellingViewModel();

                    //Ensure the viewmodel is valid before saving
                    ModelState.Clear();
                    if (!TryValidateModel(pageViewModel))
                    {
                        viewModel.ErrorCount = ModelState.ErrorCount;

                        //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
                        var openModelResult = await SubmissionPresenter.OpenDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

                        //Handle any StatementErrors
                        if (openModelResult.Fail) return HandleStatementErrors(openModelResult.Errors);

                        //Get the modifications
                        var statementModel = SubmissionPresenter.SetViewModelToStatementModel(pageViewModel, openModelResult.Result);
                        viewModel.Modifications = await SubmissionPresenter.GetDraftModifications(statementModel);

                        //Return the errors
                        return View(viewModel);
                    }

                    //Save the viewmodel
                    switch (pageViewModel)
                    {
                        case YourStatementPageViewModel vm:
                            viewModelResult = await SubmissionPresenter.SaveViewModelAsync(vm, organisationIdentifier, year, VirtualUser.UserId);
                            break;
                        case CompliancePageViewModel vm:
                            viewModelResult = await SubmissionPresenter.SaveViewModelAsync(vm, organisationIdentifier, year, VirtualUser.UserId);
                            break;
                        case YourOrganisationPageViewModel vm:
                            viewModelResult = await SubmissionPresenter.SaveViewModelAsync(vm, organisationIdentifier, year, VirtualUser.UserId);
                            break;
                        case PoliciesPageViewModel vm:
                            viewModelResult = await SubmissionPresenter.SaveViewModelAsync(vm, organisationIdentifier, year, VirtualUser.UserId);
                            break;
                        case SupplyChainRisksPageViewModel vm:
                            viewModelResult = await SubmissionPresenter.SaveViewModelAsync(vm, organisationIdentifier, year, VirtualUser.UserId);
                            break;
                        case DueDiligencePageViewModel vm:
                            viewModelResult = await SubmissionPresenter.SaveViewModelAsync(vm, organisationIdentifier, year, VirtualUser.UserId);
                            break;
                        case TrainingPageViewModel vm:
                            viewModelResult = await SubmissionPresenter.SaveViewModelAsync(vm, organisationIdentifier, year, VirtualUser.UserId);
                            break;
                        case MonitoringProgressPageViewModel vm:
                            viewModelResult = await SubmissionPresenter.SaveViewModelAsync(vm, organisationIdentifier, year, VirtualUser.UserId);
                            break;
                    }
                    //Handle any StatementErrors
                    if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

                    //Close the draft and release the user lock
                    viewModelResult = await SubmissionPresenter.CloseDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), $"CommandType {command} is not valid here");
            }

            //Handle any StatementErrors
            if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

            //Delete the stashed viewModel
            UnstashCancellingViewModel(true);

            //Redirect to the continue url
            return Redirect(viewModel.ContinueUrl);
        }

        #endregion

        #region SubmissionComplete
        [HttpGet("{organisationIdentifier}/{year}/submission-complete")]
        public async Task<IActionResult> SubmissionComplete(string organisationIdentifier, int year)
        {
            return await GetAsync<SubmissionCompleteViewModel>(organisationIdentifier, year);
        }

        #endregion
    }
}
