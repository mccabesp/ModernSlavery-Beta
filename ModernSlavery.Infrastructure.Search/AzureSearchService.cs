using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Infrastructure.Search
{
    public class AzureSearchService
    {
        public ISearchRepository<EmployerSearchModel> SearchRepository;
        public ISearchRepository<SicCodeSearchModel> SicCodeSearchRepository;

        public AzureSearchService(ISearchRepository<EmployerSearchModel> searchRepository,
            ISearchRepository<SicCodeSearchModel> sicCodeSearchRepository)
        {
            SearchRepository = searchRepository;
            SicCodeSearchRepository = sicCodeSearchRepository;
        }
    }
}