using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.BusinessDomain.Viewing;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.Shared.Models;
using ModernSlavery.WebUI.Viewing.Models;
using static ModernSlavery.BusinessDomain.Shared.Models.StatementModel;
using static ModernSlavery.Core.Entities.Statement;

namespace ModernSlavery.WebUI.Viewing.Presenters
{
    public interface IViewingPresenter
    {
        IObfuscator Obfuscator { get; }
        Task<SearchViewModel> SearchAsync(SearchQueryModel searchQuery);
        Task<Outcome<StatementErrors, StatementViewModel>> GetStatementViewModelAsync(string organisationIdentifier, int reportingDeadlineYear);
        Task<Outcome<StatementErrors, StatementSummaryViewModel>> GetStatementSummaryViewModel(string organisationIdentifier, int reportingDeadlineYear);
        Task<Outcome<StatementErrors, List<StatementSummaryViewModel>>> GetStatementSummaryGroupViewModel(string organisationIdentifier, int reportingDeadlineYear);
        SearchViewModel GetSearchViewModel(SearchQueryModel searchQuery);
        Task<Outcome<StatementErrors, string>> GetLinkRedirectUrl(string organisationIdentifier, int reportingDeadlineYear);
    }

    public class ViewingPresenter : IViewingPresenter
    {
        #region Dependencies
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private readonly IStatementBusinessLogic _statementBusinessLogic;
        private readonly IViewingService _viewingService;
        private readonly IUrlChecker _urlChecker;
        public IObfuscator Obfuscator { get; }
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISearchBusinessLogic _searchBusinessLogic;
        private readonly SectorTypeIndex _sectorTypes;
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
            SectorTypeIndex sectorTypes)
        {
            _viewingService = viewingService;
            _statementBusinessLogic = statementBusinessLogic;
            _sharedBusinessLogic = sharedBusinessLogic;
            Obfuscator = obfuscator;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
            _sectorTypes = sectorTypes;
            _searchBusinessLogic = searchBusinessLogic;
            _urlChecker = urlChecker;
        }
        #endregion

        #region Search methods

        public SearchViewModel GetSearchViewModel(SearchQueryModel searchQuery)
        {
            return new SearchViewModel
            {
                TurnoverOptions = GetTurnoverOptions(searchQuery.Turnovers),
                SectorOptions = GetSectorOptions(searchQuery.Sectors),
                ReportingYearOptions = GetReportingYearOptions(searchQuery.Years),
                Keywords = searchQuery.Keywords,
                Sectors = searchQuery.Sectors,
                Turnovers = searchQuery.Turnovers,
                Years = searchQuery.Years
            };
        }

        public async Task<SearchViewModel> SearchAsync(SearchQueryModel searchQuery)
        {
            //Execute the search
            var searchResults = await _viewingService.SearchBusinessLogic.SearchOrganisationsAsync(
                searchQuery.Keywords,
                searchQuery.Turnovers,
                searchQuery.Sectors,
                searchQuery.Years,
                true,
                false,
                false,
                searchQuery.PageNumber,
                searchQuery.PageSize);

            // build the result view model
            return new SearchViewModel
            {
                TurnoverOptions = GetTurnoverOptions(searchQuery.Turnovers),
                SectorOptions = GetSectorOptions(searchQuery.Sectors),
                ReportingYearOptions = GetReportingYearOptions(searchQuery.Years),
                Organisations = searchResults,
                Keywords = searchQuery.Keywords,
                PageNumber = searchQuery.PageNumber,
                Sectors = searchQuery.Sectors,
                Turnovers = searchQuery.Turnovers,
                Years = searchQuery.Years
            };
        }

        #endregion

        #region Filter methods
        public List<OptionSelect> GetTurnoverOptions(IEnumerable<byte> filterTurnoverRanges)
        {
            var allRanges = Enums.GetValues<StatementTurnoverRanges>();

            // setup the filters
            var results = new List<OptionSelect>();
            foreach (var range in allRanges)
            {
                if (range == StatementTurnoverRanges.NotProvided) continue;
                var id = (byte)range;
                var label = range.GetEnumDescription();
                var isChecked = filterTurnoverRanges != null && filterTurnoverRanges.Contains(id);
                results.Add(
                    new OptionSelect
                    {
                        Id = $"Turnover{id}",
                        Label = label,
                        Value = id.ToString(),
                        Checked = isChecked
                        // Disabled = facetResults.Count == 0 && !isChecked
                    });
            }

            return results;
        }

