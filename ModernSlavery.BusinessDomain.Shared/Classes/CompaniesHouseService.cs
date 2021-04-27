using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models.CompaniesHouse;
using ModernSlavery.Core.Options;

namespace ModernSlavery.BusinessDomain.Shared.Classes
{
    public class CompaniesHouseService : ICompaniesHouseService
    {
        private const string SourceOfChange = "CoHo";
        private const string DetailsOfChange = "Replaced by CoHo";
        private readonly ICompaniesHouseAPI _CompaniesHouseAPI;
        private readonly CompaniesHouseOptions _companiesHouseOptions;

        private readonly IEventLogger _CustomLogger;
        private readonly IDataRepository _dataRepository;
        private readonly IPostcodeChecker _PostcodeChecker;
        private readonly IOrganisationBusinessLogic _organisationBusinessLogic;
        private readonly ISearchBusinessLogic _searchBusinessLogic;

        public CompaniesHouseService(IEventLogger customLogger, IDataRepository dataRepository, IOrganisationBusinessLogic organisationBusinessLogic,
            ICompaniesHouseAPI companiesHouseAPI, CompaniesHouseOptions companiesHouseOptions, IPostcodeChecker postcodeChecker, ISearchBusinessLogic searchBusinessLogic)
        {
            _CustomLogger = customLogger;
            _dataRepository = dataRepository;
            _organisationBusinessLogic = organisationBusinessLogic;
            _CompaniesHouseAPI = companiesHouseAPI;
            _companiesHouseOptions = companiesHouseOptions;
            _PostcodeChecker = postcodeChecker;
            _searchBusinessLogic = searchBusinessLogic;
        }

        public async Task<OrganisationAddress> CreateOrganisationAddressFromCompaniesHouseAddressAsync(CompaniesHouseAddress companiesHouseAddress)
        {
            var premisesAndLine1 = GetAddressLineFromPremisesAndAddressLine1(companiesHouseAddress);
            bool? isUkAddress = null;
            if (companiesHouseAddress!=null && !string.IsNullOrWhiteSpace(companiesHouseAddress.PostalCode) && await _PostcodeChecker.CheckPostcodeAsync(companiesHouseAddress.PostalCode)) isUkAddress = true;

            var address = new OrganisationAddress
            {
                Address1 = companiesHouseAddress?.CareOf ?? premisesAndLine1,
                Address2 = companiesHouseAddress?.CareOf != null ? premisesAndLine1 : companiesHouseAddress?.AddressLine2,
                Address3 = companiesHouseAddress?.CareOf != null ? companiesHouseAddress?.AddressLine2 : null,
                TownCity = companiesHouseAddress?.Locality,
                County = companiesHouseAddress?.Region,
                Country = companiesHouseAddress?.Country,
                PostCode = companiesHouseAddress?.PostalCode,
                PoBox = companiesHouseAddress?.PoBox,
                Status = AddressStatuses.Active,
                StatusDate = VirtualDateTime.Now,
                StatusDetails = DetailsOfChange,
                Modified = VirtualDateTime.Now,
                Created = VirtualDateTime.Now,
                Source = SourceOfChange,
                IsUkAddress = isUkAddress,
                CreatedByUserId=-1
            };
            address.Trim();
            return address;
        }

        public async Task UpdateOrganisationsAsync()
        {
            var lastCheck = VirtualDateTime.Now.AddHours(0- _companiesHouseOptions.UpdateHours);

            IQueryable<Organisation> organisations;

            if (_companiesHouseOptions.BatchUpdateSize>0)
                organisations = _dataRepository.GetAll<Organisation>()
                .Where(org => !org.OptedOutFromCompaniesHouseUpdate && org.CompanyNumber != null && org.CompanyNumber != "" && (org.LastCheckedAgainstCompaniesHouse == null || org.LastCheckedAgainstCompaniesHouse < lastCheck))
                .OrderByDescending(org => org.LastCheckedAgainstCompaniesHouse).Take(_companiesHouseOptions.BatchUpdateSize);
            else
                organisations = _dataRepository.GetAll<Organisation>()
                .Where(org => !org.OptedOutFromCompaniesHouseUpdate && org.CompanyNumber != null && org.CompanyNumber != "" && (org.LastCheckedAgainstCompaniesHouse == null || org.LastCheckedAgainstCompaniesHouse < lastCheck))
                .OrderByDescending(org => org.LastCheckedAgainstCompaniesHouse);

            foreach (var organisation in organisations)
                await UpdateOrganisationAsync(organisation);
        }

