﻿using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
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
    public class CompareController : BaseController
    {
        public ICompareBusinessLogic CompareBusinessLogic { get; set; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; set; }

        public ISearchPresenter SearchViewService { get; }

        public IComparePresenter CompareViewService { get; }
        public CompareController(
            ISearchPresenter searchViewService,
            IComparePresenter compareViewService,
            ICompareBusinessLogic compareBusinessLogic,
            IOrganisationBusinessLogic organisationBusinessLogic,
            ILogger<CompareController> logger, IWebService webService, ISharedBusinessLogic sharedBusinessLogic) : base(
            logger, webService, sharedBusinessLogic)
        {
            CompareBusinessLogic = compareBusinessLogic;
            OrganisationBusinessLogic = organisationBusinessLogic;
            SearchViewService = searchViewService;
            CompareViewService = compareViewService;
        }

        [HttpGet("add-employer/{employerIdentifier}")]
        public IActionResult AddEmployer(string employerIdentifier, string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(employerIdentifier))
                return new HttpBadRequestResult($"Missing {nameof(employerIdentifier)}");

            if (string.IsNullOrWhiteSpace(returnUrl)) return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");

            //Get the employer from the encrypted identifier
            var employer = GetEmployer(employerIdentifier);

            //Add the employer to the compare list
            CompareViewService.AddToBasket(employer.OrganisationIdEncrypted);

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            //Redirect the user to the original page
            return Redirect(returnUrl);
        }

        [NoCache]
        [HttpGet("add-employer-js/{employerIdentifier}")]
        public IActionResult AddEmployerJs(string employerIdentifier, string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(employerIdentifier))
                return new HttpBadRequestResult($"Missing {nameof(employerIdentifier)}");

            if (string.IsNullOrWhiteSpace(returnUrl)) return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");

            //Get the employer from the encrypted identifier
            var employer = GetEmployer(employerIdentifier);

            //Add the employer to the compare list
            CompareViewService.AddToBasket(employer.OrganisationIdEncrypted);

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            //Setup compare basket
            var fromSearchResults = returnUrl.Contains("/search-results");
            var fromEmployer = returnUrl.StartsWithI("/employer");
            ViewBag.BasketViewModel = new CompareBasketViewModel
            {
                CanAddEmployers = false,
                CanClearCompare = true,
                CanViewCompare = fromSearchResults && CompareViewService.BasketItemCount > 1
                                 || fromEmployer && CompareViewService.BasketItemCount > 0
            };

            ViewBag.ReturnUrl = returnUrl;

            var model = new AddRemoveButtonViewModel
            {
                OrganisationIdEncrypted = employer.OrganisationIdEncrypted, OrganisationName = employer.Name
            };

            return PartialView("Basket_Button", model);
        }

        [HttpGet("remove-employer/{employerIdentifier}")]
        public IActionResult RemoveEmployer(string employerIdentifier, string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(employerIdentifier))
                return new HttpBadRequestResult($"Missing {nameof(employerIdentifier)}");

            if (string.IsNullOrWhiteSpace(returnUrl)) return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");

            //Get the employer from the encrypted identifier
            var employer = GetEmployer(employerIdentifier);

            //Remove the employer from the list
            CompareViewService.RemoveFromBasket(employer.OrganisationIdEncrypted);

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            return Redirect(returnUrl);
        }

        [NoCache]
        [HttpGet("remove-employer-js/{employerIdentifier}")]
        public IActionResult RemoveEmployerJs(string employerIdentifier, string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(employerIdentifier))
                return new HttpBadRequestResult($"Missing {nameof(employerIdentifier)}");

            if (string.IsNullOrWhiteSpace(returnUrl)) return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");

            //Get the employer from the encrypted identifier
            var employer = GetEmployer(employerIdentifier);

            //Remove the employer from the list
            CompareViewService.RemoveFromBasket(employer.OrganisationIdEncrypted);

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            //Setup compare basket
            var fromSearchResults = returnUrl.Contains("/search-results");
            var fromEmployer = returnUrl.StartsWithI("/employer");
            ViewBag.BasketViewModel = new CompareBasketViewModel
            {
                CanAddEmployers = false,
                CanClearCompare = true,
                CanViewCompare = fromSearchResults && CompareViewService.BasketItemCount > 1
                                 || fromEmployer && CompareViewService.BasketItemCount > 0
            };

            ViewBag.ReturnUrl = returnUrl;

            var model = new AddRemoveButtonViewModel
            {
                OrganisationIdEncrypted = employer.OrganisationIdEncrypted, OrganisationName = employer.Name
            };

            return PartialView("Basket_Button", model);
        }

        [HttpGet("clear-employers")]
        public IActionResult ClearEmployers(string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(returnUrl)) return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");

            CompareViewService.ClearBasket();

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            return Redirect(returnUrl);
        }

        [HttpGet("sort-employers/{column}")]
        public async Task<IActionResult> SortEmployers(string column, string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(column)) return new HttpBadRequestResult($"Missing {nameof(column)}");

            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(returnUrl)) return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");


            //Calculate the sort direction
            var sort = CompareViewService.SortColumn != column || !CompareViewService.SortAscending;

            //Track the download 
            if (CompareViewService.SortColumn != column || CompareViewService.SortAscending != sort)
            {
                await TrackPageViewAsync(
                    $"sort-employers: {column} {(CompareViewService.SortAscending ? "Ascending" : "Descending")}",
                    $"/compare-employers/sort-employers?{column}={(CompareViewService.SortAscending ? "Ascending" : "Descending")}");

                //Change the sort order
                CompareViewService.SortAscending = sort;

                //Set the column
                CompareViewService.SortColumn = column;
            }

            return Redirect(returnUrl);
        }

        [HttpGet("~/compare-employers/{year:int=0}")]
        public async Task<IActionResult> CompareEmployers(int year, string employers = null)
        {
            if (year == 0)
            {
                CompareViewService.SortColumn = null;
                CompareViewService.SortAscending = true;
                year = SharedBusinessLogic.SharedOptions.FirstReportingDeadlineYear;
            }

            //Load employers from querystring (via shared email)
            if (!string.IsNullOrWhiteSpace(employers))
            {
                var comparedEmployers = employers.SplitI("-");
                if (comparedEmployers.Any())
                {
                    CompareViewService.ClearBasket();
                    CompareViewService.AddRangeToBasket(comparedEmployers);
                    CompareViewService.SortAscending = true;
                    CompareViewService.SortColumn = null;
                    return RedirectToAction("CompareEmployers", new {year});
                }
            }

            //If the session is lost then load employers from the cookie
            else if (CompareViewService.BasketItemCount == 0)
            {
                CompareViewService.LoadComparedEmployersFromCookie();
            }

            ViewBag.ReturnUrl = Url.Action("CompareEmployers", new {year});

            //Clear the default back url of the employer hub pages
            EmployerBackUrl = null;
            ReportBackUrl = null;

            //Get the compare basket organisations
            var compareReports = await CompareBusinessLogic.GetCompareDataAsync(
                CompareViewService.ComparedEmployers.Value.AsEnumerable(),
                year,
                CompareViewService.SortColumn,
                CompareViewService.SortAscending);

            //Track the compared employers
            var lastComparedEmployerList =
                CompareViewService.ComparedEmployers.Value.ToList().ToSortedSet().ToDelimitedString();
            if (CompareViewService.LastComparedEmployerList != lastComparedEmployerList && IsAction("CompareEmployers"))
            {
                var employerIds = compareReports.Select(r => r.EncOrganisationId).ToSortedSet();
                await TrackPageViewAsync(
                    $"compare-employers: {employerIds.ToDelimitedString()}",
                    $"{ViewBag.ReturnUrl}?{employerIds.ToEncapsulatedString("e=", null, "&", "&", false)}");
                foreach (var employer in compareReports)
                    await TrackPageViewAsync(
                        $"{employer.EncOrganisationId}: {employer.OrganisationName}",
                        $"{ViewBag.ReturnUrl}?{employer.EncOrganisationId}={employer.OrganisationName}");

                CompareViewService.LastComparedEmployerList = lastComparedEmployerList;
            }

            //Generate the shared links
            var shareEmailUrl = Url.Action(
                nameof(CompareEmployers),
                "Compare",
                new {year, employers = CompareViewService.ComparedEmployers.Value.ToList().ToDelimitedString("-")},
                Request.Scheme);

            ViewBag.BasketViewModel = new CompareBasketViewModel
                {CanAddEmployers = true, CanViewCompare = false, CanClearCompare = true};

            return View(
                "CompareEmployers",
                new CompareViewModel
                {
                    LastSearchUrl = SearchViewService.GetLastSearchUrl(),
                    CompareReports = compareReports,
                    CompareBasketCount = CompareViewService.BasketItemCount,
                    ShareEmailUrl =
                        CompareViewService.BasketItemCount <= CompareViewService.MaxCompareBasketShareCount
                            ? shareEmailUrl
                            : null,
                    Year = year,
                    SortAscending = CompareViewService.SortAscending,
                    SortColumn = CompareViewService.SortColumn
                });
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("~/compare-employers/{year:int=0}")]
        public IActionResult CompareEmployers(string command, int year = 0)
        {
            if (year == 0) year = SharedBusinessLogic.SharedOptions.FirstReportingDeadlineYear;

            var args = command.AfterFirst(":");
            command = command.BeforeFirst(":");

            //Clear the default back url of the employer hub pages
            EmployerBackUrl = null;
            ReportBackUrl = null;

            switch (command.ToLower())
            {
                case "employer":
                    EmployerBackUrl = RequestUrl.PathAndQuery;
                    return RedirectToAction(nameof(ViewingController.Employer), "Viewing",
                        new {employerIdentifier = args});
                case "report":
                    ReportBackUrl = RequestUrl.PathAndQuery;
                    return RedirectToAction(nameof(ViewingController.Report), "Viewing",
                        new {employerIdentifier = args, year});
            }

            return new HttpBadRequestResult($"Invalid command '{command}'");
        }

        [HttpGet("download-compare-data")]
        public async Task<IActionResult> DownloadCompareData(int year = 0)
        {
            if (year == 0) year = SharedBusinessLogic.SharedOptions.FirstReportingDeadlineYear;

            var result = await CompareEmployers(year) as ViewResult;
            var viewModel = result.Model as CompareViewModel;
            var data = viewModel?.CompareReports;

            //Ensure we some data
            if (data == null || !data.Any())
                return new HttpNotFoundResult($"There is no employer data for year {year}");

            var model = CompareBusinessLogic.GetCompareDatatable(data);

            //Setup the HTTP response
            var contentDisposition = new ContentDisposition
            {
                FileName = $"Compared GPG Data {year}-{(year + 1).ToTwoDigitYear()}.csv", Inline = false
            };
            HttpContext.SetResponseHeader("Content-Disposition", contentDisposition.ToString());

            /* No Longer required as AspNetCore has response buffering on by default
            Response.BufferOutput = true;
            */
            //Track the download 
            await TrackPageViewAsync(contentDisposition.FileName);

            //Return the data
            return Content(model.ToCSV(), "text/csv");
        }

        [HttpGet("help/{view}")]
        public IActionResult CompareHelp(string view)
        {
            if (string.IsNullOrWhiteSpace(view)) return new HttpBadRequestResult("Missing view name");

            return View($"Help/{view}");
        }

        #region Helpers

        private EmployerSearchModel GetEmployer(string employerIdentifier, bool activeOnly = true)
        {
            var employer = SearchViewService.LastSearchResults?.GetEmployer(employerIdentifier);
            if (employer == null)
            {
                //Get the employer from the database
                var organisationResult = activeOnly
                    ? OrganisationBusinessLogic.LoadInfoFromActiveEmployerIdentifier(employerIdentifier)
                    : OrganisationBusinessLogic.LoadInfoFromEmployerIdentifier(employerIdentifier);
                if (organisationResult.Failed) throw organisationResult.ErrorMessage.ToHttpException();

                if (organisationResult.Result.OrganisationId == 0)
                    throw new HttpException(HttpStatusCode.NotFound, "Employer not found");

                employer = OrganisationBusinessLogic.CreateEmployerSearchModel(organisationResult.Result);
            }

            return employer;
        }

        #endregion


    }
}