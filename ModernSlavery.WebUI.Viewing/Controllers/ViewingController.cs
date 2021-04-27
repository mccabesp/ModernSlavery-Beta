using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Viewing.Models;
using ModernSlavery.WebUI.Viewing.Presenters;

namespace ModernSlavery.WebUI.Viewing.Controllers
{
    [Area("Viewing")]
    [Route("viewing")]
    [WhitelistUsersFilter(nameof(BaseController.Init))]
    public class ViewingController : BaseController
    {
        #region Dependencies
        public IViewingService ViewingService { get; }
        public IViewingPresenter ViewingPresenter { get; }
        public ISearchPresenter SearchPresenter { get; }
        #endregion

        #region Constructors
        public ViewingController(
            IViewingService viewingService,
            IViewingPresenter viewingPresenter,
            ISearchPresenter searchPresenter,
            ILogger<ViewingController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(
            logger, webService, sharedBusinessLogic)
        {
            ViewingService = viewingService;
            ViewingPresenter = viewingPresenter;
            SearchPresenter = searchPresenter;
        }
        #endregion

        #region Page Handling Methods
        /// <summary>
        /// Returns an ActionResult to handle any StatementErrors
        /// </summary>
        /// <param name="errors">A List of Statement Errors and their description</param>
        /// <returns>The IActionResult to execute</returns>
        private IActionResult HandleStatementViewErrors(IEnumerable<(StatementErrors Error, string Message)> errors)
        {
            //Return full page errors which return to the ManageOrganisation page
            var error = errors.FirstOrDefault();
            switch (error.Error)
            {
                case StatementErrors.NotFound:
                    return new HttpNotFoundResult(error.Message);
                default:
                    throw new NotImplementedException($"{nameof(StatementErrors)} type '{error.Error}' is not recognised");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Redirect()
        {
            //Dont save in history
            SkipSaveHistory = true;

            await TrackPageViewAsync();

            return RedirectPermanent("/");
        }

        #endregion

        #region Landing
        [HttpGet("~/landing")]
        public IActionResult Landing()
        {
            return View("Landing", null);
        }
        #endregion

        #region Search

        [NoCache]
        [HttpGet("~/search")]
        [HttpGet("search")]
        public async Task<IActionResult> Search()
        {
            //Ensure search service is enabled
            if (ViewingService.SearchBusinessLogic.Disabled)
                return View("CustomError",
                    WebService.ErrorViewModelFactory.Create(1151, new { featureName = "Search Service" }));

            var viewModel = ActivatorUtilities.CreateInstance<SearchViewModel>(HttpContext.RequestServices);

            return View("Search", viewModel);
        }

        /// <summary>
        /// </summary>
        /// <param name="viewModel"></param>
        [NoCache]
        [HttpGet("~/search-results")]
        [HttpGet("search-results")]
        public async Task<IActionResult> SearchResults(SearchViewModel viewModel)
        {
            //Ensure search service is enabled
            if (ViewingService.SearchBusinessLogic.Disabled)
                return View("CustomError",WebService.ErrorViewModelFactory.Create(1151, new { featureName = "Search Service" }));

            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors(viewModel);
                return View("SearchResults", viewModel);
            }

            //Clear the default back url of the organisation hub pages
            OrganisationBackUrl = null;
            ReportBackUrl = null;

            // ensure parameters are valid
            if (!viewModel.TryValidateSearchParams(out var result)) return result;

            // generate result view model
            await ViewingPresenter.SearchAsync(viewModel);

            SearchPresenter.CacheCurrentSearchUrl();

            return View("SearchResults", viewModel);
        }

        [NoCache]
        [HttpGet("~/search-results-js")]
        [HttpGet("search-results-js")]
        public async Task<IActionResult> SearchResultsJs(SearchViewModel viewModel)
        {
            //Clear the default back url of the organisation hub pages
            OrganisationBackUrl = null;
            ReportBackUrl = null;

            if (!ModelState.IsValid)
            {
                this.SetModelCustomErrors(viewModel);
                return new HttpBadRequestResult(ModelState.Values.FirstOrDefault(v=>v.ValidationState==ModelValidationState.Invalid)?.Errors?.FirstOrDefault()?.ErrorMessage);
            }

            // ensure parameters are valid
            if (!viewModel.TryValidateSearchParams(out var result)) return result;

            // generate result view model
            await ViewingPresenter.SearchAsync(viewModel);

            SearchPresenter.CacheCurrentSearchUrl();

            //Set the back page
            var backUrl = SearchPresenter.GetLastSearchUrl();
            SetBackUrl(backUrl);

            return PartialView("Parts/_SearchMainContent", viewModel);
        }

        #endregion

        #region Downloads

        [HttpGet("~/download")]
        [HttpGet("download")]
        [ResponseCache(CacheProfileName = "1Hour")]
        public async Task<IActionResult> Download()
        {
            var model = new DownloadViewModel { Downloads = new List<DownloadViewModel.Download>() };

            //returns back to correct search page
            var history = PageHistory.FirstOrDefault();
            if (history != null && history.Contains("search-results-js"))
            {
                var lastSearchUrl = SearchPresenter.GetLastSearchUrl();
                model.BackUrl = string.IsNullOrWhiteSpace(lastSearchUrl) ? "/search" : lastSearchUrl;
            }
            else
                model.BackUrl = string.IsNullOrWhiteSpace(history) ? "/search" : history;

            //Return the view with the model
            return View("Download", model);
        }

        #endregion

        #region Statement Summary
        [HttpGet("statement-summary/{organisationIdentifier}/{year}")]
        public async Task<IActionResult> StatementSummary([Obfuscated]long organisationIdentifier, int year, [RelativeUrl]string returnUrl = null)
        {
            //Get the latest statement data for this organisation, reporting year
            var openResult = await ViewingPresenter.GetStatementSummaryViewModel(organisationIdentifier, year);
            if (openResult.Fail) return HandleStatementViewErrors(openResult.Errors);

            var viewModel = openResult.Result;
            var backUrl = SearchPresenter.GetLastSearchUrl();
            viewModel.BackUrl = returnUrl ?? (string.IsNullOrEmpty(backUrl) ? "/search" : backUrl);

            return View("StatementSummary", viewModel);
        }

        [HttpGet("statement-summary/{organisationIdentifier}/{year}/group")]
        public async Task<IActionResult> StatementSummaryGroup([Obfuscated] long organisationIdentifier, int year)
        {
            //Get the latest statement data for this organisation, reporting year
            var openResult = await ViewingPresenter.GetStatementSummaryGroupViewModel(organisationIdentifier, year);
            if (openResult.Fail) return HandleStatementViewErrors(openResult.Errors);

            var viewModel = openResult.Result;
            return View("StatementSummaryGroup", viewModel);
        }

        #endregion
    }
}