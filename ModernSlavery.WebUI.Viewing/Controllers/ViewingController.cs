using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
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
        #region Constructors

        public ViewingController(
            IViewingService viewingService,
            IViewingPresenter viewingPresenter,
            ISearchPresenter searchPresenter,
            IComparePresenter comparePresenter,
            ILogger<ViewingController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(
            logger, webService, sharedBusinessLogic)
        {
            ViewingService = viewingService;
            ViewingPresenter = viewingPresenter;
            SearchPresenter = searchPresenter;
            ComparePresenter = comparePresenter;
        }

        #endregion

        #region Dependencies

        public IViewingService ViewingService { get; }
        public IViewingPresenter ViewingPresenter { get; }
        public ISearchPresenter SearchPresenter { get; }
        public IComparePresenter ComparePresenter { get; }

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

        [HttpGet("Index")]
        public IActionResult Index()
        {
            //Clear the default back url of the organisation hub pages
            OrganisationBackUrl = null;
            ReportBackUrl = null;

            if (WebService.FeatureSwitchOptions.IsEnabled("ReportingStepByStep"))
                return View("Launchpad/PrototypeIndex");
            return View("Launchpad/Index");
        }

        [HttpGet]
        public async Task<IActionResult> Redirect()
        {
            await TrackPageViewAsync();

            return RedirectToActionPermanent("Index");
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
        #endregion

        #region Search

        /// <summary>
        /// </summary>
        /// <param name="searchQuery"></param>
        [NoCache]
        [HttpGet("~/search-results")]
        [HttpGet("search-results")]
        public async Task<IActionResult> SearchResults([FromQuery] SearchResultsQuery searchQuery)
        {
            //Ensure search service is enabled
            if (ViewingService.SearchBusinessLogic.OrganisationSearchRepository.Disabled)
                return View("CustomError",
                    WebService.ErrorViewModelFactory.Create(1151, new {featureName = "Search Service"}));

            //When never searched in this session
            if (string.IsNullOrWhiteSpace(SearchPresenter.LastSearchParameters))
                //If no compare organisations in session then load organisations from the cookie
                if (ComparePresenter.BasketItemCount == 0)
                    ComparePresenter.LoadComparedOrganisationsFromCookie();

            //Clear the default back url of the organisation hub pages
            OrganisationBackUrl = null;
            ReportBackUrl = null;

            // ensure parameters are valid
            if (!searchQuery.TryValidateSearchParams(out var result)) return result;

            // generate result view model
            var searchParams = AutoMapper.Map<OrganisationSearchParameters>(searchQuery);
            var model = await ViewingPresenter.SearchAsync(searchParams);

            ViewBag.ReturnUrl = SearchPresenter.GetLastSearchUrl();

            ViewBag.BasketViewModel = new CompareBasketViewModel
            {
                CanAddOrganisations = false, CanViewCompare = ComparePresenter.BasketItemCount > 1, CanClearCompare = true
            };

            return View("Finder/SearchResults", model);
        }

        [NoCache]
        [HttpGet("~/search-results-js")]
        [HttpGet("search-results-js")]
        public async Task<IActionResult> SearchResultsJs([FromQuery] SearchResultsQuery searchQuery)
        {
            //Clear the default back url of the organisation hub pages
            OrganisationBackUrl = null;
            ReportBackUrl = null;


            // ensure parameters are valid
            if (!searchQuery.TryValidateSearchParams(out var result)) return result;

            // generate result view model
            var searchParams = AutoMapper.Map<OrganisationSearchParameters>(searchQuery);
            var model = await ViewingPresenter.SearchAsync(searchParams);

            ViewBag.ReturnUrl = SearchPresenter.GetLastSearchUrl();

            return PartialView("Parts/_SearchMainContent", model);
        }

        #endregion

        #region Downloads

        [HttpGet("~/download")]
        public async Task<IActionResult> Download()
        {
            var model = new DownloadViewModel {Downloads = new List<DownloadViewModel.Download>()};

            const string filePattern = "GPGData_????-????.csv";
            foreach (var file in await SharedBusinessLogic.FileRepository.GetFilesAsync(
                SharedBusinessLogic.SharedOptions.DownloadsLocation, filePattern))
            {
                var download = new DownloadViewModel.Download
                {
                    Title = Path.GetFileNameWithoutExtension(file).AfterFirst("GPGData_"),
                    Count = await SharedBusinessLogic.FileRepository.GetMetaDataAsync(file, "RecordCount"),
                    Extension = Path.GetExtension(file).TrimI("."),
                    Size = Numeric.FormatFileSize(await SharedBusinessLogic.FileRepository.GetFileSizeAsync(file))
                };

                download.Url = Url.Action("DownloadData", new {year = download.Title.BeforeFirst("-")});
                model.Downloads.Add(download);
            }

            //Sort downloads by descending year
            model.Downloads = model.Downloads.OrderByDescending(d => d.Title).ToList();

            //Return the view with the model
            return View("Download", model);
        }

        [HttpGet("download-data")]
        [HttpGet("download-data/{year:int=0}")]
        public async Task<IActionResult> DownloadData(int year = 0)
        {
            if (year == 0) year = ViewingService.SharedBusinessLogic.GetReportingStartDate(SectorTypes.Private).Year;

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
                FileName = $"UK Modern Slavery statement - {year} to {year + 1}.csv", Inline = false
            };
            HttpContext.SetResponseHeader("Content-Disposition", contentDisposition.ToString());

            //cache old files for 1 day
            var lastWriteTime = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(file);
            if (lastWriteTime.AddMonths(12) < VirtualDateTime.Now)
                Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
                    {MaxAge = TimeSpan.FromDays(1), Public = true};

            /* No Longer required as AspNetCore has response buffering on by default
            Response.BufferOutput = true;
            */
            //Track the download 
            await TrackPageViewAsync(contentDisposition.FileName);

            //Return the data
            return Content(await SharedBusinessLogic.FileRepository.ReadAsync(file), "text/csv");
        }

        #endregion

        #region Organisation details
        [NoCache]
        [HttpGet("~/Organisation/{organisationIdentifier}")]
        public IActionResult Organisation(string organisationIdentifier)
        {
            if (string.IsNullOrWhiteSpace(organisationIdentifier))
                return new HttpBadRequestResult("Missing organisation identifier");

            CustomResult<Organisation> organisationLoadingOutcome;

            try
            {
                long organisationId = ViewingPresenter.Obfuscator.DeObfuscate(organisationIdentifier);
                organisationLoadingOutcome = ViewingService.OrganisationBusinessLogic.LoadInfoFromActiveOrganisationId(organisationId);

                if (organisationLoadingOutcome.Failed)
                    return organisationLoadingOutcome.ErrorMessage.ToHttpStatusViewResult();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Cannot decrypt return organisationIdentifier from '{organisationIdentifier}'");
                return View("CustomError", WebService.ErrorViewModelFactory.Create(400));
            }

            //Clear the default back url of the report page
            ReportBackUrl = null;

            ViewBag.BasketViewModel = new CompareBasketViewModel {CanAddOrganisations = true, CanViewCompare = true};

            return View(
                "OrganisationDetails/Organisation",
                new OrganisationDetailsViewModel
                {
                    Organisation = organisationLoadingOutcome.Result,
                    LastSearchUrl = SearchPresenter.GetLastSearchUrl(),
                    OrganisationBackUrl = OrganisationBackUrl,
                    ComparedOrganisations = ComparePresenter.ComparedOrganisations.Value
                });
        }

        #endregion

        #region Reports
        [HttpGet("~/Organisation/{organisationIdentifier}/{year}")]
        public async Task<IActionResult> Report(string organisationIdentifier, int year)
        {
            //Get the latest statement data for this organisation, reporting year
            var openResult = await ViewingPresenter.GetStatementViewModelAsync(organisationIdentifier, year);
            if (openResult.Fail) return HandleStatementErrors(openResult.Errors);

            var viewModel = openResult.Result;
            return View("OrganisationDetails/Report", viewModel);
        }

        [HttpGet("add-search-results-to-compare")]
        public async Task<IActionResult> AddSearchResultsToCompare([FromQuery] SearchResultsQuery searchQuery)
        {
            if (!searchQuery.TryValidateSearchParams(out var result)) return result;

            // generate compare list
            var searchParams = AutoMapper.Map<OrganisationSearchParameters>(searchQuery);

            // set maximum search size
            searchParams.Page = 1;
            searchParams.PageSize = ComparePresenter.MaxCompareBasketCount;
            var searchResultsModel = await ViewingPresenter.SearchAsync(searchParams);

            // add any new items to the compare list
            var resultIds = searchResultsModel.Organisations.Results
                .Where(organisation => ComparePresenter.BasketContains(ViewingPresenter.Obfuscator.Obfuscate(organisation.OrganisationId)) == false)
                .Take(ComparePresenter.MaxCompareBasketCount - ComparePresenter.BasketItemCount)
                .Select(organisation => ViewingPresenter.Obfuscator.Obfuscate(organisation.OrganisationId))
                .ToArray();

            ComparePresenter.AddRangeToBasket(resultIds);

            // save the results to the cookie
            ComparePresenter.SaveComparedOrganisationsToCookie(Request);

            return RedirectToAction(nameof(SearchResults), searchQuery);
        }

        #endregion
    }
}