using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Registration.Classes
{
    public class PrivateSectorRepository : IPagedRepository<OrganisationRecord>
    {
        private readonly ICompaniesHouseAPI _CompaniesHouseAPI;
        private readonly CompaniesHouseOptions _companiesHouseOptions;
        private readonly IDataRepository _DataRepository;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpSession _Session;
        private readonly TestOptions _testOptions;
        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;
        public PrivateSectorRepository(
            TestOptions testOptions,
            IHttpContextAccessor httpContextAccessor,
            IHttpSession session,
            IDataRepository dataRepository,
            ICompaniesHouseAPI companiesHouseAPI, CompaniesHouseOptions companiesHouseOptions,
            IOrganisationBusinessLogic organisationBusinessLogic)
        {
            _testOptions = testOptions ?? throw new ArgumentNullException(nameof(testOptions));
            _httpContextAccessor = httpContextAccessor;
            _Session = session;
            _DataRepository = dataRepository;
            _CompaniesHouseAPI = companiesHouseAPI;
            _companiesHouseOptions = companiesHouseOptions;
            _organisationBusinessLogic = organisationBusinessLogic;
        }

        public void Delete(OrganisationRecord entity)
        {
            throw new NotImplementedException();
        }

        public void Insert(OrganisationRecord entity)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResult<OrganisationRecord>> SearchAsync(string searchText, int page, int pageSize)
        {
            if (searchText.IsNumber()) searchText = searchText.PadLeft(8, '0');

            if (_testOptions.LoadTesting && _testOptions.SearchCompaniesHouse)
            {
                var lastSearchText = _Session["LastPrivateSearchText"] as string;
                if (string.IsNullOrWhiteSpace(lastSearchText))
                {
                    var testOrg = _DataRepository.GetAll<Organisation>().Where(o => o.SectorType == SectorTypes.Private && o.Status == OrganisationStatuses.Active).OrderBy(o => Guid.NewGuid()).First();
                    lastSearchText = testOrg.OrganisationName;
                }
                searchText = lastSearchText;
            }

            var remoteTotal = 0;
            var searchResults = LoadSearch(searchText);

            if (searchResults == null)
            {
                var localResults = new List<Organisation>();

                if (_testOptions.LoadTesting && !_testOptions.SearchCompaniesHouse)
                {
                    searchResults = new PagedResult<OrganisationRecord>();
                    localResults = _DataRepository.GetAll<Organisation>().Where(o => o.SectorType == SectorTypes.Private && o.Status == OrganisationStatuses.Active).OrderBy(o => Guid.NewGuid()).Take(_companiesHouseOptions.MaxResponseCompanies).ToList();
                    searchResults.Results= _organisationBusinessLogic.CreateOrganisationRecords(localResults,false).ToList();
                    searchResults.PageSize = _companiesHouseOptions.MaxResponseCompanies;
                    searchResults.VirtualRecordTotal = searchResults.Results.Count;
                    searchResults.ActualRecordTotal = searchResults.VirtualRecordTotal;
                }
                else
                {

                    var orgs = _DataRepository.GetAll<Organisation>().Where(o => o.SectorType == SectorTypes.Private && o.Status == OrganisationStatuses.Active).OrderBy(o => o.OrganisationName).ToList();

                    if (searchText.IsCompanyNumber())
                        localResults = orgs.Where(o => o.CompanyNumber.EqualsI(searchText)).Take(_companiesHouseOptions.MaxResponseCompanies).ToList();
                    else
                        localResults = orgs.Where(o => o.OrganisationName.ContainsI(searchText)).Take(_companiesHouseOptions.MaxResponseCompanies).OrderBy(o => o.OrganisationName).ToList();

                    var maxCoHoResults = _companiesHouseOptions.MaxResponseCompanies - localResults.Count;

                    try
                    {
                        searchResults = await _CompaniesHouseAPI.SearchOrganisationsAsync(searchText, 1, _companiesHouseOptions.MaxResponseCompanies, maxCoHoResults);
                        remoteTotal = searchResults.Results.Count;
                    }
                    catch (Exception ex)
                    {
                        remoteTotal = -1;
                        if ((ex.InnerException ?? ex).Message.ContainsI(
                                "502",
                                "Bad Gateway",
                                "the connected party did not properly respond after a period of time")
                            && localResults.Count > 0)
                            searchResults = new PagedResult<OrganisationRecord>();
                        else
                            throw;
                    }

                    if (!searchText.IsCompanyNumber() && remoteTotal > 0)
                    {
                        //Replace CoHo orgs with db orgs with same company number
                        var companyNumbers = new SortedSet<string>(searchResults.Results.Select(s => s.CompanyNumber),StringComparer.OrdinalIgnoreCase);
                        var existingResults = orgs.Where(o => companyNumbers.Contains(o.CompanyNumber)).ToList();
                        localResults = localResults.Union(existingResults).ToList();
                    }

                    var localTotal = localResults.Count;

                    //Remove any coho results found in DB
                    if (localTotal > 0)
                    {
                        localTotal -= searchResults.Results.RemoveAll(r => localResults.Any(l => l.CompanyNumber == r.CompanyNumber));

                        if (localResults.Count > 0)
                        {
                            var localRecords = _organisationBusinessLogic.CreateOrganisationRecords(localResults, false);

                            //Make sure local organisations are first
                            searchResults.Results.InsertRange(0, localRecords);

                            searchResults.ActualRecordTotal += localTotal;
                        }
                    }
                }

                searchResults.VirtualRecordTotal = searchResults.ActualRecordTotal > _companiesHouseOptions.MaxResponseCompanies
                    ? _companiesHouseOptions.MaxResponseCompanies
                    : searchResults.ActualRecordTotal;

                SaveSearch(searchText, searchResults, remoteTotal);
            }

            var result = new PagedResult<OrganisationRecord>();
            result.VirtualRecordTotal = searchResults.VirtualRecordTotal;
            result.ActualRecordTotal = searchResults.ActualRecordTotal;
            result.CurrentPage = page;
            result.PageSize = pageSize;
            result.Results = searchResults.Results.Page(pageSize, page).ToList();
            return result;
        }

        public async Task<string> GetSicCodesAsync(string companyNumber)
        {
            return !string.IsNullOrWhiteSpace(companyNumber) ? await _CompaniesHouseAPI.GetSicCodesAsync(companyNumber) : null;
        }

        public void ClearSearch()
        {
            _Session.Remove("LastPrivateSearchText");
            _Session.Remove("LastPrivateSearchResults");
            _Session.Remove("LastPrivateSearchRemoteTotal");
        }

        public void SaveSearch(string searchText, PagedResult<OrganisationRecord> results, int remoteTotal)
        {
            _Session["LastPrivateSearchText"] = searchText;
            _Session["LastPrivateSearchResults"] = results;
            _Session["LastPrivateSearchRemoteTotal"] = remoteTotal;
        }

        public PagedResult<OrganisationRecord> LoadSearch(string searchText)
        {
            var lastSearchText = _Session["LastPrivateSearchText"] as string;
            var remoteTotal = _Session["LastPrivateSearchRemoteTotal"].ToInt32();

            PagedResult<OrganisationRecord> result = null;

            if (!_testOptions.IsProduction()
                && _httpContextAccessor.HttpContext != null
                && _httpContextAccessor.HttpContext.Request.Query["fail"].ToBoolean())
            {
                ClearSearch();
            }
            else if (remoteTotal == -1 || string.IsNullOrWhiteSpace(lastSearchText) || searchText != lastSearchText)
            {
                ClearSearch();
            }
            else
            {
                result = _Session.Get<PagedResult<OrganisationRecord>>("LastPrivateSearchResults");
                if (result == null) result = new PagedResult<OrganisationRecord>();
            }

            return result;
        }
    }
}