        public List<OptionSelect> GetSectorOptions(IEnumerable<short> filterSectorTypeIds)
        {
            // setup the filters
            var sources = new List<OptionSelect>();
            foreach (var sectorType in _sectorTypes)
            {
                sources.Add(
                    new OptionSelect
                    {
                        Id = sectorType.Id.ToString(),
                        Label = sectorType.Description.TrimEnd('\r', '\n'),
                        Value = sectorType.Id.ToString(),
                        Checked = filterSectorTypeIds != null && filterSectorTypeIds.Any(x => x == sectorType.Id)
                    });
            }

            return sources;
        }

        public List<OptionSelect> GetReportingYearOptions(IEnumerable<int> filterSnapshotYears)
        {
            // setup the filters
            var reportingDeadlines = _sharedBusinessLogic.ReportingDeadlineHelper.GetReportingDeadlines(SectorTypes.Public);
            var sources = new List<OptionSelect>();
            foreach (var reportingDeadline in reportingDeadlines)
            {
                var isChecked = filterSnapshotYears != null && filterSnapshotYears.Any(x => x == reportingDeadline.Year);
                sources.Add(
                    new OptionSelect
                    {
                        Id = reportingDeadline.Year.ToString(),
                        Label = reportingDeadline.Year.ToString(),
                        Value = reportingDeadline.Year.ToString(),
                        Checked = isChecked
                        // Disabled = facetResults.Count == 0 && !isChecked
                    });
            }

            return sources;
        }
        #endregion

        #region Statement methods
        public async Task<Outcome<StatementErrors, StatementViewModel>> GetStatementViewModelAsync(string organisationIdentifier, int reportingDeadlineYear)
        {
            long organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var openOutcome = await _statementBusinessLogic.GetLatestSubmittedStatementModelAsync(organisationId, reportingDeadlineYear);
            if (openOutcome.Fail) return new Outcome<StatementErrors, StatementViewModel>(openOutcome.Errors);

            if (openOutcome.Result == null) throw new ArgumentNullException(nameof(openOutcome.Result));
            var statementModel = openOutcome.Result;

            //Copy the statement properties to the view model
            var viewModel = ActivatorUtilities.CreateInstance<StatementViewModel>(_serviceProvider);
            var statementViewModel = _mapper.Map(statementModel, viewModel);

            //Return the view model
            return new Outcome<StatementErrors, StatementViewModel>(statementViewModel);
        }

        public async Task<Outcome<StatementErrors, StatementSummaryViewModel>> GetStatementSummaryViewModel(string organisationIdentifier, int reportingDeadlineYear)
        {
            long organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var organisationSearchModel = await _searchBusinessLogic.GetOrganisationAsync(organisationId, reportingDeadlineYear);

            if (organisationSearchModel == null)
                return new Outcome<StatementErrors, StatementSummaryViewModel>(StatementErrors.NotFound, $"Cannot find statement summary for Organisation:{organisationId} due for reporting deadline year {reportingDeadlineYear}");

            var vm = _mapper.Map<StatementSummaryViewModel>(organisationSearchModel);
            return new Outcome<StatementErrors, StatementSummaryViewModel>(vm);
        }

        public async Task<Outcome<StatementErrors, List<StatementSummaryViewModel>>> GetStatementSummaryGroupViewModel(string organisationIdentifier, int reportingDeadlineYear)
        {
            long organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var groups = await _searchBusinessLogic.ListGroupOrganisationsAsync(organisationId, reportingDeadlineYear);

            if (!groups.Any())
                return new Outcome<StatementErrors, List<StatementSummaryViewModel>>(StatementErrors.NotFound, $"Cannot find statement summary for Organisation:{organisationId} due for reporting deadline year {reportingDeadlineYear}");

            var vm = _mapper.Map<List<StatementSummaryViewModel>>(groups);
            return new Outcome<StatementErrors, List<StatementSummaryViewModel>>(vm);
        }

        public async Task<Outcome<StatementErrors, string>> GetLinkRedirectUrl(string organisationIdentifier, int reportingDeadlineYear)
        {
            var organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var organisationSearchModel = await _searchBusinessLogic.GetOrganisationAsync(organisationId, reportingDeadlineYear);

            if (organisationSearchModel == null)
                return new Outcome<StatementErrors, string>(StatementErrors.NotFound, $"Cannot find statement summary for Organisation:{organisationId} due for reporting deadline year {reportingDeadlineYear}");

            var isWorking = await _urlChecker.IsUrlWorking(organisationSearchModel.StatementUrl);

            if (isWorking)
                return new Outcome<StatementErrors, string>(organisationSearchModel.StatementUrl);
            else
                return new Outcome<StatementErrors, string>(result: null);
        }

        #endregion
    }
}