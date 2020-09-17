using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Classes.ErrorMessages;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.Viewing.Models;
using static ModernSlavery.BusinessDomain.Shared.Models.StatementModel;

namespace ModernSlavery.WebUI.Viewing.Presenters
{
    public interface IViewingPresenter
    {
        IObfuscator Obfuscator { get; }

        Task<SearchViewModel> SearchAsync(OrganisationSearchParameters searchParams);
        List<OptionSelect> GetTurnoverOptions(IEnumerable<byte> filterTurnoverRanged, Dictionary<object, long> facetReults);
        Task<List<OptionSelect>> GetSectorOptionsAsync(IEnumerable<short> filterSectorTypeIds, Dictionary<object, long> facetResults);
        PagedResult<OrganisationSearchModel> GetPagedResult(IEnumerable<OrganisationSearchModel> searchResults,long totalRecords,int page,int pageSize);
        Task<Outcome<StatementErrors, StatementViewModel>> GetStatementViewModelAsync(string organisationIdentifier, int reportingDeadlineYear);
    }

    public class ViewingPresenter : IViewingPresenter
    {
        #region Dependencies
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private readonly IStatementBusinessLogic _statementBusinessLogic;
        private readonly IViewingService _viewingService;
        public IObfuscator Obfuscator { get; }
        private readonly IMapper _mapper;
        #endregion

        #region Constructor
        public ViewingPresenter(IViewingService viewingService, IStatementBusinessLogic statementBusinessLogic, ISharedBusinessLogic sharedBusinessLogic, IObfuscator obfuscator, IMapper mapper)
        {
            _viewingService = viewingService;
            _statementBusinessLogic = statementBusinessLogic;
            _sharedBusinessLogic = sharedBusinessLogic;
            Obfuscator = obfuscator;
            _mapper = mapper;
        }
        #endregion

        #region Search methods
        public async Task<SearchViewModel> SearchAsync(OrganisationSearchParameters searchParams)
        {
            var facets = new Dictionary<string, Dictionary<object, long>>();
            facets.Add(nameof(OrganisationSearchModel.Turnover), null);
            facets.Add(nameof(OrganisationSearchModel.SectorTypeIds), null);
            facets.Add(nameof(OrganisationSearchModel.StatementDeadlineYear), null);

            var searchTermEnteredOnScreen = searchParams.Keywords;

            searchParams.Keywords = searchParams.Keywords?.Trim();
            searchParams.Keywords = searchParams.RemoveTheMostCommonTermsOnOurDatabaseFromTheKeywords();
            var searchResults = await DoSearchAsync(searchParams, facets);

            // build the result view model
            return new SearchViewModel
            {
                TurnoverOptions = GetTurnoverOptions(searchParams.FilterTurnoverRanges, facets["Turnover"]),
                SectorOptions = await GetSectorOptionsAsync(searchParams.FilterSectorTypeIds, facets["SectorTypeIds"]),
                ReportingYearOptions = GetReportingYearOptions(searchParams.FilterReportedYears),
                Organisations = searchResults,
                search = searchTermEnteredOnScreen,
                p = searchParams.Page,
                s = searchParams.FilterSectorTypeIds,
                tr = searchParams.FilterTurnoverRanges,
                y = searchParams.FilterReportedYears
            };
        }

        private async Task<PagedResult<OrganisationSearchModel>> DoSearchAsync(OrganisationSearchParameters searchParams,Dictionary<string, Dictionary<object, long>> facets)
        {
            return await _viewingService.SearchBusinessLogic.SearchDocumentsAsync(
                searchParams.Keywords, // .ToSearchQuery(),
                searchParams.Page,
                searchParams.PageSize,
                filter: searchParams.ToFilterQuery(),
                facets: facets,
                orderBy: searchParams.ToSearchCriteria(),
                searchFields: searchParams.SearchFields,
                searchMode: searchParams.SearchMode);
        }

        public PagedResult<OrganisationSearchModel> GetPagedResult(IEnumerable<OrganisationSearchModel> searchResults,long totalRecords,int page,int pageSize)
        {
            var result = new PagedResult<OrganisationSearchModel>();

            if (page == 0 || page < 0) page = 1;

            result.Results = searchResults.ToList();
            result.ActualRecordTotal = (int) totalRecords;
            result.VirtualRecordTotal = result.Results.Count;
            result.CurrentPage = page;
            result.PageSize = pageSize;

            return result;
        }
        #endregion

