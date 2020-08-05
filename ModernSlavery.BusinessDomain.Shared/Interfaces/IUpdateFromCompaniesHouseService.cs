using System.Collections.Generic;
using System.Threading.Tasks;
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
        Task UpdateOrganisationDetailsAsync(Organisation organisation);
        Task UpdateSicCodeAsync(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse);
        Task UpdateAddressAsync(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse);
        Task UpdateNameAsync(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse);
    }
}