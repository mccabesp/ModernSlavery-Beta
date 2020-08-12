using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
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

        PagedResult<OrganisationSearchModel> GetPagedResult(IEnumerable<OrganisationSearchModel> searchResults,
            long totalRecords,
            int page,
            int pageSize);

        Task<List<SuggestEmployerResult>> SuggestEmployerNameAsync(string search);
    }

    public class ViewingPresenter : IViewingPresenter
    {
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private readonly IViewingService _viewingService;
        public IObfuscator Obfuscator { get; }

        public ViewingPresenter(IViewingService viewingService, ISharedBusinessLogic sharedBusinessLogic, IObfuscator obfuscator)
        {
            _viewingService = viewingService;
            _sharedBusinessLogic = sharedBusinessLogic;
            Obfuscator = obfuscator;
        }

        public async Task<SearchViewModel> SearchAsync(OrganisationSearchParameters searchParams)
        {
            var facets = new Dictionary<string, Dictionary<object, long>>();
            facets.Add("Turnover", null);
            facets.Add("SectorTypeIds", null);
            facets.Add("StatementYears", null);

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
                Employers = searchResults,
                search = searchTermEnteredOnScreen,
                p = searchParams.Page,
                s = searchParams.FilterSectorTypeIds,
                tr = searchParams.FilterTurnoverRanges,
                y = searchParams.FilterReportedYears
            };
        }

        public async Task<List<SuggestEmployerResult>> SuggestEmployerNameAsync(string searchText)
        {
            var results = await _viewingService.SearchBusinessLogic.OrganisationSearchRepository.SuggestAsync(
                searchText,
                $"{nameof(OrganisationSearchModel.Name)};{nameof(OrganisationSearchModel.PreviousName)};{nameof(OrganisationSearchModel.Abbreviations)}");

            var matches = new List<SuggestEmployerResult>();
            foreach (var result in results)
            {
                //Ensure all names in suggestions are unique
                if (matches.Any(m => m.Text == result.Value.Name)) continue;

                matches.Add(
                    new SuggestEmployerResult
                    {
                        Id = Obfuscator.Obfuscate(result.Value.OrganisationId), Text = result.Value.Name,
                        PreviousName = result.Value.PreviousName
                    });
            }

            return matches;
        }

        public PagedResult<OrganisationSearchModel> GetPagedResult(IEnumerable<OrganisationSearchModel> searchResults,
            long totalRecords,
            int page,
            int pageSize)
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

        public List<OptionSelect> GetTurnoverOptions(IEnumerable<byte> filterTurnoverRanges,
            Dictionary<object, long> facetResults)
        {
            var allRanges = Enum.GetValues(typeof(TurnoverRanges));

            // setup the filters
            var results = new List<OptionSelect>();
            foreach (TurnoverRanges range in allRanges)
            {
                var id = (byte) range;
                var label = range.GetAttribute<DisplayAttribute>().Name;
                var isChecked = filterTurnoverRanges != null && filterTurnoverRanges.Contains(id);
                results.Add(
                    new OptionSelect
                    {
                        Id = $"Turnover{id}", Label = label, Value = id.ToString(), Checked = isChecked
                        // Disabled = facetResults.Count == 0 && !isChecked
                    });
            }

            return results;
        }

        public async Task<List<OptionSelect>> GetSectorOptionsAsync(IEnumerable<short> filterSectorTypeIds,Dictionary<object, long> facetResults)
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

        private async Task<PagedResult<OrganisationSearchModel>> DoSearchAsync(OrganisationSearchParameters searchParams,
            Dictionary<string, Dictionary<object, long>> facets)
        {
            return await _viewingService.SearchBusinessLogic.OrganisationSearchRepository.SearchAsync(
                searchParams.Keywords, // .ToSearchQuery(),
                searchParams.Page,
                searchParams.PageSize,
                filter: searchParams.ToFilterQuery(),
                facets: facets,
                orderBy: string.IsNullOrWhiteSpace(searchParams.Keywords) ? nameof(OrganisationSearchModel.Name) : null,
                searchFields: searchParams.SearchFields,
                searchMode: searchParams.SearchMode);
        }

        public List<OptionSelect> GetReportingYearOptions(IEnumerable<int> filterSnapshotYears)
        {
            // setup the filters
            var firstYear = _sharedBusinessLogic.SharedOptions.FirstReportingDeadlineYear;
            var currentYear = _sharedBusinessLogic.GetReportingStartDate(SectorTypes.Public).Year;
            var allYears = new List<int>();
            for (var year = firstYear; year <= currentYear; year++) allYears.Add(year);

            var sources = new List<OptionSelect>();
            for (var year = currentYear; year >= firstYear; year--)
            {
                var isChecked = filterSnapshotYears != null && filterSnapshotYears.Any(x => x == year);
                sources.Add(
                    new OptionSelect
                    {
                        Id = year.ToString(), Label = $"{year} to {year + 1}", Value = year.ToString(),
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
    }
}