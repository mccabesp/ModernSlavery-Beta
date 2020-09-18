using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebAPI.Public.Models;

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

        public PublicApiController(ILogger<PublicApiController> logger, IMapper mapper, IDataRepository dataRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _dataRepository = dataRepository;
        }

        /// <summary>
        /// Returns a list of statements submitted for specified or all years
        /// </summary>
        /// <param name="years">The list of years to include (if empty returns all years)</param>
        /// <returns>A list of statements for the specified years (or all years)</returns>
        [HttpGet("ListStatements")]
        public IEnumerable<StatementSummaryModel> ListStatements(params int[] years)
        {
            if (years.Length>0) return _dataRepository.GetAll<Statement>().Where(s => s.Status== StatementStatuses.Submitted && years.Contains(s.SubmissionDeadline.Year)).OrderBy(s => s.Organisation.OrganisationName).Select(s => _mapper.Map<StatementSummaryModel>(s));

            return _dataRepository.GetAll<Statement>().Where(s => s.Status == StatementStatuses.Submitted).OrderBy(s => s.SubmissionDeadline.Year).ThenBy(s => s.Organisation.OrganisationName).Select(s => _mapper.Map<StatementSummaryModel>(s));            
        }
    }
}
