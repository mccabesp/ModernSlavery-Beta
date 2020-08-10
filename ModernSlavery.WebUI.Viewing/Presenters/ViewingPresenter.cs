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
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Viewing.Models;

namespace ModernSlavery.WebUI.Viewing.Presenters
{
    public interface IViewingPresenter
    {
        Task<SearchViewModel> SearchAsync(EmployerSearchParameters searchParams);
        Task<List<SearchViewModel.SicSection>> GetAllSicSectionsAsync();
        List<OptionSelect> GetOrgSizeOptions(IEnumerable<int> filterOrgSizes, Dictionary<object, long> facetReults);

        Task<List<OptionSelect>> GetSectorOptionsAsync(IEnumerable<char> filterSicSectionIds,
            Dictionary<object, long> facetReults);

        PagedResult<EmployerSearchModel> GetPagedResult(IEnumerable<EmployerSearchModel> searchResults,
            long totalRecords,
            int page,
            int pageSize);

        Task<List<SuggestEmployerResult>> SuggestEmployerNameAsync(string search);
        Task<List<SicCodeSearchResult>> GetListOfSicCodeSuggestionsAsync(string search);
    }

    public class ViewingPresenter : IViewingPresenter
    {
        private readonly ISharedBusinessLogic _sharedBusinessLogic;
        private readonly IViewingService _viewingService;

        public ViewingPresenter(IViewingService viewingService, ISharedBusinessLogic sharedBusinessLogic)
        {
            _viewingService = viewingService;
            _sharedBusinessLogic = sharedBusinessLogic;
        }

