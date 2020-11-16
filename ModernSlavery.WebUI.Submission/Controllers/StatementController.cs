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

//TODO: Ensure we use PRG model on all POST actions so the 'Browser' back button will work correctly on all pages in this journey.

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

        #region Url Methods
        private string GetOrganisationIdentifier() => RouteData.Values["OrganisationIdentifier"].ToString();
        private int GetReportingDeadlineYear() => RouteData.Values["Year"].ToInt32();
        private object GetOrgAndYearRouteData(object routeValues = null)
        {
            var pars = new { OrganisationIdentifier = GetOrganisationIdentifier(), year = GetReportingDeadlineYear() };
            return routeValues == null ? pars : pars.MergeDynamic(routeValues, true);
        }

        private string GetReturnUrl() => Url.Action(nameof(SubmissionController.ManageOrganisation), "Submission", new { organisationIdentifier = GetOrganisationIdentifier() });
        private string GetBeforeYourStartUrl() => Url.Action(nameof(BeforeYouStart), GetOrgAndYearRouteData());
        private string GetTransparencyUrl() => Url.Action(nameof(Transparency), GetOrgAndYearRouteData());
        private string GetReviewUrl() => Url.Action(nameof(Review), GetOrgAndYearRouteData());

        private string GetGroupStatusUrl() => Url.Action(nameof(GroupStatus), GetOrgAndYearRouteData());
        private string GetGroupSearchUrl() => Url.Action(nameof(GroupSearch), GetOrgAndYearRouteData());
        private string GetGroupAddUrl() => Url.Action(nameof(GroupAdd), GetOrgAndYearRouteData());
        private string GetGroupReviewUrl() => Url.Action(nameof(GroupReview), GetOrgAndYearRouteData());

        private string GetUrlEmailUrl() => Url.Action(nameof(UrlEmail), GetOrgAndYearRouteData());
        private string GetPeriodUrl() => Url.Action(nameof(Period), GetOrgAndYearRouteData());
        private string GetSignOffUrl() => Url.Action(nameof(SignOff), GetOrgAndYearRouteData());

        private string GetComplianceUrl() => Url.Action(nameof(Compliance), GetOrgAndYearRouteData());

        private string GetSectorsUrl() => Url.Action(nameof(Sectors), GetOrgAndYearRouteData());
        private string GetTurnoverUrl() => Url.Action(nameof(Turnover), GetOrgAndYearRouteData());

        private string GetYearsUrl() => Url.Action(nameof(Years), GetOrgAndYearRouteData());


        private string GetPoliciesUrl() => Url.Action(nameof(Policies), GetOrgAndYearRouteData());
        private string GetTrainingUrl() => Url.Action(nameof(Training), GetOrgAndYearRouteData());

        private string GetPartnersUrl() => Url.Action(nameof(Partners), GetOrgAndYearRouteData());
        private string GetSocialAuditsUrl() => Url.Action(nameof(SocialAudits), GetOrgAndYearRouteData());
        private string GetGrievancesUrl() => Url.Action(nameof(Grievances), GetOrgAndYearRouteData());
        private string GetMonitoringUrl() => Url.Action(nameof(Monitoring), GetOrgAndYearRouteData());

        private string GetHighestRisksUrl() => Url.Action(nameof(HighestRisks), GetOrgAndYearRouteData());
        private string GetHighRiskUrl(int index) => Url.Action(nameof(HighRisk), GetOrgAndYearRouteData(new { index }));

        private string GetIndicatorsUrl() => Url.Action(nameof(Indicators), GetOrgAndYearRouteData());
        private string GetRemediationsUrl() => Url.Action(nameof(Remediations), GetOrgAndYearRouteData());

        private string GetProgressUrl() => Url.Action(nameof(Progress), GetOrgAndYearRouteData());

        private string GetCompleteUrl() => Url.Action(nameof(SubmissionComplete), GetOrgAndYearRouteData());

        /// <summary>
        /// Sets the Back, Cancel and continue url
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="viewModel"></param>
        private void SetNavigationUrl<TViewModel>(TViewModel viewModel)
        {
            switch (viewModel)
            {
                case BeforeYouStartViewModel vm:
                    vm.BackUrl = GetReturnUrl();
                    vm.ContinueUrl = GetTransparencyUrl();
                    break;
                case TransparencyViewModel vm:
                    vm.BackUrl = GetBeforeYourStartUrl();
                    vm.ContinueUrl = GetReviewUrl();
                    break;
                case ReviewViewModel vm:
                    vm.BackUrl = GetReturnUrl();
                    vm.SkipUrl = GetReturnUrl();
                    vm.ContinueUrl = GetReturnUrl();
                    vm.GroupReportingUrl = vm.GroupOrganisationsPages?.GroupSubmission != true ? GetGroupStatusUrl() : (vm.GroupOrganisationsPages?.StatementOrganisations.Any() == true) ? GetGroupReviewUrl() : GetGroupStatusUrl();
                    vm.UrlSignOffUrl = GetUrlEmailUrl();
                    vm.ComplianceUrl = GetComplianceUrl();
                    vm.SectorsUrl = GetSectorsUrl();
                    vm.YearsUrl = GetYearsUrl();
                    vm.PoliciesUrl = GetPoliciesUrl();
                    vm.TrainingUrl = GetTrainingUrl();
                    vm.WorkingConditionsUrl = GetPartnersUrl();
                    vm.RisksUrl = GetHighestRisksUrl();
                    vm.IndicatorsUrl = GetIndicatorsUrl();
                    vm.ProgressUrl = GetProgressUrl();
                    break;
                case GroupStatusViewModel vm:
                    vm.BackUrl = GetReviewUrl();
                    vm.SkipUrl = GetReviewUrl();
                    vm.ContinueUrl = vm.GroupSubmission != true ? GetReviewUrl() : GetGroupSearchUrl();
                    break;
                case GroupSearchViewModel vm:
                    vm.BackUrl = GetGroupStatusUrl();
                    vm.SkipUrl = GetReviewUrl();
                    vm.ContinueUrl = GetGroupReviewUrl();
                    break;
                case GroupAddViewModel vm:
                    vm.BackUrl = GetGroupSearchUrl();
                    vm.SkipUrl = GetReviewUrl();
                    vm.ContinueUrl = GetGroupReviewUrl();
                    break;
                case GroupReviewViewModel vm:
                    vm.BackUrl = GetGroupSearchUrl();
                    vm.SkipUrl = vm.ContinueUrl = GetReviewUrl();
                    vm.ContinueUrl = GetReviewUrl();
                    break;
                case UrlEmailViewModel vm:
                    vm.BackUrl = GetReviewUrl();
                    vm.SkipUrl = GetPeriodUrl();
                    vm.ContinueUrl = GetPeriodUrl();
                    break;
                case PeriodCoveredViewModel vm:
                    vm.BackUrl = GetUrlEmailUrl();
                    vm.SkipUrl = vm.ContinueUrl = GetSignOffUrl();
                    break;
                case SignOffViewModel vm:
                    vm.BackUrl = GetPeriodUrl();
                    vm.SkipUrl = vm.ContinueUrl = GetReviewUrl();
                    break;
                case ComplianceViewModel vm:
                    vm.BackUrl = vm.SkipUrl = vm.ContinueUrl = GetReviewUrl();
                    break;
                case SectorsViewModel vm:
                    vm.BackUrl = GetReviewUrl();
                    vm.ContinueUrl = vm.SkipUrl = GetTurnoverUrl();
                    break;
                case TurnoverViewModel vm:
                    vm.BackUrl = GetSectorsUrl();
                    vm.ContinueUrl = vm.SkipUrl = GetReviewUrl();
                    break;
                case YearsViewModel vm:
                    vm.BackUrl = vm.SkipUrl = vm.ContinueUrl = GetReviewUrl();
                    break;
                case TrainingViewModel vm:
                    vm.BackUrl = vm.SkipUrl = vm.ContinueUrl = GetReviewUrl();
                    break;
                case PoliciesViewModel vm:
                    vm.BackUrl = vm.SkipUrl = vm.ContinueUrl = GetReviewUrl();
                    break;
                case PartnersViewModel vm:
                    vm.BackUrl = GetReviewUrl();
                    vm.ContinueUrl = vm.SkipUrl = GetSocialAuditsUrl();
                    break;
                case SocialAuditsViewModel vm:
                    vm.BackUrl = GetPartnersUrl();
                    vm.ContinueUrl = vm.SkipUrl = GetGrievancesUrl();
                    break;
                case GrievancesViewModel vm:
                    vm.BackUrl = GetSocialAuditsUrl();
                    vm.ContinueUrl = vm.SkipUrl = GetMonitoringUrl();
                    break;
                case MonitoringViewModel vm:
                    vm.BackUrl = GetGrievancesUrl();
                    vm.SkipUrl = vm.ContinueUrl = GetReviewUrl();
                    break;
                case HighestRisksViewModel vm:
                    vm.BackUrl = GetReviewUrl();
                    vm.ContinueUrl = vm.SkipUrl = !string.IsNullOrWhiteSpace(vm.HighRisk1) || !string.IsNullOrWhiteSpace(vm.HighRisk2) || !string.IsNullOrWhiteSpace(vm.HighRisk3) ? GetHighRiskUrl(0) : GetReviewUrl();
                    break;
                case HighRiskViewModel vm:
                    vm.BackUrl = vm.Index == 0 ? GetHighestRisksUrl() : GetHighRiskUrl(vm.Index - 1);
                    vm.ContinueUrl = vm.SkipUrl = vm.Index == vm.TotalRisks - 1 ? GetReviewUrl() : GetHighRiskUrl(vm.Index + 1);
                    break;
                case IndicatorsViewModel vm:
                    vm.BackUrl = GetReviewUrl();
                    vm.ContinueUrl = vm.SkipUrl = GetRemediationsUrl();
                    break;
                case RemediationsViewModel vm:
                    vm.BackUrl = GetIndicatorsUrl();
                    vm.SkipUrl = vm.ContinueUrl = GetReviewUrl();
                    break;
                case ProgressViewModel vm:
                    vm.BackUrl = vm.SkipUrl = vm.ContinueUrl = GetReviewUrl();
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
            var viewModel = UnstashModel<TViewModel>();
            if (viewModel != null)
            {
                //Make sure we show any validation errors
                TryValidateModel(viewModel);

                //Dont validate the group search or organisation name
                if (viewModel is GroupSearchViewModel) ModelState.Exclude($"{nameof(GroupSearchViewModel.SearchKeywords)}");
                if (viewModel is GroupAddViewModel) ModelState.Exclude($"{nameof(GroupAddViewModel.NewOrganisationName)}", $"{nameof(GroupAddViewModel.OrganisationName)}");
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

                //Copy the previous search results
                var stashedViewModel = UnstashModel<GroupSearchViewModel>();
                if (stashedViewModel != null)
                {
                    searchViewModel.SearchKeywords = stashedViewModel.SearchKeywords;
                    searchViewModel.ResultsPage = stashedViewModel.ResultsPage;
                }

                //Populate the extra submission info
                await SubmissionPresenter.GetOtherSubmissionsInformationAsync(searchViewModel, GetReportingDeadlineYear());

                //Add the group search model to session
                StashModel(searchViewModel);
            }
            else if (viewModel is GroupAddViewModel)
            {
                var addViewModel = viewModel as GroupAddViewModel;

                //TODO: better way to handle this??
                var history = PageHistory.FirstOrDefault();
                if (!history.Contains("add"))
                {
                    //Copy the previous search keywords
                    var stashedViewModel = UnstashModel<GroupSearchViewModel>();
                    if (stashedViewModel != null)
                    {
                        addViewModel.NewOrganisationName = stashedViewModel.SearchKeywords;
                    }
                }
            }
            else if (viewModel is GroupReviewViewModel)
            {
                var reviewViewModel = viewModel as GroupReviewViewModel;

                //Populate the extra submission info
                await SubmissionPresenter.GetOtherSubmissionsInformationAsync(reviewViewModel, GetReportingDeadlineYear());

            }
            else
            {
                //Remove the group search model from session
                UnstashModel<GroupSearchViewModel>(true);
            }

            //Otherwise return the view using the populated ViewModel
            return View(viewModel);
        }

        private async Task<IActionResult> PostAsync<TViewModel>(TViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command, params object[] arguments) where TViewModel : BaseViewModel
        {
            SetNavigationUrl(viewModel);
 
            switch (command)
            {
                case BaseViewModel.CommandType.Continue:
                    //Validate the submitted ViewModel data
                    if (!ModelState.IsValid)
                    {
                        this.SetModelCustomErrors(viewModel);
                        return View(viewModel);
                    }

                    //Save the new ViewMOdel
                    var viewModelResult = await SubmissionPresenter.SaveViewModelAsync(viewModel, organisationIdentifier, year, VirtualUser.UserId, arguments);

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
            return View("BeforeYouStart", new BeforeYouStartViewModel { BackUrl = GetReturnUrl(), ContinueUrl = GetTransparencyUrl() });
        }
        #endregion

        #region Transparency
        [HttpGet("{organisationIdentifier}/{year}/transparency")]
        public async Task<IActionResult> Transparency(string organisationIdentifier, int year)
        {
            //Check if the current user can open edit draft statement data for this organisation, reporting year
            var openResult = await SubmissionPresenter.OpenDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);
            if (openResult.Fail) return HandleStatementErrors(openResult.Errors);

            //Show the correct view
            return View("Transparency", new TransparencyViewModel { BackUrl = GetBeforeYourStartUrl(), ContinueUrl = GetReviewUrl() });
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

            //set the navigation urls
            SetNavigationUrl(viewModel);

            //Otherwise return the view using the populated ViewModel
            return View(viewModel);
        }


        [HttpPost("{organisationIdentifier}/{year}/review-statement")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewPost(string organisationIdentifier, int year, bool acceptedDeclaration, BaseViewModel.CommandType command)
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

                    if (!acceptedDeclaration)
                        ModelState.AddModelError(3999, "AcceptedDeclaration");

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
                case BaseViewModel.CommandType.Skip:
                    //set the navigation urls
                    SetNavigationUrl(viewModel);

                    //Close the draft and release the user lock
                    var closeResult = await SubmissionPresenter.CloseDraftStatementModelAsync(organisationIdentifier, year, VirtualUser.UserId);

                    //Handle any StatementErrors
                    if (closeResult.Fail) return HandleStatementErrors(closeResult.Errors);

                    //Redirect to the continue url
                    return Redirect(GetReturnUrl());
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), $"CommandType {command} is not valid here");
            }
        }

        private async Task<ReviewViewModel> CreateReviewViewModelAsync(StatementModel statementModel)
        {
            //Create the view model
            var viewModel = SubmissionPresenter.GetViewModelFromStatementModel<ReviewViewModel>(statementModel);

            viewModel.GroupOrganisationsPages = SubmissionPresenter.GetViewModelFromStatementModel<GroupOrganisationsViewModel>(statementModel);
            viewModel.UrlPage = SubmissionPresenter.GetViewModelFromStatementModel<UrlEmailViewModel>(statementModel);
            viewModel.PeriodCoveredPage = SubmissionPresenter.GetViewModelFromStatementModel<PeriodCoveredViewModel>(statementModel);
            viewModel.SignOffPage = SubmissionPresenter.GetViewModelFromStatementModel<SignOffViewModel>(statementModel);
            viewModel.CompliancePage = SubmissionPresenter.GetViewModelFromStatementModel<ComplianceViewModel>(statementModel);
            viewModel.SectorsPage = SubmissionPresenter.GetViewModelFromStatementModel<SectorsViewModel>(statementModel);
            viewModel.TurnoverPage = SubmissionPresenter.GetViewModelFromStatementModel<TurnoverViewModel>(statementModel);
            viewModel.YearsPage = SubmissionPresenter.GetViewModelFromStatementModel<YearsViewModel>(statementModel);
            viewModel.PoliciesPage = SubmissionPresenter.GetViewModelFromStatementModel<PoliciesViewModel>(statementModel);
            viewModel.TrainingPage = SubmissionPresenter.GetViewModelFromStatementModel<TrainingViewModel>(statementModel);
            viewModel.PartnersPage = SubmissionPresenter.GetViewModelFromStatementModel<PartnersViewModel>(statementModel);
            viewModel.SocialAuditsPage = SubmissionPresenter.GetViewModelFromStatementModel<SocialAuditsViewModel>(statementModel);
            viewModel.GrievancesPage = SubmissionPresenter.GetViewModelFromStatementModel<GrievancesViewModel>(statementModel);
            viewModel.MonitoringPage = SubmissionPresenter.GetViewModelFromStatementModel<MonitoringViewModel>(statementModel);
            viewModel.HighestRisksPage = SubmissionPresenter.GetViewModelFromStatementModel<HighestRisksViewModel>(statementModel);
            viewModel.HighRiskPages.Clear();
            for (var index = 0; index < statementModel.Summary.Risks.Count; index++)
                viewModel.HighRiskPages.Add(SubmissionPresenter.GetViewModelFromStatementModel<HighRiskViewModel>(statementModel, index));

            viewModel.IndicatorsPage = SubmissionPresenter.GetViewModelFromStatementModel<IndicatorsViewModel>(statementModel);
            viewModel.RemediationsPage = SubmissionPresenter.GetViewModelFromStatementModel<RemediationsViewModel>(statementModel);
            viewModel.ProgressPage = SubmissionPresenter.GetViewModelFromStatementModel<ProgressViewModel>(statementModel);

            //Gewt the modifications
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

            //Rebuild the search model from the postback
            stashedModel.SearchKeywords = viewModel.SearchKeywords;
            viewModel = stashedModel;

            ModelState.Exclude(nameof(viewModel.Submitted));

            if (command.IsAny(BaseViewModel.CommandType.Search, BaseViewModel.CommandType.SearchNext, BaseViewModel.CommandType.SearchPrevious))
            {
                //Check the search string
                if (!ModelState.IsValid)
                {
                    this.SetModelCustomErrors(viewModel);
                    return View(viewModel);
                }

                if (command == BaseViewModel.CommandType.Search)
                    viewModel.ResultsPage.CurrentPage = 1;
                else if (command == BaseViewModel.CommandType.SearchNext)
                    viewModel.ResultsPage.CurrentPage++;
                else if (command == BaseViewModel.CommandType.SearchPrevious)
                    viewModel.ResultsPage.CurrentPage--;

                var outcome = await SubmissionPresenter.SearchGroupOrganisationsAsync(viewModel, VirtualUser);
                if (!outcome.Success) HandleStatementErrors(outcome.Errors, viewModel);
            }
            else if (addIndex > -1 || removeIndex > -1)
            {
                if (addIndex > -1)
                {
                    //Add the organisation to the view model
                    var outcome = await SubmissionPresenter.IncludeGroupOrganisationAsync(viewModel, addIndex);

                    //Handle any errors
                    if (!outcome.Success) return HandleStatementErrors(outcome.Errors);
                }
                else
                {
                    //Add the organisation to the view model
                    var removeOrg = viewModel.ResultsPage.Results[removeIndex];
                    var index = viewModel.FindGroupOrganisation(removeOrg);
                    viewModel.StatementOrganisations.RemoveAt(index);
                }

                //Populate the extra submission info
                await SubmissionPresenter.GetOtherSubmissionsInformationAsync(viewModel, GetReportingDeadlineYear());

                //Save to Draft file
                var viewModelResult = await SubmissionPresenter.SaveViewModelAsync(viewModel, organisationIdentifier, year, VirtualUser.UserId, null);

                //Handle any StatementErrors
                if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

            }
            else
                throw new NotImplementedException();

            //Add the group search model to session
            StashModel(viewModel);

            //Must clear otherwise hidden for doesnt work
            ModelState.Clear();

            //Redirect to the page
            return RedirectToAction(nameof(GroupSearch), GetOrgAndYearRouteData());

        }

        [HttpGet("{organisationIdentifier}/{year}/group-add")]
        public async Task<IActionResult> GroupAdd(string organisationIdentifier, int year)
        {
            return await GetAsync<GroupAddViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/group-add")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GroupAdd(GroupAddViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command, int addIndex = -1, int removeIndex = -1, params object[] arguments)
        {
            //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
            var viewModelResult = await SubmissionPresenter.GetViewModelAsync<GroupAddViewModel>(organisationIdentifier, year, VirtualUser.UserId, arguments);
            ////Handle any StatementErrors
            if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);
            var viewModelDraft = viewModelResult.Result;

            //Rebuild the search model from the postback          
            viewModelDraft.NewOrganisationName = viewModel.NewOrganisationName;
            viewModel = viewModelDraft;


            if (addIndex > -1 || removeIndex > -1)
            {
                //Dont validate the organisation name
                ModelState.Exclude(nameof(viewModel.Submitted), $"{nameof(viewModel.NewOrganisationName)}");

                if (addIndex > -1)
                {
                    //Check the search string
                    if (!ModelState.IsValid)
                    {
                        this.SetModelCustomErrors(viewModel);
                        return View(viewModel);
                    }

                    //Add the organisation to the view model
                    var outcome = await SubmissionPresenter.AddGroupOrganisationAsync(viewModel);

                    //Handle any errors
                    if (!outcome.Success) return HandleStatementErrors(outcome.Errors);

                }
                else
                {
                    //Remove the selected organisation
                    viewModel.StatementOrganisations.RemoveAt(removeIndex);
                }

                //Clear the input name 
                viewModel.NewOrganisationName = null;

                //Save the new ViewModel
                var viewModelResultForSave = await SubmissionPresenter.SaveViewModelAsync(viewModel, organisationIdentifier, year, VirtualUser.UserId, arguments);

                //Handle any StatementErrors
                if (viewModelResultForSave.Fail) return HandleStatementErrors(viewModelResultForSave.Errors);

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

            //Must clear otherwise hidden for doesnt work
            ModelState.Clear();

            //Redirect to the page
            return RedirectToAction(nameof(GroupAdd), GetOrgAndYearRouteData());
        }

        [HttpGet("{organisationIdentifier}/{year}/group-review")]
        public async Task<IActionResult> GroupReview(string organisationIdentifier, int year)
        {
            return await GetAsync<GroupReviewViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/group-review")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GroupReview(GroupReviewViewModel viewModel, string organisationIdentifier, int year, BaseViewModel.CommandType command, int removeIndex = -1, params object[] arguments)
        {
            //Get the populated ViewModel from the Draft StatementModel for this organisation, reporting year and user
            var viewModelDraft = await SubmissionPresenter.GetViewModelAsync<GroupReviewViewModel>(organisationIdentifier, year, VirtualUser.UserId, arguments);
            ////Handle any StatementErrors
            if (viewModelDraft.Fail) return HandleStatementErrors(viewModelDraft.Errors);

            viewModel = viewModelDraft.Result;

            if (command.IsAny(BaseViewModel.CommandType.Continue, BaseViewModel.CommandType.Skip))
            {
                //Continue or cancel normally
                return await PostAsync(viewModel, organisationIdentifier, year, command);
            }
            else if (removeIndex > -1)
            {
                //Remove the selected organisation
                viewModel.StatementOrganisations.RemoveAt(removeIndex);

                //Save the new ViewModel
                var viewModelResult = await SubmissionPresenter.SaveViewModelAsync(viewModel, organisationIdentifier, year, VirtualUser.UserId, null);

                //Handle any StatementErrors
                if (viewModelResult.Fail) return HandleStatementErrors(viewModelResult.Errors);

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

            //Must clear otherwise hidden for doesnt work
            ModelState.Clear();

            //Redirect to the page          
            return RedirectToAction(nameof(GroupReview), GetOrgAndYearRouteData());
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
            return await GetAsync<HighRiskViewModel>(organisationIdentifier, year, index);
        }

        [HttpPost("{organisationIdentifier}/{year}/highest-risk/{index}")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HighRisk(HighRiskViewModel viewModel, string organisationIdentifier, int year, int index, BaseViewModel.CommandType command)
        {
            return await PostAsync(viewModel, organisationIdentifier, year, command, index);
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
        [HttpGet("{organisationIdentifier}/{year}/progress")]
        public async Task<IActionResult> Progress(string organisationIdentifier, int year)
        {
            return await GetAsync<ProgressViewModel>(organisationIdentifier, year);
        }

        [HttpPost("{organisationIdentifier}/{year}/progress")]
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
