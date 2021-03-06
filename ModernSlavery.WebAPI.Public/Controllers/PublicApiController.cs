﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebAPI.Models;
using ModernSlavery.WebAPI.Public.Classes;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using Swashbuckle.AspNetCore.Filters;

namespace ModernSlavery.WebAPI.Public.Controllers
{
    [ApiController]
    [ApiControllerExceptionFilter]
    [ResponseCache(CacheProfileName = "PublicApi")]
    [Area("Api")]
    [Route("Api/Public")]
    [Produces("application/json", "text/json", "application/csv", "text/csv", "application/xml", "text/xml")]
    public class PublicApiController : ControllerBase
    {
        private readonly ILogger<PublicApiController> _logger;
        private readonly IObfuscator _obfuscator;
        private readonly IMapper _mapper;
        private readonly IDataRepository _dataRepository;
        private readonly ISearchBusinessLogic _searchBusinessLogic;
        private readonly IMemoryCache _memoryCache;

        public PublicApiController(ILogger<PublicApiController> logger, IObfuscator obfuscator, IMapper mapper, IDataRepository dataRepository, ISearchBusinessLogic searchBusinessLogic, IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
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
        [HttpGet("SearchStatementSummaries")]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(ListModelExample<StatementSummaryDownloadModel>))]
        public async IAsyncEnumerable<StatementSummaryDownloadModel> SearchStatementSummariesAsync([FromQuery] SearchQueryModel searchQuery)
        {
            //Ensure search service is enabled
            if (_searchBusinessLogic.Disabled) throw new HttpException(HttpStatusCode.ServiceUnavailable,"Service is disabled");

            // ensure parameters are valid
            if (!searchQuery.TryValidateSearchParams(out var result)) throw result.ToHttpException();

            //Ensure the years are in 4 digits
            searchQuery.Years = searchQuery.Years?.Select(y => y.ToTwoDigitYear()).ToArray();

            //Execute the search
            var searchResults = await _searchBusinessLogic.SearchOrganisationsAsync(
                searchQuery.Keywords,
                searchQuery.Turnovers,
                searchQuery.Sectors,
                searchQuery.Years,
                false,
                false,
                searchQuery.PageNumber,
                searchQuery.PageSize);

            //Set the results for a download if browser accepts mime types of "application/*"
            SetDownloadDisposition($"SearchResults");

            // build the result view model
            foreach (var organisationSearchModel in searchResults.Results)
                yield return _mapper.Map<StatementSummaryDownloadModel>(organisationSearchModel);
        }