        public async Task<SearchViewModel> SearchAsync(EmployerSearchParameters searchParams)
        {
            var searchResults = new PagedResult<EmployerSearchModel>();

            var facets = new Dictionary<string, Dictionary<object, long>>();
            facets.Add("Size", null);
            facets.Add("SicSectionIds", null);
            facets.Add("ReportedYears", null);
            facets.Add("ReportedLateYears", null);
            facets.Add("ReportedExplanationYears", null);

            var searchTermEnteredOnScreen = searchParams.Keywords;

            if (searchParams.SearchType == SearchTypes.BySectorType)
            {
                var list =
                    await GetListOfSicCodeSuggestionsFromIndexAsync(searchParams.Keywords);
                searchParams.FilterCodeIds = list.Select(x => int.Parse(x.Value.SicCodeId));

                #region Log the search

                if (!string.IsNullOrEmpty(searchParams.Keywords))
                {
                    var detailedListOfReturnedSearchTerms =
                        string.Join(", ", list.Take(5).Select(x => x.Value.ToLogFriendlyString()));

                    var telemetryProperties = new Dictionary<string, string>
                    {
                        {"TimeStamp", VirtualDateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")},
                        {"QueryTerms", searchParams.Keywords},
                        {"ResultCount", list.Count().ToString()},
                        {"SearchType", searchParams.SearchType.ToString()},
                        {"SampleOfResultsReturned", detailedListOfReturnedSearchTerms}
                    };

                    //SharedBusinessLogic.SharedOptions.AppInsightsClient?.TrackEvent("Gpg_SicCode_Suggest", telemetryProperties);

                    await _viewingService.SearchBusinessLogic.SearchLog.WriteAsync(telemetryProperties);
                }

                #endregion

                searchParams.SearchFields =
                    $"{nameof(EmployerSearchModel.SicCodeIds)};{nameof(EmployerSearchModel.SicCodeListOfSynonyms)}";
                searchParams.Keywords = "*"; // searchTermModified

                if (list.Any()) searchResults = await DoSearchAsync(searchParams, facets);
            }

            if (searchParams.SearchType == SearchTypes.ByEmployerName)
            {
                searchParams.Keywords = searchParams.Keywords?.Trim();
                searchParams.Keywords = searchParams.RemoveTheMostCommonTermsOnOurDatabaseFromTheKeywords();
                searchResults = await DoSearchAsync(searchParams, facets);
            }

            // build the result view model
            return new SearchViewModel
            {
                SizeOptions = GetOrgSizeOptions(searchParams.FilterEmployerSizes, facets["Size"]),
                SectorOptions = await GetSectorOptionsAsync(searchParams.FilterSicSectionIds, facets["SicSectionIds"]),
                ReportingYearOptions = GetReportingYearOptions(searchParams.FilterReportedYears),
                ReportingStatusOptions = GetReportingStatusOptions(searchParams.FilterReportingStatus),
                Employers = searchResults,
                search = searchTermEnteredOnScreen,
                p = searchParams.Page,
                s = searchParams.FilterSicSectionIds,
                es = searchParams.FilterEmployerSizes,
                y = searchParams.FilterReportedYears,
                st = searchParams.FilterReportingStatus,
                t = searchParams.SearchType.ToInt32().ToString()
            };
        }

        public async Task<List<SuggestEmployerResult>> SuggestEmployerNameAsync(string searchText)
        {
            var results = await _viewingService.SearchBusinessLogic.EmployerSearchRepository.SuggestAsync(
                searchText,
                $"{nameof(EmployerSearchModel.Name)};{nameof(EmployerSearchModel.PreviousName)};{nameof(EmployerSearchModel.Abbreviations)}");

            var matches = new List<SuggestEmployerResult>();
            foreach (var result in results)
            {
                //Ensure all names in suggestions are unique
                if (matches.Any(m => m.Text == result.Value.Name)) continue;

                matches.Add(
                    new SuggestEmployerResult
                    {
                        Id = result.Value.OrganisationIdEncrypted, Text = result.Value.Name,
                        PreviousName = result.Value.PreviousName
                    });
            }

            return matches;
        }

        public async Task<List<SicCodeSearchResult>> GetListOfSicCodeSuggestionsAsync(string searchText)
        {
            var listOfSicCodeSuggestionsFromIndex =
                await GetListOfSicCodeSuggestionsFromIndexAsync(searchText);

            return SicCodeSearchResult.ConvertToScreenReadableListOfSuggestions(searchText,
                listOfSicCodeSuggestionsFromIndex);
        }

        public PagedResult<EmployerSearchModel> GetPagedResult(IEnumerable<EmployerSearchModel> searchResults,
            long totalRecords,
            int page,
            int pageSize)
        {
            var result = new PagedResult<EmployerSearchModel>();

            if (page == 0 || page < 0) page = 1;

            result.Results = searchResults.ToList();
            result.ActualRecordTotal = (int) totalRecords;
            result.VirtualRecordTotal = result.Results.Count;
            result.CurrentPage = page;
            result.PageSize = pageSize;

            return result;
        }

        public List<OptionSelect> GetOrgSizeOptions(IEnumerable<int> filterOrgSizes,
            Dictionary<object, long> facetResults)
        {
            var allSizes = Enum.GetValues(typeof(OrganisationSizes));

            // setup the filters
            var results = new List<OptionSelect>();
            foreach (OrganisationSizes size in allSizes)
            {
                var id = (int) size;
                var label = size.GetAttribute<DisplayAttribute>().Name;
                var isChecked = filterOrgSizes != null && filterOrgSizes.Contains(id);
                results.Add(
                    new OptionSelect
                    {
                        Id = $"Size{id}", Label = label, Value = id.ToString(), Checked = isChecked
                        // Disabled = facetResults.Count == 0 && !isChecked
                    });
            }

            return results;
        }

        public async Task<List<OptionSelect>> GetSectorOptionsAsync(IEnumerable<char> filterSicSectionIds,
            Dictionary<object, long> facetResults)
        {
            // setup the filters
            var allSectors = await GetAllSicSectionsAsync();
            var sources = new List<OptionSelect>();
            foreach (var sector in allSectors)
            {
                var isChecked = filterSicSectionIds != null &&
                                filterSicSectionIds.Any(x => x == sector.SicSectionCode[0]);
                sources.Add(
                    new OptionSelect
                    {
                        Id = sector.SicSectionCode,
                        Label = sector.Description.TrimEnd('\r', '\n'),
                        Value = sector.SicSectionCode,
                        Checked = isChecked
                        // Disabled = facetResults.Count == 0 && !isChecked
                    });
            }

            return sources;
        }

        public async Task<List<SearchViewModel.SicSection>> GetAllSicSectionsAsync()
        {
            var results = new List<SearchViewModel.SicSection>();
            var sortedSics =
                await _sharedBusinessLogic.DataRepository.ToListAscendingAsync<SicSection, string>(sic =>
                    sic.Description);

            foreach (var sector in sortedSics)
                results.Add(
                    new SearchViewModel.SicSection
                    {
                        SicSectionCode = sector.SicSectionId,
                        Description = sector.Description = sector.Description.BeforeFirst(";")
                    });

            return results;
        }

        private async Task<PagedResult<EmployerSearchModel>> DoSearchAsync(EmployerSearchParameters searchParams,
            Dictionary<string, Dictionary<object, long>> facets)
        {
            return await _viewingService.SearchBusinessLogic.EmployerSearchRepository.SearchAsync(
                searchParams.Keywords, // .ToSearchQuery(),
                searchParams.Page,
                searchParams.SearchType,
                searchParams.PageSize,
                filter: searchParams.ToFilterQuery(),
                facets: facets,
                orderBy: string.IsNullOrWhiteSpace(searchParams.Keywords) ? nameof(EmployerSearchModel.Name) : null,
                searchFields: searchParams.SearchFields,
                searchMode: searchParams.SearchMode);
        }

        private async Task<IEnumerable<KeyValuePair<string, SicCodeSearchModel>>>
            GetListOfSicCodeSuggestionsFromIndexAsync(
                string searchText)
        {
            var listOfSicCodeSuggestionsFromIndex =
                await _viewingService.SearchBusinessLogic.SicCodeSearchRepository.SuggestAsync(
                    searchText,
                    $"{nameof(SicCodeSearchModel.SicCodeDescription)},{nameof(SicCodeSearchModel.SicCodeListOfSynonyms)}",
                    null,
                    false,
                    100);
            return listOfSicCodeSuggestionsFromIndex;
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