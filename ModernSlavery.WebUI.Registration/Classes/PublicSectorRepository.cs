using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.WebUI.Registration.Classes
{
    public class PublicSectorRepository : IPagedRepository<OrganisationRecord>
    {
        private readonly ICompaniesHouseAPI _CompaniesHouseAPI;
        private readonly IDataRepository _DataRepository;
        private readonly SharedOptions _sharedOptions;
        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;

        public PublicSectorRepository(IDataRepository dataRepository, ICompaniesHouseAPI companiesHouseAPI,
            SharedOptions sharedOptions,
            IOrganisationBusinessLogic organisationBusinessLogic)
        {
            _DataRepository = dataRepository;
            _CompaniesHouseAPI = companiesHouseAPI;
            _sharedOptions = sharedOptions;
            _organisationBusinessLogic = organisationBusinessLogic;
        }

        public async Task<PagedResult<OrganisationRecord>> SearchAsync(string searchText, int page, int pageSize,
            bool test = false)
        {
            var result = new PagedResult<OrganisationRecord>();
            if (test)
            {
                var organisations = new List<OrganisationRecord>();

                var min = await _DataRepository.CountAsync<Organisation>();

                var id = Numeric.Rand(min, int.MaxValue - 1);
                var organisation = new OrganisationRecord
                {
                    OrganisationName = _sharedOptions.TestPrefix + "_GovDept_" + id,
                    CompanyNumber = ("_" + id).Left(10),
                    Address1 = "Test Address 1",
                    Address2 = "Test Address 2",
                    City = "Test Address 3",
                    Country = "Test Country",
                    PostCode = "Test Post Code",
                    EmailDomains = "*@*",
                    PoBox = null,
                    SicCodeIds = "1"
                };
                organisations.Add(organisation);

                result.ActualRecordTotal = organisations.Count;
                result.VirtualRecordTotal = organisations.Count;
                result.CurrentPage = page;
                result.PageSize = pageSize;
                result.Results = organisations;
                return result;
            }

            var searchResults = await _DataRepository.ToListAsync<Organisation>(o =>
                o.SectorType == SectorTypes.Public && o.Status == OrganisationStatuses.Active);
            var searchResultsList = searchResults.Where(o => o.OrganisationName.ContainsI(searchText))
                .OrderBy(o => o.OrganisationName)
                .ThenBy(o => o.OrganisationName)
                .ToList();
            result.ActualRecordTotal = searchResultsList.Count;
            result.VirtualRecordTotal = searchResultsList.Count;
            result.CurrentPage = page;
            result.PageSize = pageSize;
            result.Results = searchResultsList.Page(pageSize, page).Select(o => _organisationBusinessLogic.CreateOrganisationRecord(o)).ToList();
            return result;
        }

        public async Task<string> GetSicCodesAsync(string companyNumber)
        {
            string sics = null;
            if (!string.IsNullOrWhiteSpace(companyNumber))
                sics = await _CompaniesHouseAPI.GetSicCodesAsync(companyNumber);

            if (!string.IsNullOrWhiteSpace(sics)) sics = "," + sics;

            sics = "1" + sics;
            return sics;
        }

        #region Properties

        public void Delete(OrganisationRecord entity)
        {
            throw new NotImplementedException();
        }

        public void Insert(OrganisationRecord entity)
        {
            throw new NotImplementedException();
        }

        public void ClearSearch()
        {
        }
    }

    #endregion
}