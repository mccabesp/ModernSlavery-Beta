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
        /// <summary>
        /// These are the stronly type appsettings for search
        /// </summary>
        SearchOptions SearchOptions { get; }
        
        /// <summary>
        /// This is the audit log for all search activities
        /// </summary>
        IAuditLogger SearchLog { get; }

        /// <summary>
        /// This is the repository for talking to the Azure Cognitive Search API
        /// </summary>
        ISearchRepository<OrganisationSearchModel> OrganisationSearchRepository { get; set; }

        /// <summary>
        /// Retrieves all the search index documents from Azure Congitic Search for a specific organisation
        /// </summary>
        /// <param name="organisation">The organisation whos search index documents to retrieve</param>
        /// <returns>The list of search index documents from Azure</returns>
        Task<IEnumerable<OrganisationSearchModel>> GetOrganisationSearchIndexesAsync(Organisation organisation, bool keyOnly=true);

        /// <summary>
        /// Delete old search index documents for a specific organisation entity
        /// </summary>
        /// <param name="organisation">The organisation whos search index documents to remove</param>
        Task RemoveOrganisationSearchIndexesAsync(Organisation organisation);

        /// <summary>
        /// Delete old search index documents
        /// </summary>
        /// <param name="searchIndexes">The search index documents to remove</param>
        Task RemoveSearchIndexesAsync(IEnumerable<OrganisationSearchModel> searchIndexes);
        
        /// <summary>
        /// Add new search index documents, update existing and delete old documents for all organisation entities in the database
        /// </summary>
        Task UpdateOrganisationSearchIndexAsync();

        /// <summary>
        /// Add new search index documents, update existing and delete old documents for specified list of organisation entities
        /// </summary>
        /// <param name="organisations">The organisations whos search index documents we want to update</param>
        Task UpdateOrganisationSearchIndexAsync(IEnumerable<Organisation> organisations);

        /// <summary>
        /// Adds new search index documents, update existing and delete old documents for a specific organisation entity
        /// </summary>
        /// <param name="organisation">The organisation whos search index documents to update</param>
        Task UpdateOrganisationSearchIndexAsync(Organisation organisation);
    }
}