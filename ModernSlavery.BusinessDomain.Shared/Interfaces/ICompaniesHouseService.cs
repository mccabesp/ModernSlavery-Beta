using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Models.CompaniesHouse;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface ICompaniesHouseService
    {
        Task<OrganisationAddress> CreateOrganisationAddressFromCompaniesHouseAddressAsync(CompaniesHouseAddress companiesHouseAddress);

        Task UpdateOrganisationsAsync();
        Task UpdateOrganisationAsync(Organisation organisation);
        Task<bool> UpdateSicCodeAsync(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse);
        Task<bool> UpdateAddressAsync(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse);
        Task<bool> UpdateNameAsync(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse);

        bool AddressMatches(OrganisationAddress firstOrganisationAddress,OrganisationAddress secondOrganisationAddress);
        bool IsCompanyNameEqual(OrganisationName organisationName, string companyName);
        bool SicCodesEqual(IEnumerable<OrganisationSicCode> sicCodes, IEnumerable<string> companiesHouseSicCodes);
    }
}