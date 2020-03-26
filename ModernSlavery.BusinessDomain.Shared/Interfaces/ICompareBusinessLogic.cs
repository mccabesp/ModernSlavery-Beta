using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ModernSlavery.BusinessDomain.Shared.Models;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface ICompareBusinessLogic
    {
        DataTable GetCompareDatatable(IEnumerable<CompareReportModel> data);

        Task<IEnumerable<CompareReportModel>> GetCompareDataAsync(
            IEnumerable<string> encBasketOrgIds,
            int year,
            string sortColumn,
            bool sortAscending);
    }
}