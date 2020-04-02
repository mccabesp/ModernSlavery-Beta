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
using ModernSlavery.WebUI.Shared.Interfaces;

namespace ModernSlavery.WebUI.Registration.Classes
{
    public class PrivateSectorRepository : IPagedRepository<EmployerRecord>
    {
        private readonly ICompaniesHouseAPI _CompaniesHouseAPI;
        private readonly IDataRepository _DataRepository;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpSession _Session;
        private readonly SharedOptions SharedOptions;
        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;
        public PrivateSectorRepository(
            SharedOptions sharedOptions,
            IHttpContextAccessor httpContextAccessor,
            IHttpSession session,
            IDataRepository dataRepository,
            ICompaniesHouseAPI companiesHouseAPI,
            IOrganisationBusinessLogic organisationBusinessLogic)
        {
            SharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _httpContextAccessor = httpContextAccessor;
            _Session = session;
            _DataRepository = dataRepository;
            _CompaniesHouseAPI = companiesHouseAPI;
            _organisationBusinessLogic = organisationBusinessLogic;
        }

        public void Delete(EmployerRecord entity)
        {
            throw new NotImplementedException();
        }

        public void Insert(EmployerRecord entity)
        {
            throw new NotImplementedException();
        }

        public async Task<PagedResult<EmployerRecord>> SearchAsync(string searchText, int page, int pageSize,
            bool test = false)
        {
            if (searchText.IsNumber()) searchText = searchText.PadLeft(8, '0');


            var remoteTotal = 0;
            var searchResults = test ? null : LoadSearch(searchText);

            if (searchResults == null)
            {
                var orgs = new List<Organisation>();
                var localResults = new List<Organisation>();

                if (!test)
                {
                    orgs = _DataRepository.GetAll<Organisation>()
                        .Where(
                            o => o.SectorType == SectorTypes.Private && o.Status == OrganisationStatuses.Active &&
                                 o.LatestAddress != null)
                        .ToList();

                    if (searchText.IsCompanyNumber())
                        localResults = orgs.Where(o => o.CompanyNumber.EqualsI(searchText))
                            .OrderBy(o => o.OrganisationName).ToList();
                    else
                        localResults = orgs.Where(o => o.OrganisationName.ContainsI(searchText))
                            .OrderBy(o => o.OrganisationName).ToList();
                }

                try
                {
                    searchResults =
                        await _CompaniesHouseAPI.SearchEmployersAsync(searchText, 1, _CompaniesHouseAPI.MaxRecords,
                            test);
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
                        searchResults = new PagedResult<EmployerRecord>();
                    else
                        throw;
                }

                if (!searchText.IsCompanyNumber() && remoteTotal > 0)
                {
                    //Replace CoHo orgs with db orgs with same company number
                    var companyNumbers = new SortedSet<string>(
                        searchResults.Results.Select(s => s.CompanyNumber),
                        StringComparer.OrdinalIgnoreCase);
                    var existingResults = orgs.Where(o => companyNumbers.Contains(o.CompanyNumber)).ToList();
                    localResults = localResults.Union(existingResults).ToList();
                }

                var localTotal = localResults.Count;

                //Remove any coho results found in DB
                if (localTotal > 0)
                {
                    localTotal -=
                        searchResults.Results.RemoveAll(r => localResults.Any(l => l.CompanyNumber == r.CompanyNumber));

                    if (localResults.Count > 0)
                    {
                        if (test) //Make sure test employer is first
                            searchResults.Results.AddRange(localResults.Select(o => _organisationBusinessLogic.CreateEmployerRecord(o)));
                        else
                            searchResults.Results.InsertRange(0, localResults.Select(o => _organisationBusinessLogic.CreateEmployerRecord(o)));

                        searchResults.ActualRecordTotal += localTotal;
                    }
                }

                if (!test) SaveSearch(searchText, searchResults, remoteTotal);
            }

            var result = new PagedResult<EmployerRecord>();
            result.VirtualRecordTotal = searchResults.ActualRecordTotal > _CompaniesHouseAPI.MaxRecords
                ? _CompaniesHouseAPI.MaxRecords
                : searchResults.ActualRecordTotal;
            result.ActualRecordTotal = searchResults.ActualRecordTotal;
            result.CurrentPage = page;
            result.PageSize = pageSize;
            result.Results = searchResults.Results.Page(pageSize, page).ToList();
            return result;
        }

        public async Task<string> GetSicCodesAsync(string companyNumber)
        {
            return await _CompaniesHouseAPI.GetSicCodesAsync(companyNumber);
        }

        public void ClearSearch()
        {
            _Session.Remove("LastPrivateSearchText");
            _Session.Remove("LastPrivateSearchResults");
            _Session.Remove("LastPrivateSearchRemoteTotal");
        }

        public void SaveSearch(string searchText, PagedResult<EmployerRecord> results, int remoteTotal)
        {
            _Session["LastPrivateSearchText"] = searchText;
            _Session["LastPrivateSearchResults"] = results;
            _Session["LastPrivateSearchRemoteTotal"] = remoteTotal;
        }

        public PagedResult<EmployerRecord> LoadSearch(string searchText)
        {
            var lastSearchText = _Session["LastPrivateSearchText"] as string;
            var remoteTotal = _Session["LastPrivateSearchRemoteTotal"].ToInt32();

            PagedResult<EmployerRecord> result = null;

            if (!SharedOptions.IsProduction()
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
                result = _Session.Get<PagedResult<EmployerRecord>>("LastPrivateSearchResults");
                if (result == null) result = new PagedResult<EmployerRecord>();
            }

            return result;
        }
    }
}