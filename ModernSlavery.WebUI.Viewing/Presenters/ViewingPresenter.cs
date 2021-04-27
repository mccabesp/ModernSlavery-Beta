using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Options;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Viewing.Models;

namespace ModernSlavery.WebUI.Viewing.Presenters
{
    public interface IViewingPresenter
    {
        Task SearchAsync(SearchViewModel viewModel);
        Task<Outcome<StatementErrors, StatementSummaryViewModel>> GetStatementSummaryViewModel(long organisationId, int reportingDeadlineYear);
        Task<Outcome<StatementErrors, List<StatementSummaryViewModel>>> GetStatementSummaryGroupViewModel(long organisationId, int reportingDeadlineYear);
        Task<Outcome<StatementErrors, string>> GetLinkRedirectUrl(long organisationId, int reportingDeadlineYear);

        /// <summary>
        /// Checks if a statement has changed since the last modified date and returns Http 304 (Not Modified)
        /// Also sets the last modified header to the status date of the statement
        /// </summary>
        /// <param name="httpContext">The current http context to get/set the caching flags from</param>
        /// <param name="organisationId">The unique id of the organisation</param>
        /// <param name="reportingDeadlineYear"></param>
        /// <returns></returns>
        IActionResult CheckStatementModified(HttpContext httpContext, long organisationId, int reportingDeadlineYear, int cacheSeconds = 3600);
    }

    public class ViewingPresenter : IViewingPresenter
    {
        #region Dependencies
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private readonly IStatementBusinessLogic _statementBusinessLogic;
        private readonly IViewingService _viewingService;
        private readonly IUrlChecker _urlChecker;
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISearchBusinessLogic _searchBusinessLogic;
        private readonly TestOptions _testOptions;
        #endregion

        #region Constructor
        public ViewingPresenter(IViewingService viewingService,
            IStatementBusinessLogic statementBusinessLogic,
            ISharedBusinessLogic sharedBusinessLogic,
            IObfuscator obfuscator,
            IServiceProvider serviceProvider,
            IMapper mapper,
            ISearchBusinessLogic searchBusinessLogic,
            IUrlChecker urlChecker,
            TestOptions testOptions)
        {
            _viewingService = viewingService;
            _statementBusinessLogic = statementBusinessLogic;
            _sharedBusinessLogic = sharedBusinessLogic;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
            _searchBusinessLogic = searchBusinessLogic;
            _urlChecker = urlChecker;
            _testOptions = testOptions;
        }
        #endregion

        #region Search methods

        public async Task SearchAsync(SearchViewModel viewModel)
        {
            //Execute the search
            viewModel.Organisations = await _viewingService.SearchBusinessLogic.SearchOrganisationsAsync(
                viewModel.Search,
                viewModel.Turnovers,
                viewModel.Sectors,
                viewModel.Years,
                false,
                false,
                viewModel.PageNumber,
                viewModel.PageSize);
        }

        #endregion

        #region Statement methods

        /// <summary>
        /// Note: This wont work as CSP nonce - breaks javascript when response is cached
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="organisationId"></param>
        /// <param name="reportingDeadlineYear"></param>
        /// <param name="cacheSeconds"></param>
        /// <returns></returns>
        public IActionResult CheckStatementModified(HttpContext httpContext, long organisationId, int reportingDeadlineYear, int cacheSeconds=3600)
        {
            //Net No-Check
            httpContext.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue {
                NoCache = true
            };

            var statement = _sharedBusinessLogic.DataRepository.GetAll<Statement>().FirstOrDefault(s => s.OrganisationId == organisationId && s.SubmissionDeadline.Year == reportingDeadlineYear && s.Status == StatementStatuses.Submitted);
            if (statement != null)
            {
                var lastSubmitted = new DateTime(statement.StatusDate.Year, statement.StatusDate.Month, statement.StatusDate.Day, statement.StatusDate.Hour, statement.StatusDate.Minute, statement.StatusDate.Second);

                var ifModifiedSince = httpContext.Request.GetTypedHeaders().IfModifiedSince;
                if (ifModifiedSince != null && ifModifiedSince.HasValue)
                {
                    if (lastSubmitted <= ifModifiedSince.Value && ifModifiedSince.Value.AddSeconds(cacheSeconds) > DateTime.Now)
                    {
                            httpContext.Response.GetTypedHeaders().LastModified = ifModifiedSince.Value.LocalDateTime;
                            return new StatusCodeResult(StatusCodes.Status304NotModified);
                        }
                }
            }

            //Set the last modified date to when the statement was last submitted
            httpContext.Response.GetTypedHeaders().LastModified = DateTime.Now;

            return null;
        }

        public async Task<Outcome<StatementErrors, StatementSummaryViewModel>> GetStatementSummaryViewModel(long organisationId, int reportingDeadlineYear)
        {
            var organisationSearchModel = await _searchBusinessLogic.GetOrganisationAsync(organisationId, reportingDeadlineYear);

            if (organisationSearchModel == null)
                return new Outcome<StatementErrors, StatementSummaryViewModel>(StatementErrors.NotFound, $"Cannot find statement summary for Organisation:{organisationId} due for reporting deadline year {reportingDeadlineYear}");

            var vm = _mapper.Map<StatementSummaryViewModel>(organisationSearchModel);
            return new Outcome<StatementErrors, StatementSummaryViewModel>(vm);
        }

        public async Task<Outcome<StatementErrors, List<StatementSummaryViewModel>>> GetStatementSummaryGroupViewModel(long organisationId, int reportingDeadlineYear)
        {
            var groups = await _searchBusinessLogic.ListGroupOrganisationsAsync(organisationId, reportingDeadlineYear);

            if (groups==null || !groups.Any())
                return new Outcome<StatementErrors, List<StatementSummaryViewModel>>(StatementErrors.NotFound, $"Cannot find statement summary for Organisation:{organisationId} due for reporting deadline year {reportingDeadlineYear}");

            var vm = _mapper.Map<List<StatementSummaryViewModel>>(groups);
            return new Outcome<StatementErrors, List<StatementSummaryViewModel>>(vm);
        }

        public async Task<Outcome<StatementErrors, string>> GetLinkRedirectUrl(long organisationId, int reportingDeadlineYear)
        {
            var organisationSearchModel = await _searchBusinessLogic.GetOrganisationAsync(organisationId, reportingDeadlineYear);

            if (organisationSearchModel == null)
                return new Outcome<StatementErrors, string>(StatementErrors.NotFound, $"Cannot find statement summary for Organisation:{organisationId} due for reporting deadline year {reportingDeadlineYear}");

            var isWorking = _testOptions.LoadTesting || await _urlChecker.IsUrlWorkingAsync(organisationSearchModel.StatementUrl);

            if (isWorking)
                return new Outcome<StatementErrors, string>(organisationSearchModel.StatementUrl);
            else
                return new Outcome<StatementErrors, string>(result: null);
        }

        #endregion
    }
}