        /// <summary>
        /// Returns a list of statement summaries submitted for specified or all years
        /// </summary>
        /// <param name="years">The list of years to include (if empty returns all reporting years)</param>
        /// <returns>A list of statement summaries for the specified years (or all years)</returns>
        [HttpGet("ListStatementSummaries")]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(ListModelExample<StatementSummaryDownloadModel>))]
        public async IAsyncEnumerable<StatementSummaryDownloadModel> ListStatementSummariesAsync([FromQuery] params int[] years)
        {
            //Ensure search service is enabled
            if (_searchBusinessLogic.Disabled) throw new HttpException(HttpStatusCode.ServiceUnavailable, "Service is disabled");

            //Ensure the years are in 4 digits
            years = years?.Select(y => y.ToFourDigitYear()).ToArray();

            //Calculate the filename
            string fileName;
            if (years.Count()>1)
                fileName=$"StatementSummaries-{years[0]}-{years[years.Length - 1].ToString().Substring(2)}";
            else if (years.Any())
                fileName=$"StatementSummary-{years[0]}";
            else
                fileName=$"StatementSummaries {years.ToDelimitedString(" ")}";

            //Retrieve the results from the cache or search service
            var organisationSearchModels = GetStatementSummariesFromCache(fileName);
            if (organisationSearchModels == null)
            {
                organisationSearchModels = await _searchBusinessLogic.ListSearchDocumentsAsync(years);
                SetStatementSummariesToCache(fileName, organisationSearchModels);
            }

            //Set the results for a download if browser accepts mime types of "application/*"
            SetDownloadDisposition(fileName);

            // build the result view model
            foreach (var organisationSearchModel in organisationSearchModels)
                yield return _mapper.Map<StatementSummaryDownloadModel>(organisationSearchModel);
        }

        /// <summary>
        /// Returns a file containing a list of statement summaries submitted for between the specified year in a specified format
        /// </summary>
        /// <param name="fromYear">The first reporting year</param>
        /// <param name="toYear">The last reporting year</param>
        /// <param name="extension">The file type to return (i.e., 'json', 'csv' or 'xml')</param>
        /// <returns></returns>
        [HttpGet("StatementSummaries{fromYear}-{toYear}.{extension}")]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(ListModelExample<StatementSummaryDownloadModel>))]
        public async IAsyncEnumerable<StatementSummaryDownloadModel> ListStatementSummariesAsync(int fromYear, int toYear, [IgnoreText]string extension)
        {
            //Ensure search service is enabled
            if (_searchBusinessLogic.Disabled) throw new HttpException(HttpStatusCode.ServiceUnavailable, "Service is disabled");

            //Check the parameters
            if (fromYear == 0) throw new HttpException(HttpStatusCode.BadRequest, $"{nameof(fromYear)} cannot be 0");
            if (toYear == 0) throw new HttpException(HttpStatusCode.BadRequest, $"{nameof(toYear)} cannot be 0");
            if (fromYear>=toYear) throw new HttpException(HttpStatusCode.BadRequest, $"{nameof(fromYear)} must be less than {nameof(toYear)}");

            fromYear = fromYear.ToFourDigitYear();
            toYear = toYear.ToFourDigitYear();

            switch (extension?.ToLower())
            {
                case "json":
                case "csv":
                case "xml":
                    Request.Headers["accept"] = $"application/{extension}";
                    break;
                default:
                    throw new HttpException(HttpStatusCode.BadRequest, $"{nameof(extension)} must be 'json', 'csv' or 'xml'");
            }

            var years = Enumerable.Range(fromYear, (toYear-fromYear)+1).ToList();
            var statementSummaryModels = ListStatementSummariesAsync(years.ToArray());

            // build the result view model
            await foreach (var statementSummaryModel in statementSummaryModels)
                yield return statementSummaryModel;
        }

        /// <summary>
        /// Returns a file containing a list of statement summaries submitted for the specified year in a specified format
        /// </summary>
        /// <param name="year">The reporting year</param>
        /// <param name="extension">The file type to return (i.e., 'json', 'csv' or 'xml')</param>
        /// <returns></returns>
        [HttpGet("StatementSummaries{year}.{extension}")]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(ListModelExample<StatementSummaryDownloadModel>))]
        public async IAsyncEnumerable<StatementSummaryDownloadModel> ListStatementSummariesAsync(int year, [IgnoreText]string extension)
        {
            //Ensure search service is enabled
            if (_searchBusinessLogic.Disabled) throw new HttpException(HttpStatusCode.ServiceUnavailable, "Service is disabled");

            //Check the parameters
            if (year == 0) throw new HttpException(HttpStatusCode.BadRequest, $"{nameof(year)} cannot be 0");

            year = year.ToFourDigitYear();

            switch (extension?.ToLower())
            {
                case "json":
                case "csv":
                case "xml":
                    Request.Headers["accept"] = $"application/{extension}";
                    break;
                default:
                    throw new HttpException(HttpStatusCode.BadRequest, $"{nameof(extension)} must be 'json', 'csv' or 'xml'");
            }

            var statementSummaryModels = ListStatementSummariesAsync(new [] { year });

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
        [HttpGet("GetStatementSummary")]
        public async Task<StatementSummaryDownloadModel> GetStatementSummaryAsync([FromQuery][Obfuscated] string parentOrganisationId, int year)
        {
            //Ensure search service is enabled
            if (_searchBusinessLogic.Disabled) throw new HttpException(HttpStatusCode.ServiceUnavailable, "Service is disabled");

            //Check the parameters
            if (string.IsNullOrWhiteSpace(parentOrganisationId)) throw new HttpException(HttpStatusCode.BadRequest, $"{nameof(parentOrganisationId)} cannot be null");
            if (year == 0) throw new HttpException(HttpStatusCode.BadRequest, $"{nameof(year)} cannot be 0");

            var organisationId = _obfuscator.DeObfuscate(parentOrganisationId);

            //Get the results from the search repository
            var organisationSearchModel = await _searchBusinessLogic.GetOrganisationAsync(organisationId, year);

            //Set the results for a download if browser accepts mime types of "application/*"
            SetDownloadDisposition($"StatementSummary-{parentOrganisationId}-{year}");

            // build the result view model
            return _mapper.Map<StatementSummaryDownloadModel>(organisationSearchModel);
        }

        /// <summary>
        /// Returns a list of group statement summaries submitted for the specified organisation and year
        /// </summary>
        /// <param name="parentOrganisationId">The unique id of the parent group organisation</param>
        /// <param name="year">The year of the submitted statement</param>
        /// <returns>A list of group statement summaries for the specified organisation and year</returns>
        [HttpGet("ListGroupStatementSummaries")]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(ListModelExample<StatementSummaryDownloadModel>))]
        public async IAsyncEnumerable<StatementSummaryDownloadModel> ListGroupStatementSummariesAsync([FromQuery][Obfuscated] string parentOrganisationId, int year)
        {
            //Ensure search service is enabled
            if (_searchBusinessLogic.Disabled) throw new HttpException(HttpStatusCode.ServiceUnavailable, "Service is disabled");

            //Check the parameters
            if (string.IsNullOrWhiteSpace(parentOrganisationId)) throw new HttpException(HttpStatusCode.BadRequest, $"{nameof(parentOrganisationId)} cannot be null");
            if (year==0) throw new HttpException(HttpStatusCode.BadRequest, $"{nameof(year)} cannot be 0");

            var organisationId =_obfuscator.DeObfuscate(parentOrganisationId);

            //Get the results from the search repository
            var organisationSearchModels = await _searchBusinessLogic.ListGroupOrganisationsAsync(organisationId, year);

            //Set the results for a download if browser accepts mime types of "application/*"
            SetDownloadDisposition($"GroupStatementSummaries-{parentOrganisationId}-{year}");

            // build the result view model
            foreach (var organisationSearchModel in organisationSearchModels)
                yield return _mapper.Map<StatementSummaryDownloadModel>(organisationSearchModel);
        }

        //Sets the content disposition for a download if browser accepting application/* mime type
        private void SetDownloadDisposition(string filename)
        {
            var mimeType=Request.Headers["accept"].FirstOrDefault(h => h.StartsWithI("application"));
            if (mimeType == null) return;

            filename = $"{Path.GetFileNameWithoutExtension(filename)}.{mimeType.AfterFirst("/")}";

            System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition
            {
                FileName = filename,
                Inline = false  // false = prompt the user for downloading;  true = browser to try to show the file inline
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
        }


        private IEnumerable<OrganisationSearchModel> GetStatementSummariesFromCache(string fileName)
        {
            return _memoryCache.TryGetValue(fileName.ToLower(), out IEnumerable<OrganisationSearchModel> statementSummaries) ? statementSummaries : null;
        }

        private MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(60));
        private void SetStatementSummariesToCache(string fileName, IEnumerable<OrganisationSearchModel> statementSummaries)
        {
            // Save data in cache.
            _memoryCache.Set(fileName.ToLower(), statementSummaries, _cacheEntryOptions);
        }
    }
}
