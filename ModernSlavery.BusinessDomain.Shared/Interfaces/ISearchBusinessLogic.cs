using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.BusinessDomain.Shared.Interfaces
{
    public interface ISearchBusinessLogic
    {
        bool Disabled { get; }

        /// <summary>
        /// These are the stronly type appsettings for search
        /// </summary>
        SearchOptions SearchOptions { get; }
        
        /// <summary>
        /// This is the audit log for all search activities
        /// </summary>
        IAuditLogger SearchLog { get; }

        /// <summary>
        /// Retrieves all the search index documents from Azure Congitic Search for a specific organisation
        /// </summary>
        /// <param name="organisation">The organisation whos search index documents to retrieve</param>
        /// <param name="statementDeadlineYear">The reporting year whos search index documents to retrieve or 0 if all years</param>
        /// <returns>The list of search index documents from Azure</returns>
        Task<IEnumerable<OrganisationSearchModel>> ListSearchDocumentsAsync(Organisation organisation, int statementDeadlineYear = 0, bool keyOnly=true);

        /// <summary>
        /// Delete old search index documents for a specific organisation entity
        /// </summary>
        /// <param name="organisation">The organisation whos search index documents to remove</param>
        Task RemoveSearchDocumentsAsync(Organisation organisation);

        /// <summary>
        /// Delete old search index documents
        /// </summary>
        /// <param name="searchIndexes">The search index documents to remove</param>
        Task RemoveSearchDocumentsAsync(IEnumerable<OrganisationSearchModel> searchIndexes);

        /// <summary>
        /// Add new search index documents, update existing and delete old documents for all organisation entities in the database
        /// </summary>
        Task RefreshSearchDocumentsAsync();

        /// <summary>
        /// Add new search index documents, update existing and delete old documents for specified list of organisation entities
        /// </summary>
        /// <param name="organisations">The organisations whos search index documents we want to update</param>
        Task RefreshSearchDocumentsAsync(IEnumerable<Organisation> organisations);

        /// <summary>
        /// Adds new search index documents, update existing and delete old documents for a specific organisation entity
        /// </summary>
        /// <param name="organisation">The organisation whos search index documents to update</param>
        /// <param name="statementDeadlineYear">The reporting year whos search index documents to update or 0 if all years</param>
        Task RefreshSearchDocumentsAsync(Organisation organisation, int statementDeadlineYear = 0);


        Task<PagedResult<OrganisationSearchModel>> SearchDocumentsAsync(string searchText, int currentPage, int pageSize = 20, string searchFields = null, string selectFields = null, string orderBy = null, Dictionary<string, Dictionary<object, long>> facets = null, string filter = null, string highlights = null, SearchModes searchMode = SearchModes.Any);
    }
}