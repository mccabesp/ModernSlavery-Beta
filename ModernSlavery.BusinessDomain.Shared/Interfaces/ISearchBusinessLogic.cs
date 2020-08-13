using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface ISearchBusinessLogic
    {
        SearchOptions SearchOptions { get; }
        IAuditLogger SearchLog { get; }
        ISearchRepository<OrganisationSearchModel> OrganisationSearchRepository { get; set; }

        Task<IEnumerable<OrganisationSearchModel>> GetOrganisationSearchIndexesAsync(Organisation organisation);
        Task RemoveOrganisationSearchIndexesAsync(Organisation organisation);
        Task RemoveSearchIndexesAsync(IEnumerable<OrganisationSearchModel> searchIndexes);
        Task UpdateOrganisationSearchIndexAsync();
        Task UpdateOrganisationSearchIndexAsync(IEnumerable<Organisation> organisations);
        Task UpdateOrganisationSearchIndexAsync(Organisation organisation);
    }
}