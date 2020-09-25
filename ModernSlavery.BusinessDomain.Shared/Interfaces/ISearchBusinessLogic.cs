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
        /// <param name="submissionDeadlineYear">The reporting year whos search index documents to retrieve or 0 if all years</param>
        /// <returns>The list of search index documents from Azure</returns>
        Task<IEnumerable<OrganisationSearchModel>> ListSearchDocumentsAsync(Organisation organisation, int submissionDeadlineYear = 0, bool keyOnly=true);

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

        /// <summary>
        /// Returns an organisation statement summary for the specified reporting year
        /// </summary>
        /// <param name="parentOrganisationId">The Id of the organisation</param>
        /// <param name="submissionDeadlineYear">The reporting deadline year</param>
        /// <returns></returns>
        Task<OrganisationSearchModel> GetOrganisationAsync(long parentOrganisationId, int submissionDeadlineYear);

        /// <summary>
        /// Search index of statements by organisation name and return sorted and filtered results
        /// </summary>
        /// <param name="keywords">The organisatio name to search for</param>
        /// <param name="turnovers">A list of turnovers to apply as a filter</param>
        /// <param name="sectors">A list of sectors to apply as a filter</param>
        /// <param name="deadlineYears">A list of deadline years to apply as a filter</param>
        /// <param name="submittedOnly">Whether to return only only organisations who have submitted</param>
        /// <param name="returnFacets">Whether to return the faceted results (i.e., filter counts) </param>
        /// <param name="returnAllFields">When true returns all retrievable fields (for Api) otherwise returns only fields for viewing service</param>
        /// <param name="currentPage">The page of results to return</param>
        /// <param name="pageSize">The size of the results page</param>
        /// <returns></returns>
        Task<PagedSearchResult<OrganisationSearchModel>> SearchOrganisationsAsync(string keywords, IEnumerable<byte> turnovers = null, IEnumerable<short> sectors = null, IEnumerable<int> deadlineYears = null, bool submittedOnly = true, bool returnFacets = false, bool returnAllFields = false, int currentPage = 1, int pageSize = 20);

        /// <summary>
        /// Returns a list of group organisations reporting under the parent organisation for the specified reporting year
        /// </summary>
        /// <param name="parentOrganisationId">The Id of the parent organisation</param>
        /// <param name="submissionDeadlineYear">The reporting deadline year</param>
        /// <returns></returns>
        Task<IEnumerable<OrganisationSearchModel>> ListGroupOrganisationsAsync(long parentOrganisationId, int submissionDeadlineYear);
    }
}