        #region Filter methods
        public async Task<List<SearchViewModel.SectorTypeViewModel>> GetAllSectorTypesAsync()
        {
            var results = new List<SearchViewModel.SectorTypeViewModel>();
            var sortedSectors =
                await _sharedBusinessLogic.DataRepository.ToListAscendingAsync<StatementSectorType, string>(st =>
                    st.Description);

            foreach (var sector in sortedSectors)
                results.Add(
                    new SearchViewModel.SectorTypeViewModel
                    {
                        SectorTypeId = sector.StatementSectorTypeId,
                        Description = sector.Description = sector.Description.BeforeFirst(";")
                    });

            return results;
        }

        public List<OptionSelect> GetTurnoverOptions(IEnumerable<byte> filterTurnoverRanges, Dictionary<object, long> facetResults)
        {
            var allRanges = Enum.GetValues(typeof(SearchViewModel.TurnoverRanges));

            // setup the filters
            var results = new List<OptionSelect>();
            foreach (SearchViewModel.TurnoverRanges range in allRanges)
            {
                if (range == SearchViewModel.TurnoverRanges.NotProvided) continue;
                var id = (byte)range;
                var label = range.GetAttribute<GovUkRadioCheckboxLabelTextAttribute>()?.Text;
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

        public async Task<List<OptionSelect>> GetSectorOptionsAsync(IEnumerable<short> filterSectorTypeIds, Dictionary<object, long> facetResults)
        {
            // setup the filters
            var allSectorTypes = await GetAllSectorTypesAsync();
            var sources = new List<OptionSelect>();
            foreach (var sectorType in allSectorTypes)
            {
                var isChecked = filterSectorTypeIds != null &&
                                filterSectorTypeIds.Any(x => x == sectorType.SectorTypeId);
                sources.Add(
                    new OptionSelect
                    {
                        Id = sectorType.SectorTypeId.ToString(),
                        Label = sectorType.Description.TrimEnd('\r', '\n'),
                        Value = sectorType.SectorTypeId.ToString(),
                        Checked = isChecked
                        // Disabled = facetResults.Count == 0 && !isChecked
                    });
            }

            return sources;
        }

        public List<OptionSelect> GetReportingYearOptions(IEnumerable<int> filterSnapshotYears)
        {
            // setup the filters
            var reportingDeadlines = _sharedBusinessLogic.GetReportingDeadlines(SectorTypes.Public);
            var sources = new List<OptionSelect>();
            foreach (var reportingDeadline in reportingDeadlines)
            {
                var isChecked = filterSnapshotYears != null && filterSnapshotYears.Any(x => x == reportingDeadline.Year);
                sources.Add(
                    new OptionSelect
                    {
                        Id = reportingDeadline.Year.ToString(), Label = $"{reportingDeadline.Year-1} to {reportingDeadline.Year}", Value = reportingDeadline.Year.ToString(),
                        Checked = isChecked
                        // Disabled = facetResults.Count == 0 && !isChecked
                    });
            }

            return sources;
        }

        public List<OptionSelect> GetReportingStatusOptions(IEnumerable<int> filterReportingStatus)
        {
            var allStatuses = Enum.GetValues(typeof(SearchReportingStatusFilter));

            // setup the filters
            var results = new List<OptionSelect>();
            foreach (SearchReportingStatusFilter enumEntry in allStatuses)
            {
                var id = (int) enumEntry;
                var label = enumEntry.GetAttribute<DisplayAttribute>().Name;
                var isChecked = filterReportingStatus != null && filterReportingStatus.Contains(id);
                results.Add(new OptionSelect
                    {Id = $"ReportingStatus{id}", Label = label, Value = id.ToString(), Checked = isChecked});
            }

            return results;
        }
        #endregion

        #region Statement methods
        public async Task<Outcome<StatementErrors, StatementViewModel>> GetStatementViewModelAsync(string organisationIdentifier, int reportingDeadlineYear)
        {
            long organisationId = _sharedBusinessLogic.Obfuscator.DeObfuscate(organisationIdentifier);
            var reportingDeadline = _sharedBusinessLogic.GetReportingDeadline(organisationId, reportingDeadlineYear);
            var openOutcome = await _statementBusinessLogic.GetLatestSubmittedStatementModelAsync(organisationId, reportingDeadline);
            if (openOutcome.Fail) return new Outcome<StatementErrors, StatementViewModel>(openOutcome.Errors);

            if (openOutcome.Result == null) throw new ArgumentNullException(nameof(openOutcome.Result));
            var statementModel = openOutcome.Result;

            //Copy the statement properties to the view model
            var statementViewModel=_mapper.Map<StatementViewModel>(statementModel);

            //Return the view model
            return new Outcome<StatementErrors, StatementViewModel>(statementViewModel);
        }

        #endregion
    }
}