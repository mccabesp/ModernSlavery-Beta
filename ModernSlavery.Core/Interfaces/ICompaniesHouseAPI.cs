using System.Threading.Tasks;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.CompaniesHouse;

namespace ModernSlavery.Core.Interfaces
{
    public interface ICompaniesHouseAPI
    {
        int MaxRecords { get;}
        Task<PagedResult<EmployerRecord>> SearchEmployersAsync(string searchText, int page, int pageSize, bool test = false);
        Task<string> GetSicCodesAsync(string companyNumber);
        Task<CompaniesHouseCompany> GetCompanyAsync(string companyNumber);

    }
}