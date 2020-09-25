using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Core.Interfaces
{
    public interface ISearchRepository<T>
    {
        public bool Disabled { get; set; }
        public string IndexName { get; }

        /// <summary>
        ///     Create the default index if it doesnt already exist
        /// </summary>
        /// <returns>The existing or new index</returns>
        Task CreateIndexIfNotExistsAsync(string indexName);

        /// <summary>
        ///     Adds all new records to index
        /// </summary>
        /// <param name="newRecords">The new or existing records which should be indexed.</param>
        Task AddOrUpdateDocumentsAsync(IEnumerable<T> records);
        
        /// <summary>
        ///     Removes old records from index
        /// </summary>
        /// <param name="oldRecords">The old records which should be deleted from the index.</param>
        Task<int> DeleteDocumentsAsync(IEnumerable<T> records);

        Task<T> GetDocumentAsync(string key, string selectFields = null);

        Task<IList<T>> ListDocumentsAsync(string selectFields = null, string filter=null);

        
        Task<long> GetDocumentCountAsync();

        /// <summary>
        ///     Executes an advanced search using pagination, sorting, filters, facets, and highlighting.
        /// </summary>
        /// <param name="searchText">The text used for the search. When empty all results are returned.</param>
        /// <param name="totalRecords">The returned total number of records in the results</param>
        /// <param name="currentPage">The current page of results to return</param>
        /// <param name="pageSize">The size of the result set to return (default=20). Maximum is 1000.</param>
        /// <param name="filter">
        ///     A set of comma or semicolon separated field names to searching.
        ///     Only fields marked with the 'IsSearchable' attribute can be included.
        ///     The default is empty and all searchable fields will be searched.
        ///     ///
        /// </param>
        /// <param name="selectFields"></param>
        /// A set of comma or semicolon separated field names to return values for.
        /// Default is empty and will return all field values
        /// <param name="orderBy">
        ///     A set of comma or semicolon separated sort terms.
        ///     Default is empty and will return results sorted by score relevance.
        ///     For example, OrganisationName, SicName DESC
        ///     Only fields marked with the 'IsSortable' attribute can be included.
        /// </param>
        /// <param name="facets">
        ///     Specifies the facets to query and returns the facet results
        ///     The default is empty and no facets will be applied.
        ///     Only fields marked with the 'IsFacetable' attribute can be included.
        ///     Call by specifing field names as keys in the dictionary.
        ///     The resulting dictionary for each field returns all possible values and their count for that field.
        ///     ///
        /// </param>
        /// <param name="filter">
        ///     A filter expression using OData syntax (see
        ///     https://docs.microsoft.com/en-us/rest/api/searchservice/odata-expression-syntax-for-azure-search)
        ///     The default is empty and no filter will be applied.
        ///     Only fields marked with the 'IsFilterable' attribute can be included.
        ///     String comparisons are case sensitive.
        ///     You can also use the operators '==','!=', '>=', '>', '<=', '<', '&&', '||' which will be automatically replaced with OData counterparts 'EQ','NE', 'GE', 'GT', 'LE', 'LT', 'AND', 'OR'.
        /// Special functions also include search.in(myfield, 'a, b, c')
        /// /// </param>
        /// <param name="highlights">
        ///     A set of comma or semicolon separated field names used for hit highlights.
        ///     Only fields marked with the 'IsSearchable' attribute can be included.
        ///     By default, Azure Search returns up to 5 highlights per field.
        ///     The limit is configurable per field by appending -
        ///     <max # of highlights>
        ///         following the field name.
        ///         For example, highlight=title-3,description-10 returns up to 3 highlighted hits from the title field and up to
        ///         10 hits from the description field. <max # of highlights> must be an integer between 1 and 1000 inclusive.
        /// </param>
        Task<PagedSearchResult<T>> SearchDocumentsAsync(string searchText,
            int currentPage,
            int pageSize = 20,
            string searchFields = null,
            string selectFields = null,
            string facetFields = null,
            string orderBy = null,
            string filter = null,
            string highlights = null,
            SearchModes searchMode = SearchModes.Any);
    }
}