using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Models;
using ModernSlavery.WebAPI.Public.Models;

namespace ModernSlavery.WebAPI.Public.Controllers
{
    [ApiController]
    [Area("Api")]
    [Route("Api/Public")]
    public class PublicApiController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<PublicApiController> _logger;

        public PublicApiController(ILogger<PublicApiController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecastModel> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecastModel
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        /// <summary>
        /// Search for registered organisations
        /// </summary>
        /// <param name="searchQuery">The parameters of the search</param>
        /// <returns>A list of organisations</returns>
        [HttpGet("SearchOrganisations")]
        public IEnumerable<OrganisationSearchModel> SearchOrganisations([FromQuery] SearchResultsQuery searchQuery)
        {
            return null;
        }

        /// <summary>
        /// Returns detailed information on a particular organisation
        /// </summary>
        /// <param name="organisationIdentifier">The unique identifier of the organisation</param>
        /// <returns>Detailed information on a the specified organisation</returns>
        [HttpGet("GetOrganisation/{organisationIdentifier}")]
        public StatementViewModel GetOrganisation(string organisationIdentifier)
        {
            return null;
        }

        /// <summary>
        /// Returns a list of statements submitted by the organisation
        /// </summary>
        /// <param name="organisationIdentifier">The unique identifier of the organisation</param>
        /// <param name="years">The list of years to include (if empty returns all years)</param>
        /// <returns>A list of statements for the specified organisation and years</returns>
        [HttpGet("ListStatements")]
        public IEnumerable<StatementViewModel> GetStatements(string organisationIdentifier, params int[] years)
        {
            return null;
        }
    }
}