        public async Task UpdateOrganisationAsync(Organisation organisation)
        {
            if (organisation == null) throw new ArgumentNullException(nameof(organisation));
            bool dataChanged = false;
            try
            {
                var organisationFromCompaniesHouse = await _CompaniesHouseAPI.GetCompanyAsync(organisation.CompanyNumber);

                try
                {
                    if (organisationFromCompaniesHouse == null)
                    {
                        organisation.OptedOutFromCompaniesHouseUpdate = true;
                    }
                    else
                    {
                        await UpdateSicCodeAsync(organisation, organisationFromCompaniesHouse);

                        if (await UpdateAddressAsync(organisation, organisationFromCompaniesHouse)) dataChanged = true;

                        if (await UpdateNameAsync(organisation, organisationFromCompaniesHouse)) dataChanged = true;

                    }
                    organisation.LastCheckedAgainstCompaniesHouse = VirtualDateTime.Now;
                    await _dataRepository.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    var message = $"Update from Companies House: Failed to update database, organisation id :{organisation.OrganisationId}, Company number:{organisation.CompanyNumber}";
                    _CustomLogger.Error(message, ex);
                }
            }
            catch (Exception ex)
            {
                var message = $"Update from Companies House: Failed to get company data from companies house, organisation id:{organisation.OrganisationId}, Company number:{organisation.CompanyNumber}";
                _CustomLogger.Error(message, ex);
                return;
            }

            //Update the search record
            if (dataChanged)
                try
                {
                    await _searchBusinessLogic.RefreshSearchDocumentsAsync(organisation);
                }
                catch (Exception ex)
                {
                    var message = $"Update from Companies House: Failed to update search indexes after update from companies house, organisation id:{organisation.OrganisationId}, Company number:{organisation.CompanyNumber}";
                    _CustomLogger.Error(message, ex);
                }
        }

        public async Task<bool> UpdateSicCodeAsync(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse)
        {
            var companySicCodes = organisationFromCompaniesHouse.SicCodes ?? new List<string>();
            if (organisation.SectorType == SectorTypes.Public) companySicCodes.Add("1");
            var retired=RetireExtraSicCodes(organisation, companySicCodes);
            var added=AddNewSicCodes(organisation, companySicCodes);
            return retired || added;
        }

        public async Task<bool> UpdateAddressAsync(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse)
        {
            var companiesHouseAddress = organisationFromCompaniesHouse.RegisteredOfficeAddress;
            var newOrganisationAddressFromCompaniesHouse = await CreateOrganisationAddressFromCompaniesHouseAddressAsync(companiesHouseAddress);
            if (newOrganisationAddressFromCompaniesHouse.IsEmpty())return false;

            var oldOrganisationAddress = organisation.GetLatestAddress();

            var now = VirtualDateTime.Now;
            if (oldOrganisationAddress != null)
            {
                if (oldOrganisationAddress.AddressMatches(newOrganisationAddressFromCompaniesHouse)) return false;
                oldOrganisationAddress.SetStatus(AddressStatuses.Retired, -1, DetailsOfChange, now);
            }

            newOrganisationAddressFromCompaniesHouse.OrganisationId = organisation.OrganisationId;
            newOrganisationAddressFromCompaniesHouse.SetStatus(AddressStatuses.Active, -1, DetailsOfChange, now);

            organisation.OrganisationAddresses.Add(newOrganisationAddressFromCompaniesHouse);
            organisation.LatestAddress = newOrganisationAddressFromCompaniesHouse;
            _dataRepository.Insert(newOrganisationAddressFromCompaniesHouse);

            return true;
        }

        public async Task<bool> UpdateNameAsync(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse)
        {
            var companyNameFromCompaniesHouse = organisationFromCompaniesHouse.CompanyName;
            companyNameFromCompaniesHouse = companyNameFromCompaniesHouse?.Left(160);

            if (IsCompanyNameEqual(_organisationBusinessLogic.GetOrganisationName(organisation), companyNameFromCompaniesHouse)) return false;

            var nameToAdd = new OrganisationName
            {
                Organisation = organisation,
                Name = companyNameFromCompaniesHouse,
                Source = SourceOfChange
            };
            organisation.OrganisationNames.Add(nameToAdd);
            organisation.OrganisationName = companyNameFromCompaniesHouse;
            _dataRepository.Insert(nameToAdd);
            return true;
        }

