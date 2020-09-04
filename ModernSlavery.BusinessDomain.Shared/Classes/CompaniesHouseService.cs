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

        public OrganisationAddress CreateOrganisationAddressFromCompaniesHouseAddress(
            CompaniesHouseAddress companiesHouseAddress)
        {
            var premisesAndLine1 = GetAddressLineFromPremisesAndAddressLine1(companiesHouseAddress);
            bool? isUkAddress = null;
            if (_PostcodeChecker.IsValidPostcode(companiesHouseAddress?.PostalCode).Result) isUkAddress = true;

            return new OrganisationAddress
            {
                Address1 = FirstHundredChars(companiesHouseAddress?.CareOf ?? premisesAndLine1),
                Address2 =
                    FirstHundredChars(companiesHouseAddress?.CareOf != null
                        ? premisesAndLine1
                        : companiesHouseAddress?.AddressLine2),
                Address3 = FirstHundredChars(companiesHouseAddress?.CareOf != null
                    ? companiesHouseAddress?.AddressLine2
                    : null),
                TownCity = FirstHundredChars(companiesHouseAddress?.Locality),
                County = FirstHundredChars(companiesHouseAddress?.Region),
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
                var organisationFromCompaniesHouse = _CompaniesHouseAPI.GetCompanyAsync(organisation.CompanyNumber).Result;

                if (organisation.OrganisationId == 1041) System.Diagnostics.Debugger.Break();
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
            var newOrganisationAddressFromCompaniesHouse =
                CreateOrganisationAddressFromCompaniesHouseAddress(companiesHouseAddress);
            var oldOrganisationAddress = _organisationBusinessLogic.GetOrganisationAddress(organisation);
            if (oldOrganisationAddress.AddressMatches(newOrganisationAddressFromCompaniesHouse)
                || IsNewOrganisationAddressNullOrEmpty(newOrganisationAddressFromCompaniesHouse))
                return;

            newOrganisationAddressFromCompaniesHouse.OrganisationId = organisation.OrganisationId;
            organisation.OrganisationAddresses.Add(newOrganisationAddressFromCompaniesHouse);
            organisation.LatestAddress = newOrganisationAddressFromCompaniesHouse;

            oldOrganisationAddress.Status = AddressStatuses.Retired;
            oldOrganisationAddress.StatusDate = VirtualDateTime.Now;
            oldOrganisationAddress.Modified = VirtualDateTime.Now;

            _dataRepository.Insert(newOrganisationAddressFromCompaniesHouse);
        }

        public async Task UpdateNameAsync(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse)
        {
            var companyNameFromCompaniesHouse = organisationFromCompaniesHouse.CompanyName;
            companyNameFromCompaniesHouse = FirstHundredChars(companyNameFromCompaniesHouse);

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

        private bool IsNewOrganisationAddressNullOrEmpty(OrganisationAddress address)
        {
            // Some organisations are not required to provide information to Companies House, and so we might get an empty
            // address. See https://wck2.companieshouse.gov.uk/goWCK/help/en/stdwc/excl_ch.html for more details. In other cases
            // organisations may have deleted their information when closing an organisation or merging with another.
            if (
                string.IsNullOrEmpty(address.Address1)
                && string.IsNullOrEmpty(address.Address2)
                && string.IsNullOrEmpty(address.Address3)
                && string.IsNullOrEmpty(address.TownCity)
                && string.IsNullOrEmpty(address.County)
                && string.IsNullOrEmpty(address.Country)
                && string.IsNullOrEmpty(address.PoBox)
                && string.IsNullOrEmpty(address.PostCode)
            )
                return true;

            return false;
        }

        private string GetAddressLineFromPremisesAndAddressLine1(CompaniesHouseAddress companiesHouseAddress)
        {
            return companiesHouseAddress?.Premises == null
                ? companiesHouseAddress?.AddressLine1
                : companiesHouseAddress?.Premises + "," + companiesHouseAddress?.AddressLine1;
        }

        private string FirstHundredChars(string str)
        {
            if (str == null) return null;

            return str.Substring(0, Math.Min(str.Length, 100));
        }

        #endregion
    }
}