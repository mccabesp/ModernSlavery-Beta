﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.WebUI.Registration.Classes
{
    public class PublicSectorRepository : IPagedRepository<OrganisationRecord>
    {
        private readonly ICompaniesHouseAPI _CompaniesHouseAPI;
        private readonly IDataRepository _DataRepository;
        private readonly TestOptions _testOptions;
        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;

        public PublicSectorRepository(IDataRepository dataRepository, ICompaniesHouseAPI companiesHouseAPI,
            TestOptions testOptions,
            IOrganisationBusinessLogic organisationBusinessLogic)
        {
            _testOptions = testOptions ?? throw new ArgumentNullException(nameof(testOptions));
            _DataRepository = dataRepository;
            _CompaniesHouseAPI = companiesHouseAPI;
            _testOptions = testOptions;
            _organisationBusinessLogic = organisationBusinessLogic;
        }

        public async Task<PagedResult<OrganisationRecord>> SearchAsync(string searchText, int page, int pageSize)
        {
            var result = new PagedResult<OrganisationRecord>();

            List<Organisation> searchResultsList;
            if (_testOptions.LoadTesting)
                searchResultsList = _DataRepository.GetAll<Organisation>().Where(o => o.SectorType == SectorTypes.Public && o.Status == OrganisationStatuses.Active).OrderBy(o => Guid.NewGuid()).ToList();
            else
                searchResultsList = _DataRepository.GetAll<Organisation>().Where(o => o.SectorType == SectorTypes.Public && o.Status == OrganisationStatuses.Active).OrderBy(o => o.OrganisationName).ToList().Where(o => o.OrganisationName.ContainsI(searchText)).ToList();

            result.ActualRecordTotal = searchResultsList.Count;
            result.VirtualRecordTotal = searchResultsList.Count;
            result.CurrentPage = page;
            result.PageSize = pageSize;
            result.Results = _organisationBusinessLogic.CreateOrganisationRecords(searchResultsList.Page(pageSize, page),false).ToList();
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