        public bool AddressMatches(OrganisationAddress firstOrganisationAddress,
            OrganisationAddress secondOrganisationAddress)
        {
            return string.Equals(
                       firstOrganisationAddress.Address1,
                       secondOrganisationAddress.Address1,
                       StringComparison.Ordinal)
                   && string.Equals(
                       firstOrganisationAddress.Address2,
                       secondOrganisationAddress.Address2,
                       StringComparison.Ordinal)
                   && string.Equals(
                       firstOrganisationAddress.Address3,
                       secondOrganisationAddress.Address3,
                       StringComparison.Ordinal)
                   && string.Equals(
                       firstOrganisationAddress.TownCity,
                       secondOrganisationAddress.TownCity,
                       StringComparison.Ordinal)
                   && string.Equals(
                       firstOrganisationAddress.County,
                       secondOrganisationAddress.County,
                       StringComparison.Ordinal)
                   && string.Equals(
                       firstOrganisationAddress.Country,
                       secondOrganisationAddress.Country,
                       StringComparison.Ordinal)
                   && string.Equals(
                       firstOrganisationAddress.PostCode,
                       secondOrganisationAddress.PostCode,
                       StringComparison.Ordinal)
                   && string.Equals(
                       firstOrganisationAddress.PoBox,
                       secondOrganisationAddress.PoBox,
                       StringComparison.Ordinal);
        }

        public bool IsCompanyNameEqual(OrganisationName organisationName, string companyName)
        {
            return string.Equals(
                organisationName.Name,
                companyName,
                StringComparison.Ordinal);
        }

        public bool SicCodesEqual(IEnumerable<OrganisationSicCode> sicCodes, IEnumerable<string> companiesHouseSicCodes)
        {
            return new HashSet<int>(sicCodes.Select(sic => sic.SicCodeId)).SetEquals(
                companiesHouseSicCodes.Select(sic => int.Parse(sic)));
        }

        #region Private Methods
        private bool RetireExtraSicCodes(Organisation organisation, List<string> companySicCodes)
        {
            int retiredCount=0;
            var sicCodeIds = _organisationBusinessLogic.GetOrganisationSicCodes(organisation).Select(sicCode => sicCode.SicCodeId);
            var newSicCodeIds = companySicCodes.Where(sicCode => !string.IsNullOrEmpty(sicCode)).Select(sicCode => int.Parse(sicCode));

            var idsToBeRetired = sicCodeIds.Except(newSicCodeIds);
            var sicCodesToBeRetired = organisation.OrganisationSicCodes.Where(s => idsToBeRetired.Contains(s.SicCodeId));
            foreach (var sicCodeToBeRetired in sicCodesToBeRetired)
            {
                sicCodeToBeRetired.Retired = VirtualDateTime.Now;
                retiredCount++;
            }

            return retiredCount>0;
        }

        private bool AddNewSicCodes(Organisation organisation, List<string> companySicCodes)
        {
            int addedCount=0;
            var sicCodeIds = _organisationBusinessLogic.GetOrganisationSicCodes(organisation).Select(sicCode => sicCode.SicCodeId);
            var newSicCodeIds = companySicCodes.Where(sicCode => !string.IsNullOrEmpty(sicCode)).Select(sicCode => int.Parse(sicCode));

            var idsToBeAdded = newSicCodeIds.Except(sicCodeIds);
            foreach (var sicCodeId in idsToBeAdded)
                if (_dataRepository.GetAll<SicCode>().Any(sicCode => sicCode.SicCodeId == sicCodeId))
                {
                    var sicCodeToBeAdded = new OrganisationSicCode
                    {
                        Organisation = organisation,
                        SicCodeId = sicCodeId,
                        Source = SourceOfChange,
                        Created = VirtualDateTime.Now
                    };
                    organisation.OrganisationSicCodes.Add(sicCodeToBeAdded);
                    _dataRepository.Insert(sicCodeToBeAdded);
                    addedCount++;
                }

            return addedCount>0;
        }

        private string GetAddressLineFromPremisesAndAddressLine1(CompaniesHouseAddress companiesHouseAddress)
        {
            return companiesHouseAddress?.Premises == null
                ? companiesHouseAddress?.AddressLine1
                : companiesHouseAddress?.Premises + "," + companiesHouseAddress?.AddressLine1;
        }

        #endregion
    }
}