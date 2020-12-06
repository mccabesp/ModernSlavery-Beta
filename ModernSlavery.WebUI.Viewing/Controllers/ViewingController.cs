using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Viewing.Models;
using ModernSlavery.WebUI.Viewing.Presenters;

namespace ModernSlavery.WebUI.Viewing.Controllers
{
    [Area("Viewing")]
    [Route("viewing")]
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
        #endregion

        #region Search

        [NoCache]
        [HttpGet("~/search")]
        [HttpGet("search")]
        public async Task<IActionResult> Search(SearchQueryModel searchQuery)
        {
            //Ensure search service is enabled
            if (ViewingService.SearchBusinessLogic.Disabled)
                return View("CustomError",
                    WebService.ErrorViewModelFactory.Create(1151, new { featureName = "Search Service" }));

            // ensure parameters are valid
            if (!searchQuery.TryValidateSearchParams(out var result)) return result;

            // generate result view model
            // var model = await ViewingPresenter.SearchAsync(searchQuery);
            var model = ViewingPresenter.GetSearchViewModel(searchQuery);

            SearchPresenter.CacheCurrentSearchUrl();

            return View("Search", model);
        }

        /// <summary>
        /// </summary>
        /// <param name="searchQuery"></param>
        [NoCache]
        [HttpGet("~/search-results")]
        [HttpGet("search-results")]
        public async Task<IActionResult> SearchResults(SearchQueryModel searchQuery)
        {
            //Ensure search service is enabled
            if (ViewingService.SearchBusinessLogic.Disabled)
                return View("CustomError",
                    WebService.ErrorViewModelFactory.Create(1151, new { featureName = "Search Service" }));

            //Clear the default back url of the organisation hub pages
            OrganisationBackUrl = null;
            ReportBackUrl = null;

            // ensure parameters are valid
            if (!searchQuery.TryValidateSearchParams(out var result)) return result;

            // generate result view model
            var model = await ViewingPresenter.SearchAsync(searchQuery);

            SearchPresenter.CacheCurrentSearchUrl();

            return View("SearchResults", model);
        }

        [NoCache]
        [HttpGet("~/search-results-js")]
        [HttpGet("search-results-js")]
        public async Task<IActionResult> SearchResultsJs(SearchQueryModel searchQuery)
        {
            //Clear the default back url of the organisation hub pages
            OrganisationBackUrl = null;
            ReportBackUrl = null;

            // ensure parameters are valid
            if (!searchQuery.TryValidateSearchParams(out var result)) return result;

            // generate result view model
            var model = await ViewingPresenter.SearchAsync(searchQuery);

            SearchPresenter.CacheCurrentSearchUrl();

            return PartialView("Parts/_SearchMainContent", model);
        }

        #endregion

        #region Downloads

        [HttpGet("~/download")]
        [HttpGet("download")]
        public async Task<IActionResult> Download()
        {
            var model = new DownloadViewModel { Downloads = new List<DownloadViewModel.Download>() };

            //returns back to correct search page
            var history = PageHistory.FirstOrDefault();
            var lastSearchUrl = SearchPresenter.GetLastSearchUrl();
            if (history.Contains("search-results-js"))
                model.BackUrl = string.IsNullOrEmpty(lastSearchUrl) ? "/search" : lastSearchUrl;
            else
                model.BackUrl = string.IsNullOrEmpty(history) ? "/search" : history;

            //Return the view with the model
            return View("Download", model);
        }

        #endregion

        #region Statement Summary

        [HttpGet("statement-summary/{organisationIdentifier}/{year}")]
        public async Task<IActionResult> StatementSummary(string organisationIdentifier, int year, string returnUrl = null)
        {
            //Get the latest statement data for this organisation, reporting year
            var openResult = await ViewingPresenter.GetStatementSummaryViewModel(organisationIdentifier, year);
            if (openResult.Fail) return HandleStatementViewErrors(openResult.Errors);

            var viewModel = openResult.Result;
            var backUrl = SearchPresenter.GetLastSearchUrl();
            viewModel.BackUrl = returnUrl ?? (string.IsNullOrEmpty(backUrl) ? "/search" : backUrl);

            return View("StatementSummary", viewModel);
        }

        [HttpGet("statement-summary/{organisationIdentifier}/{year}/url")]
        public async Task<IActionResult> StatementSummaryUrl(string organisationIdentifier, int year)
        {
            var openResult = await ViewingPresenter.GetLinkRedirectUrl(organisationIdentifier, year);
            if (openResult.Fail)
                return HandleStatementViewErrors(openResult.Errors);

            if (string.IsNullOrEmpty(openResult.Result))
                return RedirectToAction(nameof(StatementSummaryLinkNotWorking), new { organisationIdentifier, year });
            else
                return Redirect(openResult.Result);
        }

        [HttpGet("statement-summary/{organisationIdentifier}/{year}/link-not-working")]
        public async Task<IActionResult> StatementSummaryLinkNotWorking(string organisationIdentifier, int year)
        {
            var openResult = await ViewingPresenter.GetStatementSummaryViewModel(organisationIdentifier, year);
            if (openResult.Fail) return HandleStatementViewErrors(openResult.Errors);

            var viewModel = openResult.Result;
            viewModel.BackUrl = $"/viewing/statement-summary/{organisationIdentifier}/{year}";

            return View("StatementSummaryLinkNotWorking", viewModel);
        }

        [HttpGet("statement-summary/{organisationIdentifier}/{year}/group")]
        public async Task<IActionResult> StatementSummaryGroup(string organisationIdentifier, int year)
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