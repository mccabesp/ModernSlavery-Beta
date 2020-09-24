using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebAPI.Models;
using ModernSlavery.WebUI.Shared.Models;

namespace ModernSlavery.WebAPI.Public.Controllers
{
    [ApiController]
    [Area("Api")]
    [Route("Api/Public")]
    public class PublicApiController : ControllerBase
    {
        private readonly ILogger<PublicApiController> _logger;
        private readonly IObfuscator _obfuscator;
        private readonly IMapper _mapper;
        private readonly IDataRepository _dataRepository;
        private readonly ISearchBusinessLogic _searchBusinessLogic;

        public PublicApiController(ILogger<PublicApiController> logger, IObfuscator obfuscator, IMapper mapper, IDataRepository dataRepository, ISearchBusinessLogic searchBusinessLogic)
        {
            _logger = logger;
            _obfuscator = obfuscator;
            _mapper = mapper;
            _dataRepository = dataRepository;
            _searchBusinessLogic = searchBusinessLogic;
        }

        /// <summary>
        /// Find statement summaries matching search criteria
        /// </summary>
        /// <param name="searchQuery">The parameters of the search</param>
        /// <returns>A list of statement summaries matching the search criteria</returns>
        [HttpGet("SearchStatementSummariesAsync")]
        public async IAsyncEnumerable<StatementSummaryViewModel> SearchStatementSummariesAsync([FromQuery] SearchQueryModel searchQuery)
        {
            //Ensure search service is enabled
            if (_searchBusinessLogic.Disabled) throw new HttpException(HttpStatusCode.ServiceUnavailable,"Service is disabled");

            // ensure parameters are valid
            if (!searchQuery.TryValidateSearchParams(out var result)) throw result.ToHttpException();

            //Execute the search
            var searchResults = await _searchBusinessLogic.SearchOrganisationsAsync(
                searchQuery.Keywords,
                searchQuery.Turnovers,
                searchQuery.Sectors,
                searchQuery.Years,
                false,
                false,
                true,
                searchQuery.PageNumber,
                searchQuery.PageSize);

            // build the result view model
            foreach (var organisationSearchModel in searchResults.Results)
                yield return _mapper.Map<StatementSummaryViewModel>(organisationSearchModel);
        }

        /// <summary>
        /// Returns a list of statement summaries submitted for specified or all years
        /// </summary>
        /// <param name="years">The list of years to include (if empty returns all reporting years)</param>
        /// <returns>A list of statement summaries for the specified years (or all years)</returns>
        [HttpGet("ListStatementSummariesAsync")]
        public async IAsyncEnumerable<StatementSummaryViewModel> ListStatementSummariesAsync([FromQuery] params int[] years)
        {
            //Ensure search service is enabled
            if (_searchBusinessLogic.Disabled) throw new HttpException(HttpStatusCode.ServiceUnavailable, "Service is disabled");

            var statementSummaryModels = SearchStatementSummariesAsync(new SearchQueryModel { Years=years });

            // build the result view model
            await foreach (var statementSummaryModel in statementSummaryModels)
                yield return statementSummaryModel;
        }

        /// <summary>
        /// Returns a list of group statement summaries for the specified organisation and year
        /// </summary>
        /// <param name="parentOrganisationId">The unique id of the parent group organisation</param>
        /// <param name="year">The year of the submitted statement</param>
        /// <returns>A list of group statement summaries for the specified organisation and year</returns>
        [HttpGet("GetStatementSummaryAsync")]
        public async Task<StatementSummaryViewModel> GetStatementSummaryAsync([FromQuery] string parentOrganisationId, int year)
        {
            //Ensure search service is enabled
            if (_searchBusinessLogic.Disabled) throw new HttpException(HttpStatusCode.ServiceUnavailable, "Service is disabled");

            //Check the parameters
            if (string.IsNullOrWhiteSpace(parentOrganisationId)) throw new HttpException(HttpStatusCode.BadRequest, $"{nameof(parentOrganisationId)} cannot be null");
            if (year == 0) throw new HttpException(HttpStatusCode.BadRequest, $"{nameof(year)} cannot be 0");

            var organisationId = _obfuscator.DeObfuscate(parentOrganisationId);

            //Get the results from the search repository
            var organisationSearchModel = await _searchBusinessLogic.GetOrganisationAsync(organisationId, year);

            // build the result view model
            return _mapper.Map<StatementSummaryViewModel>(organisationSearchModel);
        }

        /// <summary>
        /// Returns a list of group statement summaries submitted for the specified organisation and year
        /// </summary>
        /// <param name="parentOrganisationId">The unique id of the parent group organisation</param>
        /// <param name="year">The year of the submitted statement</param>
        /// <returns>A list of group statement summaries for the specified organisation and year</returns>
        [HttpGet("ListGroupStatementSummariesAsync")]
        public async IAsyncEnumerable<StatementSummaryViewModel> ListGroupStatementSummariesAsync([FromQuery] string parentOrganisationId, int year)
        {
            //Ensure search service is enabled
            if (_searchBusinessLogic.Disabled) throw new HttpException(HttpStatusCode.ServiceUnavailable, "Service is disabled");

            //Check the parameters
            if (string.IsNullOrWhiteSpace(parentOrganisationId)) throw new HttpException(HttpStatusCode.BadRequest, $"{nameof(parentOrganisationId)} cannot be null");
            if (year==0) throw new HttpException(HttpStatusCode.BadRequest, $"{nameof(year)} cannot be 0");

            var organisationId =_obfuscator.DeObfuscate(parentOrganisationId);

            //Get the results from the search repository
            var organisationSearchModels = await _searchBusinessLogic.ListGroupOrganisationsAsync(organisationId, year);

            // build the result view model
            foreach (var organisationSearchModel in organisationSearchModels)
                yield return _mapper.Map<StatementSummaryViewModel>(organisationSearchModel);
        }
    }
}
