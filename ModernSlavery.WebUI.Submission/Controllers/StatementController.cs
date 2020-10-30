using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
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
        private string GetOrganisationIdentifier() => RouteData.Values["OrganisationIdentifier"].ToString();
        private int GetReportingDeadlineYear() => RouteData.Values["Year"].ToInt32();
        private object GetOrgAndYearRouteData() => new { OrganisationIdentifier = GetOrganisationIdentifier(), year = GetReportingDeadlineYear() };

        private string GetReturnUrl() => Url.Action("ManageOrganisation", "Submission", new { organisationIdentifier = GetOrganisationIdentifier() });

        private string GetGroupStatusUrl() => Url.Action(nameof(GroupStatus), GetOrgAndYearRouteData());
        private string GetGroupSearchUrl() => Url.Action(nameof(GroupSearch), GetOrgAndYearRouteData());
        private string GetGroupReviewUrl() => Url.Action(nameof(GroupReview), GetOrgAndYearRouteData());

        private string GetYourStatementUrl() => Url.Action(nameof(YourStatement), GetOrgAndYearRouteData());

        private string GetComplianceUrl() => Url.Action(nameof(Compliance), GetOrgAndYearRouteData());

        private string GetYourOrganisationUrl() => Url.Action(nameof(YourOrganisation), GetOrgAndYearRouteData());

        private string GetPoliciesUrl() => Url.Action(nameof(Policies), GetOrgAndYearRouteData());

        private string GetRisksUrl() => Url.Action(nameof(SupplyChainRisks), GetOrgAndYearRouteData());

        private string GetDueDiligenceUrl() => Url.Action(nameof(DueDiligence), GetOrgAndYearRouteData());

        private string GetTrainingUrl() => Url.Action(nameof(Training), GetOrgAndYearRouteData());

        private string GetProgressUrl() => Url.Action(nameof(MonitoringProgress), GetOrgAndYearRouteData());

        private string GetReviewUrl() => Url.Action(nameof(Review), GetOrgAndYearRouteData());
        private string GetCompleteUrl() => Url.Action(nameof(SubmissionComplete), GetOrgAndYearRouteData());

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
                case YourStatementViewModel vm:
                    return GetYourStatementUrl();
                case ComplianceViewModel vm:
                    return GetComplianceUrl();
                case YourOrganisationViewModel vm:
                    return GetYourOrganisationUrl();
                case PoliciesViewModel vm:
                    return GetPoliciesUrl();
                case SupplyChainRisksViewModel vm:
                    return GetRisksUrl();
                case DueDiligenceViewModel vm:
                    return GetDueDiligenceUrl();
                case TrainingViewModel vm:
                    return GetTrainingUrl();
                case ProgressViewModel vm:
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
                case GroupStatusViewModel vm:
                    vm.BackUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetReturnUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.ReturnToReviewPage ? GetReviewUrl() : vm.GroupSubmission == true ? GetGroupSearchUrl() : GetYourStatementUrl();
                    break;
                case GroupSearchViewModel vm:
                    vm.BackUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetGroupStatusUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetGroupReviewUrl();
                    vm.GroupReviewUrl = GetGroupReviewUrl();
                    break;
                case GroupReviewViewModel vm:
                    vm.BackUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetGroupSearchUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetYourStatementUrl();
                    vm.GroupStatusUrl = GetGroupStatusUrl();
                    vm.GroupSearchUrl = GetGroupSearchUrl();
                    break;
                case YourStatementViewModel vm:
                    vm.BackUrl = vm.ReturnToReviewPage ? GetReviewUrl() : vm.GroupSubmission == true ? GetGroupReviewUrl() : GetGroupStatusUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetComplianceUrl();
                    break;
                case ComplianceViewModel vm:
                    vm.BackUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetYourStatementUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetYourOrganisationUrl();
                    break;
                case YourOrganisationViewModel vm:
                    vm.BackUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetComplianceUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetPoliciesUrl();
                    break;
                case PoliciesViewModel vm:
                    vm.BackUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetYourOrganisationUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetRisksUrl();
                    break;
                case SupplyChainRisksViewModel vm:
                    vm.BackUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetPoliciesUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetDueDiligenceUrl();
                    break;
                case DueDiligenceViewModel vm:
                    vm.BackUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetRisksUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetTrainingUrl();
                    break;
                case TrainingViewModel vm:
                    vm.BackUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetDueDiligenceUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetProgressUrl();
                    break;
                case ProgressViewModel vm:
                    vm.BackUrl = vm.ReturnToReviewPage ? GetReviewUrl() : GetTrainingUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = GetReviewUrl();
                    break;
                case ReviewViewModel vm:
                    vm.BackUrl = vm.ReturnToReviewPage ? null : GetProgressUrl();
                    vm.CancelUrl = GetCancelUrl();
                    vm.ContinueUrl = GetReturnUrl();
                    vm.GroupStatusUrl = GetGroupStatusUrl();
                    vm.GroupReviewUrl = GetGroupReviewUrl();
                    vm.YourStatementUrl = GetYourStatementUrl();
                    vm.YourStatementUrl = GetYourStatementUrl();
                    vm.ComplianceUrl = GetComplianceUrl();
                    vm.OrganisationUrl = GetYourOrganisationUrl();
                    vm.PoliciesUrl = GetPoliciesUrl();
                    vm.RisksUrl = GetRisksUrl();
                    vm.DueDiligenceUrl = GetDueDiligenceUrl();
                    vm.TrainingUrl = GetTrainingUrl();
                    vm.ProgressUrl = GetProgressUrl();
                    break;
                case CancelViewModel vm:
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

        #region Session Stash Methods
        private void StashCancellingViewModel<TModel>(TModel model)
        {
            if (model == null)
                Session.Remove(this + ":CancellingViewModel");
            else
                Session[this + ":CancellingViewModel"] = JsonConvert.SerializeObject(model, SubmissionPresenter.JsonSettings);
        }

        private TModel UnstashCancellingViewModel<TModel>() where TModel : class
        {
            var json = Session[this + ":CancellingViewModel"].ToStringOrNull();
            Session.Remove(this + ":CancellingViewModel");
            if (!string.IsNullOrWhiteSpace(json))
            {
                var value = JsonConvert.DeserializeObject(json, SubmissionPresenter.JsonSettings);
                if (value is TModel result) return result;
            }
            return default;
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
        private IActionResult HandleStatementErrors(IEnumerable<(StatementErrors Error, string Message)> errors, object viewModel = null)
        {
            //Return full page errors which return to the ManageOrganisation page
            var error = errors.FirstOrDefault();

            //Clear the cancel stash before leaving journey
            UnstashCancellingViewModel(true);

            switch (error.Error)
            {
                case StatementErrors.NotFound:
                    return new HttpNotFoundResult(error.Message);
                case StatementErrors.Unauthorised:
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1153, new { organisationIdentifier = GetOrganisationIdentifier() }));
                case StatementErrors.Locked:
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1154, new { organisationIdentifier = GetOrganisationIdentifier() }));
                case StatementErrors.TooLate:
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(1155, new { organisationIdentifier = GetOrganisationIdentifier() }));
                case StatementErrors.DuplicateName:
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(3901, new { organisationName = error.Message }));
                case StatementErrors.CoHoTransientError:
                    if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
                    ModelState.AddModelError(1141);
                    return View(viewModel);
                case StatementErrors.CoHoPermanentError:
                    return View("CustomError", WebService.ErrorViewModelFactory.Create(3905));
                default:
                    throw new NotImplementedException($"{nameof(StatementErrors)} type '{error.Error}' is not recognised");
            }
        }

        private async Task<IActionResult> GetAsync<TViewModel>(string organisationIdentifier, int year, params object[] arguments) where TViewModel : BaseViewModel
        {
            //Try and get the viewmodel from session
            var viewModel = UnstashCancellingViewModel<TViewModel>();
            if (viewModel != null)
            {
                //Make sure we show any validation errors
                TryValidateModel(viewModel);
                //Dont validate the group search or organisation name
                if (viewModel is GroupSearchViewModel) ModelState.Exclude($"{nameof(GroupSearchViewModel.GroupResults)}.{nameof(GroupSearchViewModel.GroupResults.SearchKeywords)}", $"{nameof(GroupSearchViewModel.GroupResults)}.{nameof(GroupSearchViewModel.GroupResults.OrganisationName)}");
                return View(viewModel);
            }
            
            //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
            var viewModelResult = await SubmissionPresenter.GetViewModelAsync<TViewModel>(organisationIdentifier, year, VirtualUser.UserId, arguments);

            //Handle any StatementErrors
            if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);
            viewModel = viewModelResult.Result;

            //Get the view model and set the navigation urls
            SetNavigationUrl(viewModel);

            if (viewModel is GroupSearchViewModel)
            {
                var searchViewModel = viewModel as GroupSearchViewModel;
                //Copy changes to the review model
                var reviewViewModel = UnstashModel<GroupReviewViewModel>();
                if (reviewViewModel != null) searchViewModel.StatementOrganisations = reviewViewModel.StatementOrganisations;

                //Copy the previous search results
                var stashedViewModel = UnstashModel<GroupSearchViewModel>();
                if (stashedViewModel != null) searchViewModel.GroupResults = stashedViewModel.GroupResults;

                //Populate the extra submission info
                await SubmissionPresenter.GetOtherSubmissionsInformationAsync(searchViewModel, GetReportingDeadlineYear());

                //Add the group search model to session
                StashModel(searchViewModel);
            }
            else if (viewModel is GroupReviewViewModel)
            {
                var reviewViewModel = viewModel as GroupReviewViewModel;

                //Copy changes to the review model
                var searchViewModel = UnstashModel<GroupSearchViewModel>();
                if (searchViewModel != null) reviewViewModel.StatementOrganisations = searchViewModel.StatementOrganisations;

                //Populate the extra submission info
                await SubmissionPresenter.GetOtherSubmissionsInformationAsync(reviewViewModel, GetReportingDeadlineYear());

                //Add the group search model to session
                StashModel(reviewViewModel);
            }
            else
            {
                //Remove the group search model from session
                UnstashModel<GroupSearchViewModel>(true);

                //Remove the group review model from session
                UnstashModel<GroupReviewViewModel>(true);
            }

            //Otherwise return the view using the populated ViewModel
            return View(viewModel);
        }

        private async Task<IActionResult> PostAsync<TViewModel>(TViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command,, params object[] arguments) where TViewModel : BaseViewModel
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
                        this.SetModelCustomErrors(viewModel);
                        return View(viewModel);
                    }

                    //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
                    var viewModelResult = await SubmissionPresenter.SaveViewModelAsync(viewModel, organisationIdentifier, year, VirtualUser.UserId);

                    //Handle any StatementErrors
                    if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

                    //Remove the group search from session
                    UnstashModel<GroupSearchViewModel>(true);

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
            return View("BeforeYouStart", new BeforeYouStartViewModel { BackUrl = GetReturnUrl(), ContinueUrl = GetGroupStatusUrl() });
        }
        #endregion

        #region Before You Start
        [HttpGet("{organisationIdentifier}/{year}/transparency")]
        public async Task<IActionResult> Transparency(string organisationIdentifier, int year)
        {
            //Check if the current user can open edit draft statement data for this organisation, reporting year
            var openResult = await SubmissionPresenter.OpenDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);
            if (openResult.Fail) return HandleStatementErrors(openResult.Errors);

            //Show the correct view
            return View("Transparency", new TransparencyViewModel { BackUrl = GetReturnUrl(), ContinueUrl = GetGroupStatusUrl() });
        }
        #endregion

        #region Review

        [HttpGet("{organisationIdentifier}/{year}/review-statement")]
        public async Task<IActionResult> Review(string organisationIdentifier, int year)
        {
            //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
            var viewModelResult = await SubmissionPresenter.OpenDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

            //Handle any StatementErrors
            if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

            var statementModel = viewModelResult.Result;

            //Create the view model
            var viewModel = await CreateReviewViewModelAsync(statementModel);

            //Set the flag so we now always return to the review page when we have some content
            if (!viewModel.ReturnToReviewPage && !statementModel.IsEmpty())
            {
                statementModel.ReturnToReviewPage = true;
                await SubmissionPresenter.SaveStatementModelAsync(statementModel);
                viewModel.ReturnToReviewPage = true;
            }

            //set the navigation urls
            SetNavigationUrl(viewModel);

            //Otherwise return the view using the populated ViewModel
            return View(viewModel);
        }


        [HttpPost("{organisationIdentifier}/{year}/review-statement")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewPost(string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            //Create the view model
            var viewModel = new ReviewViewModel();

            switch (command)
            {
                case BaseViewModel.CommandType.Submit:
                    //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
                    var openResult = await SubmissionPresenter.OpenDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

                    //Handle any StatementErrors
                    if (openResult.Fail) return HandleStatementErrors(openResult.Errors);

                    var statementModel = openResult.Result;

                    //Create the view model
                    viewModel = await CreateReviewViewModelAsync(statementModel);

                    //set the navigation urls
                    SetNavigationUrl(viewModel);

                    //Validate the view model
                    TryValidateModel(viewModel);

                    //Validate the submitted ViewModel data
                    if (!ModelState.IsValid) return View("Review", viewModel);

                    //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
                    var viewModelSubmitResult = await SubmissionPresenter.SubmitDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

                    //Handle any StatementErrors
                    if (viewModelSubmitResult.Fail) return HandleStatementErrors(openResult.Errors);

                    //Stash the viewmodel for the complete page
                    var submissionCompleteViewModel = SubmissionPresenter.GetViewModelFromStatementModel<SubmissionCompleteViewModel>(statementModel);
                    StashModel(submissionCompleteViewModel);

                    //Redirect to the continue url
                    return Redirect(GetCompleteUrl());
                case BaseViewModel.CommandType.ExitNoChanges:
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

        private async Task<ReviewViewModel> CreateReviewViewModelAsync(StatementModel statementModel)
        {
            //Create the view model
            var viewModel = SubmissionPresenter.GetViewModelFromStatementModel<ReviewViewModel>(statementModel);

            viewModel.GroupOrganisationsPages = SubmissionPresenter.GetViewModelFromStatementModel<GroupOrganisationsViewModel>(statementModel);
            viewModel.YourStatement = SubmissionPresenter.GetViewModelFromStatementModel<YourStatementViewModel>(statementModel);
            viewModel.Compliance = SubmissionPresenter.GetViewModelFromStatementModel<ComplianceViewModel>(statementModel);
            viewModel.Organisation = SubmissionPresenter.GetViewModelFromStatementModel<YourOrganisationViewModel>(statementModel);
            viewModel.Policies = SubmissionPresenter.GetViewModelFromStatementModel<PoliciesViewModel>(statementModel);
            viewModel.Risks = SubmissionPresenter.GetViewModelFromStatementModel<SupplyChainRisksViewModel>(statementModel);
            viewModel.DueDiligence = SubmissionPresenter.GetViewModelFromStatementModel<DueDiligenceViewModel>(statementModel);
            viewModel.Training = SubmissionPresenter.GetViewModelFromStatementModel<TrainingViewModel>(statementModel);
            viewModel.Progress = SubmissionPresenter.GetViewModelFromStatementModel<ProgressViewModel>(statementModel);
            viewModel.DraftModifications = await SubmissionPresenter.CompareToDraftBackupStatement(statementModel);
            viewModel.SubmittedModifications = await SubmissionPresenter.CompareToSubmittedStatement(statementModel);

            //Otherwise return the view using the populated ViewModel
            return viewModel;
        }

        #endregion

        #region Basic Information

        #region Group Submission

        [HttpGet("{organisationIdentifier}/{year}/grouping")]
        public async Task<IActionResult> GroupStatus(string organisationIdentifier, int year)
        {
            return await GetAsync<GroupStatusViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/grouping")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GroupStatus(GroupStatusViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }

        [HttpGet("{organisationIdentifier}/{year}/group-search")]
        public async Task<IActionResult> GroupSearch(string organisationIdentifier, int year)
        {
            return await GetAsync<GroupSearchViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/group-search")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GroupSearch(GroupSearchViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command, int addIndex = -1, int removeIndex = -1)
        {
            //Get the saved viewmodel information from session
            var stashedModel = UnstashModel<GroupSearchViewModel>();

            //Continue or cancel normally
            if (command.IsAny(BaseViewModel.CommandType.Continue, BaseViewModel.CommandType.Cancel))
            {
                viewModel = stashedModel;
                //Must clear otherwise hidden for doesnt work
                ModelState.Clear();
                return await PostAsync(viewModel, organisationIdentifier, year, command);
            };

            //Rebuild the search model from the postback
            if (stashedModel.GroupResults.ShowResults)
            {
                //Get the search keywords
                stashedModel.GroupResults.SearchKeywords = viewModel.GroupResults.SearchKeywords;
            }
            else
            {
                //Get the organisation name if it's there
                if (viewModel.GroupResults.OrganisationName != null)
                    stashedModel.GroupResults.OrganisationName = viewModel.GroupResults.OrganisationName;
                //else get the search keywords
                else
                    stashedModel.GroupResults.SearchKeywords = viewModel.GroupResults.SearchKeywords;
            }
            viewModel = stashedModel;

            ModelState.Exclude(nameof(viewModel.ReturnToReviewPage), nameof(viewModel.Submitted));

            if (command.IsAny(BaseViewModel.CommandType.Search, BaseViewModel.CommandType.SearchNext, BaseViewModel.CommandType.SearchPrevious))
            {
                //Dont validate the organisation name
                ModelState.Exclude($"{nameof(viewModel.GroupResults)}.{nameof(viewModel.GroupResults.OrganisationName)}");

                //Check the search string
                if (!ModelState.IsValid)
                {
                    this.SetModelCustomErrors(viewModel);
                    return View(viewModel);
                }

                if (command == BaseViewModel.CommandType.Search)
                    viewModel.GroupResults.ResultsPage.CurrentPage = 1;
                else if (command == BaseViewModel.CommandType.SearchNext)
                    viewModel.GroupResults.ResultsPage.CurrentPage++;
                else if (command == BaseViewModel.CommandType.SearchPrevious)
                    viewModel.GroupResults.ResultsPage.CurrentPage--;


                var outcome = await SubmissionPresenter.SearchGroupOrganisationsAsync(viewModel, VirtualUser);
                if (!outcome.Success) HandleStatementErrors(outcome.Errors, viewModel);

                //Show the search results
                viewModel.GroupResults.ShowResults = true;
            }
            else if (command == BaseViewModel.CommandType.ToggleResults)
            {
                //Toggle the view
                viewModel.GroupResults.ShowResults = !viewModel.GroupResults.ShowResults;

                //Copy the search text to the organisation name
                if (!viewModel.GroupResults.ShowResults && !string.IsNullOrWhiteSpace(viewModel.GroupResults.SearchKeywords))
                {
                    viewModel.GroupResults.OrganisationName = viewModel.GroupResults.SearchKeywords;

                    //Add the group search model to session before removing SearchKeywords
                    StashModel(viewModel);

                    // remove text from searchbox
                    viewModel.GroupResults.SearchKeywords = null;

                    //Must clear otherwise hidden for doesnt work
                    ModelState.Clear();

                    //Return the page
                    return View(viewModel);
                }

            }
            else if (addIndex > -1 || removeIndex > -1)
            {
                if (addIndex > -1)
                {
                    if (!viewModel.GroupResults.ShowResults)
                    {
                        //Dont validate the search string
                        ModelState.Exclude($"{nameof(viewModel.GroupResults)}.{nameof(viewModel.GroupResults.SearchKeywords)}");

                        //Check the search string
                        if (!ModelState.IsValid)
                        {
                            this.SetModelCustomErrors(viewModel);
                            return View(viewModel);
                        }
                    }

                    //Add the organisation to the view model
                    var outcome = await SubmissionPresenter.IncludeGroupOrganisationAsync(viewModel, addIndex);

                    //Handle any errors
                    if (!outcome.Success) HandleStatementErrors(outcome.Errors);

                    //Clear the input name 
                    viewModel.GroupResults.OrganisationName = null;
                    //Clear searchkeywords if manually added an org
                    if (!viewModel.GroupResults.ShowResults)
                        viewModel.GroupResults.SearchKeywords = null;

                }
                else
                {
                    //removing from manual orgs table
                    if (!viewModel.GroupResults.ShowResults)
                    {
                        //Remove the selected organisation
                        viewModel.StatementOrganisations.RemoveAt(removeIndex);
                        //keep search keywords empty                       
                        viewModel.GroupResults.SearchKeywords = null;

                    }
                    else
                    {
                        //Add the organisation to the view model
                        var removeOrg = viewModel.GroupResults.ResultsPage.Results[removeIndex];
                        var index = viewModel.FindGroupOrganisation(removeOrg);
                        viewModel.StatementOrganisations.RemoveAt(index);
                    }
                }

                //showBanner when including/removing organisation
                viewModel.ShowBanner = true;

                //Populate the extra submission info
                await SubmissionPresenter.GetOtherSubmissionsInformationAsync(viewModel, GetReportingDeadlineYear());

                //Copy changes to the search review model
                var reviewViewModel = UnstashModel<GroupReviewViewModel>();
                if (reviewViewModel != null)
                {
                    reviewViewModel.StatementOrganisations = viewModel.StatementOrganisations;
                    StashModel(reviewViewModel);
                }
            }
            else
                throw new NotImplementedException();

            //Add the group search model to session
            StashModel(viewModel);

            //Must clear otherwise hidden for doesnt work
            ModelState.Clear();

            //Return the page
            return View(viewModel);
        }

        [HttpGet("{organisationIdentifier}/{year}/group-add")]
        public async Task<IActionResult> GroupAdd(string organisationIdentifier, int year)
        {
            return await GetAsync<GroupSearchViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/group-add")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GroupAdd(GroupSearchViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command, int addIndex = -1, int removeIndex = -1)
        {
            //Get the saved viewmodel information from session
            var stashedModel = UnstashModel<GroupSearchViewModel>();

            //Continue or cancel normally
            if (command.IsAny(BaseViewModel.CommandType.Continue, BaseViewModel.CommandType.Cancel))
            {
                viewModel = stashedModel;
                //Must clear otherwise hidden for doesnt work
                ModelState.Clear();
                return await PostAsync(viewModel, organisationIdentifier, year, command);
            };

            //Rebuild the search model from the postback
            if (stashedModel.GroupResults.ShowResults)
            {
                //Get the search keywords
                stashedModel.GroupResults.SearchKeywords = viewModel.GroupResults.SearchKeywords;
            }
            else
            {
                //Get the organisation name if it's there
                if (viewModel.GroupResults.OrganisationName != null)
                    stashedModel.GroupResults.OrganisationName = viewModel.GroupResults.OrganisationName;
                //else get the search keywords
                else
                    stashedModel.GroupResults.SearchKeywords = viewModel.GroupResults.SearchKeywords;
            }
            viewModel = stashedModel;

            ModelState.Exclude(nameof(viewModel.ReturnToReviewPage), nameof(viewModel.Submitted));

            if (command.IsAny(BaseViewModel.CommandType.Search, BaseViewModel.CommandType.SearchNext, BaseViewModel.CommandType.SearchPrevious))
            {
                //Dont validate the organisation name
                ModelState.Exclude($"{nameof(viewModel.GroupResults)}.{nameof(viewModel.GroupResults.OrganisationName)}");

                //Check the search string
                if (!ModelState.IsValid)
                {
                    this.SetModelCustomErrors(viewModel);
                    return View(viewModel);
                }

                if (command == BaseViewModel.CommandType.Search)
                    viewModel.GroupResults.ResultsPage.CurrentPage = 1;
                else if (command == BaseViewModel.CommandType.SearchNext)
                    viewModel.GroupResults.ResultsPage.CurrentPage++;
                else if (command == BaseViewModel.CommandType.SearchPrevious)
                    viewModel.GroupResults.ResultsPage.CurrentPage--;


                var outcome = await SubmissionPresenter.SearchGroupOrganisationsAsync(viewModel, VirtualUser);
                if (!outcome.Success) HandleStatementErrors(outcome.Errors, viewModel);

                //Show the search results
                viewModel.GroupResults.ShowResults = true;
            }
            else if (command == BaseViewModel.CommandType.ToggleResults)
            {
                //Toggle the view
                viewModel.GroupResults.ShowResults = !viewModel.GroupResults.ShowResults;

                //Copy the search text to the organisation name
                if (!viewModel.GroupResults.ShowResults && !string.IsNullOrWhiteSpace(viewModel.GroupResults.SearchKeywords))
                {
                    viewModel.GroupResults.OrganisationName = viewModel.GroupResults.SearchKeywords;

                    //Add the group search model to session before removing SearchKeywords
                    StashModel(viewModel);

                    // remove text from searchbox
                    viewModel.GroupResults.SearchKeywords = null;

                    //Must clear otherwise hidden for doesnt work
                    ModelState.Clear();

                    //Return the page
                    return View(viewModel);
                }

            }
            else if (addIndex > -1 || removeIndex > -1)
            {
                if (addIndex > -1)
                {
                    if (!viewModel.GroupResults.ShowResults)
                    {
                        //Dont validate the search string
                        ModelState.Exclude($"{nameof(viewModel.GroupResults)}.{nameof(viewModel.GroupResults.SearchKeywords)}");

                        //Check the search string
                        if (!ModelState.IsValid)
                        {
                            this.SetModelCustomErrors(viewModel);
                            return View(viewModel);
                        }
                    }

                    //Add the organisation to the view model
                    var outcome = await SubmissionPresenter.IncludeGroupOrganisationAsync(viewModel, addIndex);

                    //Handle any errors
                    if (!outcome.Success) HandleStatementErrors(outcome.Errors);

                    //Clear the input name 
                    viewModel.GroupResults.OrganisationName = null;
                    //Clear searchkeywords if manually added an org
                    if (!viewModel.GroupResults.ShowResults)
                        viewModel.GroupResults.SearchKeywords = null;

                }
                else
                {
                    //removing from manual orgs table
                    if (!viewModel.GroupResults.ShowResults)
                    {
                        //Remove the selected organisation
                        viewModel.StatementOrganisations.RemoveAt(removeIndex);
                        //keep search keywords empty                       
                        viewModel.GroupResults.SearchKeywords = null;

                    }
                    else
                    {
                        //Add the organisation to the view model
                        var removeOrg = viewModel.GroupResults.ResultsPage.Results[removeIndex];
                        var index = viewModel.FindGroupOrganisation(removeOrg);
                        viewModel.StatementOrganisations.RemoveAt(index);
                    }
                }

                //showBanner when including/removing organisation
                viewModel.ShowBanner = true;

                //Populate the extra submission info
                await SubmissionPresenter.GetOtherSubmissionsInformationAsync(viewModel, GetReportingDeadlineYear());

                //Copy changes to the search review model
                var reviewViewModel = UnstashModel<GroupReviewViewModel>();
                if (reviewViewModel != null)
                {
                    reviewViewModel.StatementOrganisations = viewModel.StatementOrganisations;
                    StashModel(reviewViewModel);
                }
            }
            else
                throw new NotImplementedException();

            //Add the group search model to session
            StashModel(viewModel);

            //Must clear otherwise hidden for doesnt work
            ModelState.Clear();

            //Return the page
            return View(viewModel);
        }

        [HttpGet("{organisationIdentifier}/{year}/group-review")]
        public async Task<IActionResult> GroupReview(string organisationIdentifier, int year)
        {
            return await GetAsync<GroupReviewViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/group-review")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GroupReview(string organisationIdentifier, int year, BaseViewModel.CommandType command, int removeIndex = -1)
        {
            //Get the saved viewmodel information from session
            var viewModel = UnstashModel<GroupReviewViewModel>();

            if (command.IsAny(BaseViewModel.CommandType.Continue, BaseViewModel.CommandType.Cancel))
            {
                //Continue or cancel normally
                return await PostAsync(viewModel, organisationIdentifier, year, command);
            }
            else if (removeIndex > -1)
            {
                //Remove the selected organisation
                viewModel.StatementOrganisations.RemoveAt(removeIndex);

                //Copy changes to the search view model
                var searchViewModel = UnstashModel<GroupSearchViewModel>();
                if (searchViewModel != null)
                {
                    searchViewModel.StatementOrganisations = viewModel.StatementOrganisations;
                    StashModel(searchViewModel);
                }
            }
            else
                throw new NotImplementedException();

            //Add the group search model to session
            StashModel(viewModel);

            //Must clear otherwise hidden for doesnt work
            ModelState.Clear();

            //Return the page
            return View(viewModel);
        }

        #endregion

        #region URL && SignOff

        [HttpGet("{organisationIdentifier}/{year}/url-email")]
        public async Task<IActionResult> UrlEmail(string organisationIdentifier, int year)
        {
            return await GetAsync<UrlEmailViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/url-email")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UrlEmail(UrlEmailViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }


        [HttpGet("{organisationIdentifier}/{year}/period-covered")]
        public async Task<IActionResult> Period(string organisationIdentifier, int year)
        {
            return await GetAsync<PeriodCoveredViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/period-covered")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Period(PeriodCoveredViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }

        [HttpGet("{organisationIdentifier}/{year}/sign-off")]
        public async Task<IActionResult> SignOff(string organisationIdentifier, int year)
        {
            return await GetAsync<SignOffViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/sign-off")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOff(SignOffViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }

        #endregion

        #region Compliance

        [HttpGet("{organisationIdentifier}/{year}/compliance")]
        public async Task<IActionResult> Compliance(string organisationIdentifier, int year)
        {
            return await GetAsync<ComplianceViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/compliance")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Compliance(ComplianceViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }

        #endregion

        #region Sector & Turnover
        [HttpGet("{organisationIdentifier}/{year}/sectors")]
        public async Task<IActionResult> Sectors(string organisationIdentifier, int year)
        {
            return await GetAsync<SectorsViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/sectors")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sectors(SectorsViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }

        [HttpGet("{organisationIdentifier}/{year}/turnover")]
        public async Task<IActionResult> Turnover(string organisationIdentifier, int year)
        {
            return await GetAsync<TurnoverViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/turnover")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Turnover(TurnoverViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }
        #endregion

        #region StatementYears
        [HttpGet("{organisationIdentifier}/{year}/years")]
        public async Task<IActionResult> Years(string organisationIdentifier, int year)
        {
            return await GetAsync<YearsViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/years")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Years(YearsViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }
        #endregion

        #endregion

        #region Statement Summary

        #region Policies
        [HttpGet("{organisationIdentifier}/{year}/policies")]
        public async Task<IActionResult> Policies(string organisationIdentifier, int year)
        {
            return await GetAsync<PoliciesViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/policies")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Policies(PoliciesViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }
        #endregion

        #region Training
        [HttpGet("{organisationIdentifier}/{year}/training")]
        public async Task<IActionResult> Training(string organisationIdentifier, int year)
        {
            return await GetAsync<TrainingViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/training")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Training(TrainingViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }
        #endregion

        #region Working Conditions
        [HttpGet("{organisationIdentifier}/{year}/partners")]
        public async Task<IActionResult> Partners(string organisationIdentifier, int year)
        {
            return await GetAsync<PartnersViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/partners")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Partners(PartnersViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }

        [HttpGet("{organisationIdentifier}/{year}/social-audits")]
        public async Task<IActionResult> SocialAudits(string organisationIdentifier, int year)
        {
            return await GetAsync<SocialAuditsViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/social-audits")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SocialAudits(SocialAuditsViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }

        [HttpGet("{organisationIdentifier}/{year}/grievance-mechanisms")]
        public async Task<IActionResult> Grievances(string organisationIdentifier, int year)
        {
            return await GetAsync<GrievancesViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/grievance-mechanisms")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grievances(GrievancesViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }

        [HttpGet("{organisationIdentifier}/{year}/other-monitoring")]
        public async Task<IActionResult> Monitoring(string organisationIdentifier, int year)
        {
            return await GetAsync<MonitoringViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/other-monitoring")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Monitoring(MonitoringViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }
        #endregion

        #region Highest Risks
        [HttpGet("{organisationIdentifier}/{year}/highest-risks")]
        public async Task<IActionResult> HighestRisks(string organisationIdentifier, int year)
        {
            return await GetAsync<HighestRisksViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/highest-risks")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HighestRisks(HighestRisksViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }

        [HttpGet("{organisationIdentifier}/{year}/highest-risk/{index}")]
        public async Task<IActionResult> HighRisk(string organisationIdentifier, int year, int index)
        {
            return await GetAsync<HighRiskViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/highest-risk/{index}")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HighRisk(HighRiskViewModel viewModel, string organisationIdentifier, int year, int index, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }

        #endregion

        #region Indications and Remediations
        [HttpGet("{organisationIdentifier}/{year}/indicators")]
        public async Task<IActionResult> Indicators(string organisationIdentifier, int year)
        {
            return await GetAsync<IndicatorsViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/indicators")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Indicators(IndicatorsViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }

        [HttpGet("{organisationIdentifier}/{year}/remediations")]
        public async Task<IActionResult> Remediations(string organisationIdentifier, int year)
        {
            return await GetAsync<RemediationsViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/remediations")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remediations(RemediationsViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }
        #endregion

        #region Monitoring Progress
        [HttpGet("{organisationIdentifier}/{year}/Progress")]
        public async Task<IActionResult> Progress(string organisationIdentifier, int year)
        {
            return await GetAsync<ProgressViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/monitoring-progress")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Progress(ProgressViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command);
        }
        #endregion

        #endregion

        #region SubmissionComplete
        [HttpGet("{organisationIdentifier}/{year}/submission-complete")]
        public async Task<IActionResult> SubmissionComplete(string organisationIdentifier, int year)
        {
            var viewModel = UnstashModel<SubmissionCompleteViewModel>(true);
            if (viewModel == null) throw new Exception("Could not unstash SubmissionCompleteViewModel");

            SetNavigationUrl(viewModel);

            //Otherwise return the view using the populated ViewModel
            return View(viewModel);
        }

        #endregion
    }
}
