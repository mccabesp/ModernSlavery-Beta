using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Infrastructure.Search
{
    public class AzureSearchService
    {
        public ISearchRepository<OrganisationSearchModel> SearchRepository;

        public AzureSearchService(ISearchRepository<OrganisationSearchModel> searchRepository)
        {
            SearchRepository = searchRepository;
        }
    }
}