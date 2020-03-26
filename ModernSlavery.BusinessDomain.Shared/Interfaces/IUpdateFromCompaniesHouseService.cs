using System.Collections.Generic;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Models.CompaniesHouse;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface IUpdateFromCompaniesHouseService
    {
        OrganisationAddress CreateOrganisationAddressFromCompaniesHouseAddress(
            CompaniesHouseAddress companiesHouseAddress);

        bool AddressMatches(OrganisationAddress firstOrganisationAddress,
            OrganisationAddress secondOrganisationAddress);

        bool IsCompanyNameEqual(OrganisationName organisationName, string companyName);
        bool SicCodesEqual(IEnumerable<OrganisationSicCode> sicCodes, IEnumerable<string> companiesHouseSicCodes);
        void UpdateOrganisationDetails(long organisationId);
        void UpdateSicCode(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse);
        void UpdateAddress(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse);
        void UpdateName(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse);
    }
}