using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel;
using Index = Microsoft.Azure.Search.Models.Index;

namespace ModernSlavery.Infrastructure.Search
{
    public class AzureSicCodeSearchRepository : ISearchRepository<SicCodeSearchModel>
    {
        protected readonly Lazy<Task<ISearchServiceClient>> _searchServiceClient;
        private readonly TelemetryClient _telemetryClient;
        public readonly IAuditLogger SearchLog;
        protected Lazy<Task<ISearchIndexClient>> _searchIndexClient;
        protected string _suggesterName;


        public AzureSicCodeSearchRepository(
            [KeyFilter(Filenames.SearchLog)] IAuditLogger searchLog,
            ISearchServiceClient searchServiceClient, string indexName, bool disabled = false)
        {
            Disabled = disabled;
            if (disabled)
            {
                Console.WriteLine($"{nameof(AzureSicCodeSearchRepository)} is disabled");
                return;
            }

            SearchLog = searchLog;
            IndexName = indexName;

            _searchServiceClient = new Lazy<Task<ISearchServiceClient>>(
                async () =>
                {
                    //Ensure the index exists
                    await CreateIndexIfNotExistsAsync(searchServiceClient, IndexName).ConfigureAwait(false);

                    return searchServiceClient;
                });

            if (Disabled) return;

            _suggesterName = "sicCodeSuggester";

            _searchIndexClient = new Lazy<Task<ISearchIndexClient>>(
                async () =>
                {
                    var serviceClient = await _searchServiceClient.Value;
                    return serviceClient.Indexes?.GetClient(indexName);
                });
        }

        public string IndexName { get; }

        public bool Disabled { get; set; }

        public async Task AddOrUpdateIndexDataAsync(IEnumerable<SicCodeSearchModel> newRecords)
        {
            if (newRecords == null || !newRecords.Any())
                throw new ArgumentNullException(nameof(newRecords), "You must supply at least one record to index");

            //Set the records to add or update
            var actions = newRecords.Cast<AzureSicCodeSearchModel>().Select(IndexAction.MergeOrUpload).ToList();

            var batches = new ConcurrentBag<IndexBatch<AzureSicCodeSearchModel>>();
            while (actions.Any())
            {
                var batchSize = actions.Count > 1000 ? 1000 : actions.Count;
                var batch = IndexBatch.New(actions.Take(batchSize).ToList());
                batches.Add(batch);
                actions.RemoveRange(0, batchSize);
            }

            var searchIndexClient = await _searchIndexClient.Value;

            Parallel.ForEach(
                batches,
                batch =>
                {
                    var retries = 0;
                    retry:
                    try
                    {
                        searchIndexClient.Documents.Index(batch);
                    }
                    catch (IndexBatchException)
                    {
                        if (retries < 30)
                        {
                            retries++;
                            Thread.Sleep(1000);
                            goto retry;
                        }

                        throw;
                    }
                });
        }


        public async Task RefreshIndexDataAsync(IEnumerable<SicCodeSearchModel> listOfRecordsToAddOrUpdate)
        {
            //Add (or update) the records to the index
            await AddOrUpdateIndexDataAsync(listOfRecordsToAddOrUpdate);

            //Get the old records which will need deleting
            var currentListOfDocumentsInIndex = await ListAsync();
            var listOfDocumentsNotPartOfThisUpdate = currentListOfDocumentsInIndex.Except(listOfRecordsToAddOrUpdate);

            //Delete the old records
            if (listOfDocumentsNotPartOfThisUpdate.Any())
                await RemoveFromIndexAsync(listOfDocumentsNotPartOfThisUpdate);
        }

