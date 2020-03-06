using System.Threading.Tasks;
using ModernSlavery.Core.Classes;

namespace ModernSlavery.Core.Interfaces
{
    public interface IPagedRepository<T>
    {

        void Insert(T entity);
        void Delete(T entity);
        void ClearSearch();

        Task<PagedResult<T>> SearchAsync(string searchText, int page, int pageSize, bool test = false);

        Task<string> GetSicCodesAsync(string companyNumber);

    }
}
