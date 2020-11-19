using System.Threading.Tasks;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.CompaniesHouse;

namespace ModernSlavery.Core.Interfaces
{
    public interface ICompaniesHouseAPI
    {
        Task<PagedResult<OrganisationRecord>> SearchOrganisationsAsync(string searchText, int page, int pageSize, int maxRecords);

        Task<string> GetSicCodesAsync(string companyNumber);
        Task<CompaniesHouseCompany> GetCompanyAsync(string companyNumber);
    }
}