        /// <summary>
        ///     Returns a list of search suggestions based on input text
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="fuzzy">
        ///     Gets or sets a value indicating whether to use fuzzy matching for the suggestion
        ///     query. Default is true. when set to true, the query will find suggestions even
        ///     if there's a substituted or missing character in the search text. While this
        ///     provides a better experience in some scenarios it comes at a performance cost
        ///     as fuzzy suggestion searches are slower and consume more resources.
        /// </param>
        /// <param name="maxRecords">Maximum number of suggestions to return (default=10)</param>
        /// </param>
        public async Task<IEnumerable<KeyValuePair<string, SicCodeSearchModel>>> SuggestAsync(string searchText,
            string searchFields = null,
            string selectFields = null,
            bool fuzzy = true,
            int maxRecords = 10)
        {
            if (string.IsNullOrEmpty(searchText?.Trim())) return new List<KeyValuePair<string, SicCodeSearchModel>>();

            // Execute search based on query string
            var sp = new SuggestParameters {UseFuzzyMatching = fuzzy, Top = maxRecords};

            //Specify the fields to search
            if (!string.IsNullOrWhiteSpace(searchFields)) sp.SearchFields = searchFields.SplitI().ToList();

            //Limit result fields
            if (!string.IsNullOrWhiteSpace(selectFields)) sp.Select = selectFields.SplitI().ToList();

            var searchIndexClient = await _searchIndexClient.Value;

            var results =
                await searchIndexClient.Documents.SuggestAsync<SicCodeSearchModel>(searchText, _suggesterName, sp);
            var suggestions =
                results?.Results.Select(s => new KeyValuePair<string, SicCodeSearchModel>(s.Text, s.Document));
            return suggestions;
        }

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
        public async Task<PagedResult<SicCodeSearchModel>> SearchAsync(string searchText,
            int currentPage,
            SearchTypes searchType,
            int pageSize = 20,
            string searchFields = null,
            string selectFields = null,
            string orderBy = null,
            Dictionary<string, Dictionary<object, long>> facets = null,
            string filter = null,
            string highlights = null,
            SearchModes searchMode = SearchModes.Any)
        {
            // Execute search based on query string
            var sp = new SearchParameters
            {
                SearchMode = (SearchMode) searchMode,
                Top = pageSize,
                Skip = (currentPage - 1) * pageSize,
                IncludeTotalResultCount = true,
                QueryType = QueryType.Simple
            };

            //Specify the fields to search
            if (!string.IsNullOrWhiteSpace(searchFields)) sp.SearchFields = searchFields.SplitI().ToList();

            //Limit result fields
            if (!string.IsNullOrWhiteSpace(selectFields)) sp.Select = selectFields.SplitI().ToList();

            // Define the sort type or order by relevance score
            if (!string.IsNullOrWhiteSpace(orderBy) && !orderBy.EqualsI("Relevance", "Relevance desc", "Relevance asc"))
                sp.OrderBy = orderBy.SplitI().ToList();

            // Add filtering
            sp.Filter = string.IsNullOrWhiteSpace(filter) ? null : filter;

            //Add facets
            if (facets != null && facets.Count > 0) sp.Facets = facets.Keys.ToList();

            //Execute the search
            var searchIndexClient = await _searchIndexClient.Value;
            var results = searchIndexClient.Documents.Search<SicCodeSearchModel>(searchText, sp);

            //Return the total records
            var totalRecords = results.Count.Value;

            /* There are too many empty searches being executed (about 1200). This needs further investigation to see if/how they can be reduced */
            if (!string.IsNullOrEmpty(searchText))
            {
                var telemetryProperties = new Dictionary<string, string>
                {
                    {"TimeStamp", VirtualDateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")},
                    {"QueryTerms", searchText},
                    {"ResultCount", totalRecords.ToString()},
                    {"Top", sp.Top.ToString()},
                    {"Skip", sp.Skip.ToString()}
                };

                _telemetryClient?.TrackEvent("Gpg_Search", telemetryProperties);

                await SearchLog.WriteAsync(telemetryProperties);
            }

            //Return the facet results
            if (sp.Facets != null && sp.Facets.Any())
                foreach (var facetGroupKey in results.Facets.Keys)
                {
                    if (facets[facetGroupKey] == null) facets[facetGroupKey] = new Dictionary<object, long>();

                    foreach (var facetResult in results.Facets[facetGroupKey])
                        facets[facetGroupKey][facetResult.Value] = facetResult.Count.Value;
                }

            //Return the results
            var searchResults = new PagedResult<SicCodeSearchModel>
            {
                Results = results.Results.Select(r => r.Document).ToList(),
                CurrentPage = currentPage,
                PageSize = pageSize,
                ActualRecordTotal = totalRecords,
                VirtualRecordTotal = totalRecords
            };

            return searchResults;
        }

        /// <summary>
        ///     Removes old records from index
        /// </summary>
        /// <param name="oldRecords">The old records which should be deleted from the index.</param>
        public async Task<int> RemoveFromIndexAsync(IEnumerable<SicCodeSearchModel> oldRecords)
        {
            if (oldRecords == null || !oldRecords.Any())
                throw new ArgumentNullException(nameof(oldRecords), "You must supply at least one record to index");

            //Set the records to add or update
            var actions = oldRecords.Select(IndexAction.Delete).ToList();

            var batches = new ConcurrentBag<IndexBatch<SicCodeSearchModel>>();

            while (actions.Any())
            {
                var batchSize = actions.Count > 1000 ? 1000 : actions.Count;
                var batch = IndexBatch.New(actions.Take(batchSize).ToList());
                batches.Add(batch);
                actions.RemoveRange(0, batchSize);
            }

            var deleteCount = 0;
            var searchIndexClient = await _searchIndexClient.Value;

            Parallel.ForEach(
                batches,
                batch =>
                {
                    var retries = 0;
                    retry:
                    try
                    {
                        searchIndexClient.Documents.Index(batch);
                        Interlocked.Add(ref deleteCount, batch.Actions.Count());
                    }
                    catch (IndexBatchException)
                    {
                        if (retries < 30)
                        {
                            retries++;
                            Thread.Sleep(1000);
                            goto retry;
                        }

                        throw;
                    }
                });
            return deleteCount;
        }

        public async Task<IList<SicCodeSearchModel>> ListAsync(string selectFields = null)
        {
            long totalPages = 0;
            var currentPage = 1;
            var resultsList = new List<SicCodeSearchModel>();

            do
            {
                var searchResults =
                    await SearchAsync(null, currentPage, SearchTypes.NotSet, selectFields: selectFields);
                totalPages = searchResults.PageCount;
                resultsList.AddRange(searchResults.Results);
                currentPage++;
            } while (currentPage < totalPages);

            return resultsList;
        }

        public Task<SicCodeSearchModel> GetAsync(string key, string selectFields = null)
        {
            throw new NotImplementedException();
        }

        public Task<long> GetDocumentCountAsync()
        {
            throw new NotImplementedException();
        }

        public Task CreateIndexIfNotExistsAsync(string indexName)
        {
            throw new NotImplementedException();
        }

        private async Task CreateIndexIfNotExistsAsync(ISearchServiceClient searchServiceClient,
            string sicCodesIndexName)
        {
            if (searchServiceClient.Indexes == null ||
                await searchServiceClient.Indexes.ExistsAsync(sicCodesIndexName)) return;

            var index = new Index
            {
                Name = sicCodesIndexName,
                Fields = FieldBuilder.BuildForType<SicCodeSearchModel>(),
                Suggesters = new List<Suggester>
                {
                    new Suggester(
                        _suggesterName,
                        nameof(SicCodeSearchModel.SicCodeDescription),
                        nameof(SicCodeSearchModel.SicCodeListOfSynonyms))
                }
            };

            await searchServiceClient.Indexes.CreateAsync(index);
        }
    }
}