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
        public CompaniesHouseService(IEventLogger customLogger, IDataRepository dataRepository, IOrganisationBusinessLogic organisationBusinessLogic,
            ICompaniesHouseAPI companiesHouseAPI, CompaniesHouseOptions companiesHouseOptions, IPostcodeChecker postcodeChecker)
        {
            _CustomLogger = customLogger;
            _dataRepository = dataRepository;
            _organisationBusinessLogic = organisationBusinessLogic;
            _CompaniesHouseAPI = companiesHouseAPI;
            _companiesHouseOptions = companiesHouseOptions;
            _PostcodeChecker = postcodeChecker;
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
                IsUkAddress = isUkAddress
            };
            address.Trim();
            return address;
        }

        public async Task UpdateOrganisationsAsync()
        {
            var lastCheck = VirtualDateTime.Now.AddHours(0- _companiesHouseOptions.UpdateHours);

            var organisations = _dataRepository.GetAll<Organisation>()
                .Where(org => !org.OptedOutFromCompaniesHouseUpdate && org.CompanyNumber != null && org.CompanyNumber != "" && (org.LastCheckedAgainstCompaniesHouse == null || org.LastCheckedAgainstCompaniesHouse < lastCheck))
                .OrderByDescending(org => org.LastCheckedAgainstCompaniesHouse).Take(_companiesHouseOptions.MaxApiCallsPerFiveMins);

            foreach (var organisation in organisations)
                await UpdateOrganisationAsync(organisation);
        }

        public async Task UpdateOrganisationAsync(Organisation organisation)
        {
            if (organisation == null) throw new ArgumentNullException(nameof(organisation));

            try
            {
                var organisationFromCompaniesHouse = await _CompaniesHouseAPI.GetCompanyAsync(organisation.CompanyNumber);

                try
                {
                    await UpdateSicCodeAsync(organisation, organisationFromCompaniesHouse);

                    await UpdateAddressAsync(organisation, organisationFromCompaniesHouse);

                    await UpdateNameAsync(organisation, organisationFromCompaniesHouse);

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

        }

        public async Task UpdateSicCodeAsync(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse)
        {
            var companySicCodes = organisationFromCompaniesHouse.SicCodes ?? new List<string>();
            RetireExtraSicCodes(organisation, companySicCodes);
            AddNewSicCodes(organisation, companySicCodes);
        }

        public async Task UpdateAddressAsync(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse)
        {
            var companiesHouseAddress = organisationFromCompaniesHouse.RegisteredOfficeAddress;
            var newOrganisationAddressFromCompaniesHouse = await CreateOrganisationAddressFromCompaniesHouseAddressAsync(companiesHouseAddress);
            if (newOrganisationAddressFromCompaniesHouse.IsEmpty())return;

            var oldOrganisationAddress = organisation.GetLatestAddress();

            if (oldOrganisationAddress != null)
            {
                if (oldOrganisationAddress.AddressMatches(newOrganisationAddressFromCompaniesHouse)) return;
                oldOrganisationAddress.Status = AddressStatuses.Retired;
                oldOrganisationAddress.StatusDate = VirtualDateTime.Now;
                oldOrganisationAddress.Modified = VirtualDateTime.Now;
            }

            newOrganisationAddressFromCompaniesHouse.OrganisationId = organisation.OrganisationId;
            organisation.OrganisationAddresses.Add(newOrganisationAddressFromCompaniesHouse);
            organisation.LatestAddress = newOrganisationAddressFromCompaniesHouse;

            _dataRepository.Insert(newOrganisationAddressFromCompaniesHouse);
        }

        public async Task UpdateNameAsync(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse)
        {
            var companyNameFromCompaniesHouse = organisationFromCompaniesHouse.CompanyName;
            companyNameFromCompaniesHouse = companyNameFromCompaniesHouse?.Left(100);

            if (IsCompanyNameEqual(_organisationBusinessLogic.GetOrganisationName(organisation), companyNameFromCompaniesHouse)) return;

            var nameToAdd = new OrganisationName
            {
                Organisation = organisation,
                Name = companyNameFromCompaniesHouse,
                Source = SourceOfChange,
                Created = VirtualDateTime.Now
            };
            organisation.OrganisationNames.Add(nameToAdd);
            organisation.OrganisationName = companyNameFromCompaniesHouse;
            _dataRepository.Insert(nameToAdd);
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
        private void RetireExtraSicCodes(Organisation organisation, List<string> companySicCodes)
        {
            var sicCodeIds = _organisationBusinessLogic.GetOrganisationSicCodes(organisation).Select(sicCode => sicCode.SicCodeId);
            var newSicCodeIds =
                companySicCodes.Where(sicCode => !string.IsNullOrEmpty(sicCode)).Select(sicCode => int.Parse(sicCode));

            var idsToBeRetired = sicCodeIds.Except(newSicCodeIds);
            var sicCodesToBeRetired =
                organisation.OrganisationSicCodes.Where(s => idsToBeRetired.Contains(s.SicCodeId));
            foreach (var sicCodeToBeRetired in sicCodesToBeRetired) sicCodeToBeRetired.Retired = VirtualDateTime.Now;
        }

        private void AddNewSicCodes(Organisation organisation, List<string> companySicCodes)
        {
            var sicCodeIds = _organisationBusinessLogic.GetOrganisationSicCodes(organisation).Select(sicCode => sicCode.SicCodeId);
            var newSicCodeIds =
                companySicCodes.Where(sicCode => !string.IsNullOrEmpty(sicCode)).Select(sicCode => int.Parse(sicCode));

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
                }
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