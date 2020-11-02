using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.HttpResultModels;
using ModernSlavery.WebUI.Shared.Controllers;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Models;
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

        #region Initialisation

        /// <summary>
        ///     This action is only used to warm up this controller on initialisation
        /// </summary>
        [HttpGet("Init")]
        public IActionResult Init()
        {
            if (!SharedBusinessLogic.SharedOptions.IsProduction())
                Logger.LogInformation("Viewing Controller Initialised");

            return new EmptyResult();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //Clear the default back url of the organisation hub pages
            OrganisationBackUrl = null;
            ReportBackUrl = null;

            await TrackPageViewAsync();

            return RedirectToPage("/landing");
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

        [HttpGet("download-data")]
        [HttpGet("download-data/{year:int=0}")]
        public async Task<IActionResult> DownloadData(int year = 0)
        {
            if (year == 0) year = ViewingService.SharedBusinessLogic.ReportingDeadlineHelper.GetReportingStartDate(SectorTypes.Private).Year;

            //Ensure we have a directory
            if (!await SharedBusinessLogic.FileRepository.GetDirectoryExistsAsync(SharedBusinessLogic.SharedOptions
                .DownloadsLocation))
                return new HttpNotFoundResult(
                    $"Directory '{SharedBusinessLogic.SharedOptions.DownloadsLocation}' does not exist");

            //Ensure we have a file
            var filePattern = $"GPGData_{year}-{year + 1}.csv";
            var files = await SharedBusinessLogic.FileRepository.GetFilesAsync(
                SharedBusinessLogic.SharedOptions.DownloadsLocation, filePattern);
            var file = files.FirstOrDefault();
            if (file == null || !await SharedBusinessLogic.FileRepository.GetFileExistsAsync(file))
                return new HttpNotFoundResult("Cannot find GPG data file for year: " + year);
            //Get the public and private accounting dates for the specified year

            //TODO log download

            //Setup the HTTP response
            var contentDisposition = new ContentDisposition
            {
                FileName = $"UK Modern Slavery statement - {year} to {year + 1}.csv",
                Inline = false
            };
            HttpContext.SetResponseHeader("Content-Disposition", contentDisposition.ToString());

            //cache old files for 1 day
            var lastWriteTime = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(file);
            if (lastWriteTime.AddMonths(12) < VirtualDateTime.Now)
                Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
                { MaxAge = TimeSpan.FromDays(1), Public = true };

            /* No Longer required as AspNetCore has response buffering on by default
            Response.BufferOutput = true;
            */
            //Track the download 
            await TrackPageViewAsync(contentDisposition.FileName);

            //Return the data
            return Content(await SharedBusinessLogic.FileRepository.ReadAsync(file), "text/csv");
        }

        #endregion

        #region Statement Summary

        [HttpGet("statement-summary/{organisationIdentifier}/{year}")]
        public async Task<IActionResult> StatementSummary(string organisationIdentifier, int year)
        {
            //Get the latest statement data for this organisation, reporting year
            var openResult = await ViewingPresenter.GetStatementSummaryViewModel(organisationIdentifier, year);
            if (openResult.Fail) return HandleStatementViewErrors(openResult.Errors);

            var viewModel = openResult.Result;
            var backUrl = SearchPresenter.GetLastSearchUrl();
            viewModel.BackUrl = string.IsNullOrEmpty(backUrl) ? "/search" : backUrl;

            return View("StatementSummary", viewModel);
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