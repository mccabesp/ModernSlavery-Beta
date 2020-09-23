﻿using System;
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
using ModernSlavery.Core.Models;
using ModernSlavery.WebAPI.Models;

namespace ModernSlavery.WebAPI.Public.Controllers
{
    [ApiController]
    [Area("Api")]
    [Route("Api/Public")]
    public class PublicApiController : ControllerBase
    {
        private readonly ILogger<PublicApiController> _logger;
        private readonly IMapper _mapper;
        private readonly IDataRepository _dataRepository;
        private readonly ISearchBusinessLogic _searchBusinessLogic;

        public PublicApiController(ILogger<PublicApiController> logger, IMapper mapper, IDataRepository dataRepository, ISearchBusinessLogic searchBusinessLogic)
        {
            _logger = logger;
            _mapper = mapper;
            _dataRepository = dataRepository;
            _searchBusinessLogic = searchBusinessLogic;
        }

        /// <summary>
        /// Search for registered organisations
        /// </summary>
        /// <param name="searchQuery">The parameters of the search</param>
        /// <returns>A list of organisations</returns>
        [HttpGet("SearchOrganisations")]
        public async IAsyncEnumerable<StatementSummaryModel> SearchOrganisations([FromQuery] SearchQueryModel searchQuery)
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
                yield return _mapper.Map<StatementSummaryModel>(organisationSearchModel);
        }

        /// <summary>
        /// Returns a list of statements submitted for specified or all years
        /// </summary>
        /// <param name="years">The list of years to include (if empty returns all years)</param>
        /// <returns>A list of statements for the specified years (or all years)</returns>
        [HttpGet("ListStatements")]
        public async IAsyncEnumerable<StatementSummaryModel> ListStatements([FromQuery] params int[] years)
        {
            var statementSummaryModels = SearchOrganisations(new SearchQueryModel { Years=years });

            // build the result view model
            await foreach (var statementSummaryModel in statementSummaryModels)
                yield return statementSummaryModel;
        }
    }
}
