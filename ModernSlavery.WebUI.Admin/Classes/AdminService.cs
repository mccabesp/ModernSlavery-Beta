using System.Threading.Tasks;
using ModernSlavery.BusinessLogic;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.WebUI.Admin.Classes
{
    public interface IAdminService
    {
        public ICommonBusinessLogic CommonBusinessLogic { get; }

        Task<long> GetSearchDocumentCountAsync();

    }

    public class AdminService : IAdminService
    {
        private readonly ISearchRepository<EmployerSearchModel> searchRepository;
        public ICommonBusinessLogic CommonBusinessLogic { get; }
        public AdminService(ICommonBusinessLogic commonBusinessLogic, IDataRepository dataRepository, ISearchRepository<EmployerSearchModel> searchRepository)
        {
            CommonBusinessLogic = commonBusinessLogic;
            this.searchRepository = searchRepository;
        }

        public async Task<long> GetSearchDocumentCountAsync()
        {
            return await searchRepository.GetDocumentCountAsync();
        }

    }
}
