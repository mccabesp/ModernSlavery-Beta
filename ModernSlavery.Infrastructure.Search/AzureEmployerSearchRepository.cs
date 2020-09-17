using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Autofac.Features.AttributeFilters;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
using Index = Microsoft.Azure.Search.Models.Index;

namespace ModernSlavery.Infrastructure.Search
{
    public class AzureOrganisationSearchRepository : ISearchRepository<OrganisationSearchModel>
    {
        private const string suggestorName = "sgOrgName";
        private const string synonymMapName = "desc-synonymmap";
        private readonly IMapper _autoMapper;
        private readonly Lazy<Task<ISearchIndexClient>> _indexClient;

        private readonly Lazy<Task<ISearchServiceClient>> _serviceClient;
        private readonly TelemetryClient _telemetryClient;
        public readonly IAuditLogger SearchLog;
        private readonly SharedOptions _sharedOptions;
        private readonly SearchOptions _searchOptions;

        public AzureOrganisationSearchRepository(
            SharedOptions sharedOptions,
            SearchOptions searchOptions,
            [KeyFilter(Filenames.SearchLog)] IAuditLogger searchLog,
            IMapper autoMapper,
            TelemetryClient telemetryClient = null)
        {
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _searchOptions = searchOptions ?? throw new ArgumentNullException(nameof(searchOptions));

            Disabled = searchOptions.Disabled;
            if (Disabled)
            {
                Console.WriteLine($"{nameof(AzureOrganisationSearchRepository)} is disabled");
                return;
            }
            _autoMapper = autoMapper;
            SearchLog = searchLog;
            IndexName = searchOptions.OrganisationIndexName.ToLower();

            if (string.IsNullOrWhiteSpace(searchOptions.ServiceName)) throw new ArgumentNullException(nameof(searchOptions.ServiceName));

#if DEBUG
            /* Since we cant emulate Search Index we share the same search search on Azure DEV environment.
             * Therefore we must use a different index name to the default when running locally in development mode
             * So as not to interfer with the default indexes.
             * Add your initials after the index name in your appsettings.secret.json file. eg.,
             *   "SearchService": {"OrganisationIndexName": "OrganisationSearchModel-SMc"} */
            if (searchOptions.OrganisationIndexName.EqualsI(nameof(OrganisationSearchModel)) && _sharedOptions.IsDevelopment()) throw new ArgumentException($"Config setting 'SearchService:OrganisationIndexName' cannot be '{nameof(OrganisationSearchModel)}' when running locally in Development mode");
#endif

            if (string.IsNullOrWhiteSpace(searchOptions.AdminApiKey) && string.IsNullOrWhiteSpace(searchOptions.QueryApiKey))
                throw new ArgumentNullException($"You must provide '{nameof(searchOptions.AdminApiKey)}' or '{nameof(searchOptions.QueryApiKey)}'");

            if (!string.IsNullOrWhiteSpace(searchOptions.AdminApiKey) && !string.IsNullOrWhiteSpace(searchOptions.QueryApiKey))
                throw new ArgumentException($"Cannot specify both '{nameof(searchOptions.AdminApiKey)}' and '{nameof(searchOptions.QueryApiKey)}'");

            _serviceClient = new Lazy<Task<ISearchServiceClient>>(
                async () =>
                {
                    //Get the service client
                    var _serviceClient = new SearchServiceClient(searchOptions.ServiceName, new SearchCredentials(searchOptions.AdminApiKey));

                    //Ensure the index exists
                    await CreateIndexIfNotExistsAsync(_serviceClient, IndexName).ConfigureAwait(false);

                    return _serviceClient;
                });

            _indexClient = new Lazy<Task<ISearchIndexClient>>(
                async () =>
                {
                    //Get the index client
                    if (!string.IsNullOrWhiteSpace(searchOptions.AdminApiKey))
                    {
                        var serviceClient = await _serviceClient.Value;
                        return serviceClient.Indexes.GetClient(IndexName);
                    }

                    if (!string.IsNullOrWhiteSpace(searchOptions.QueryApiKey))
                        return new SearchIndexClient(searchOptions.ServiceName, IndexName, new SearchCredentials(searchOptions.QueryApiKey));

                    throw new ArgumentNullException(
                        $"You must provide '{nameof(searchOptions.AdminApiKey)}' or '{nameof(searchOptions.QueryApiKey)}'");
                });
        }

        public bool Disabled { get; set; }
        public string IndexName { get; }

        public async Task CreateIndexIfNotExistsAsync(string indexName)
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            var serviceClient = await _serviceClient.Value;
            await CreateIndexIfNotExistsAsync(serviceClient, indexName);
        }

        private async Task CreateIndexIfNotExistsAsync(ISearchServiceClient serviceClient, string indexName)
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            if (await serviceClient.Indexes.ExistsAsync(indexName)) return;

            var index = new Index { Name = indexName, Fields = FieldBuilder.BuildForType<AzureOrganisationSearchModel>() };

            index.Suggesters = new List<Suggester>
            {
                new Suggester(
                    suggestorName,
                    nameof(OrganisationSearchModel.OrganisationName),
                    nameof(OrganisationSearchModel.Abbreviations))
            };

            var charFilterRemoveAmpersand = new MappingCharFilter("msu_remove_Ampersand", new List<string> { "&=>" });
            var charFilterRemoveDot = new MappingCharFilter("msu_remove_Dot", new List<string> { ".=>" });
            var charFilterRemoveLtdInfoCaseInsensitive = new PatternReplaceCharFilter(
                "msu_patternReplaceCharFilter_Ltd",
                "(?i)(limited|ltd|llp| uk|\\(uk\\)|-uk)[\\.]*",
                string.Empty); // case insensitive 'limited' 'ltd', 'llp', ' uk', '(uk)', '-uk' followed by zero or more dots (to cater for ltd. and some mis-punctuated limited..)
            var charFilterRemoveWhitespace = new PatternReplaceCharFilter(
                "msu_patternReplaceCharFilter_removeWhitespace",
                "\\s",
                string.Empty);

            index.CharFilters = new List<CharFilter>
            {
                charFilterRemoveAmpersand, charFilterRemoveDot, charFilterRemoveLtdInfoCaseInsensitive,
                charFilterRemoveWhitespace
            };

            var edgeNGramTokenFilterFront =
                new EdgeNGramTokenFilterV2("msu_edgeNGram_front", 3, 300, EdgeNGramTokenFilterSide.Front);
            var edgeNGramTokenFilterBack =
                new EdgeNGramTokenFilterV2("msu_edgeNGram_back", 3, 300, EdgeNGramTokenFilterSide.Back);
            index.TokenFilters = new List<TokenFilter> { edgeNGramTokenFilterFront, edgeNGramTokenFilterBack };

            var standardTokenizer = new StandardTokenizerV2("msu_standard_v2_tokenizer");
            var keywordTokenizer = new KeywordTokenizerV2("msu_keyword_v2_tokenizer");

            index.Tokenizers = new List<Tokenizer> { standardTokenizer, keywordTokenizer };

            var suffixAnalyzer = new CustomAnalyzer(
                "msu_suffix",
                standardTokenizer.Name,
                new List<TokenFilterName> { TokenFilterName.Lowercase, edgeNGramTokenFilterBack.Name },
                new List<CharFilterName> { charFilterRemoveAmpersand.Name, charFilterRemoveLtdInfoCaseInsensitive.Name });

            var completeTokenAnalyzer = new CustomAnalyzer(
                "msu_prefix_completeToken",
                keywordTokenizer.Name,
                new List<TokenFilterName> { TokenFilterName.Lowercase, edgeNGramTokenFilterFront.Name },
                new List<CharFilterName>
                {
                    charFilterRemoveDot.Name,
                    charFilterRemoveAmpersand.Name,
                    charFilterRemoveLtdInfoCaseInsensitive.Name,
                    charFilterRemoveWhitespace.Name
                });

            index.Analyzers = new List<Analyzer> { suffixAnalyzer, completeTokenAnalyzer };

            index.Fields.First(f => f.Name == nameof(OrganisationSearchModel.PartialNameForSuffixSearches)).Analyzer =
                suffixAnalyzer.Name;
            index.Fields.First(f => f.Name == nameof(OrganisationSearchModel.PartialNameForSuffixSearches)).SynonymMaps =
                new[] { synonymMapName };

            index.Fields.First(f => f.Name == nameof(OrganisationSearchModel.PartialNameForCompleteTokenSearches))
                    .Analyzer =
                completeTokenAnalyzer.Name;
            index.Fields.First(f => f.Name == nameof(OrganisationSearchModel.PartialNameForCompleteTokenSearches))
                    .SynonymMaps =
                new[] { synonymMapName };

            index.Fields.First(f => f.Name == nameof(OrganisationSearchModel.OrganisationName)).SynonymMaps = new[] { synonymMapName };

            //Add the synonyms if they dont already exist
            if (!await serviceClient.SynonymMaps.ExistsAsync(synonymMapName))
                serviceClient.SynonymMaps.CreateOrUpdate(
                    new SynonymMap
                    {
                        Name = synonymMapName,
                        //Format = "solr", cannot set after upgrade from v5.03 to version 9.0.0
                        Synonyms = "coop, co-operative"
                    });

            await serviceClient.Indexes.CreateAsync(index);
        }

        public async Task AddOrUpdateDocumentsAsync(IEnumerable<OrganisationSearchModel> newRecords)
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            if (newRecords == null || !newRecords.Any())
                throw new ArgumentNullException(nameof(newRecords), "You must supply at least one record to index");

            //Remove all test organisations
            if (!string.IsNullOrWhiteSpace(_sharedOptions.TestPrefix))
                newRecords = newRecords.Where(e => !e.OrganisationName.StartsWithI(_sharedOptions.TestPrefix));

            //Ensure the records are ordered by name
            newRecords = newRecords.OrderBy(o => o.OrganisationName);

            //Set the records to add or update
            var actions = newRecords.Select(r => IndexAction.MergeOrUpload(_autoMapper.Map<AzureOrganisationSearchModel>(r)))
                .ToList();

            var batches = new ConcurrentBag<IndexBatch<AzureOrganisationSearchModel>>();
            while (actions.Any())
            {
                var batchSize = actions.Count > 1000 ? 1000 : actions.Count;
                var batch = IndexBatch.New(actions.Take(batchSize).ToList());
                batches.Add(batch);
                actions.RemoveRange(0, batchSize);
            }

            var indexClient = await _indexClient.Value;

            Parallel.ForEach(
                batches,
                batch =>
                {
                    var retries = 0;
                    retry:
                    try
                    {
                        indexClient.Documents.Index(batch);
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

        public async Task<int> DeleteDocumentsAsync(IEnumerable<OrganisationSearchModel> oldRecords)
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            if (oldRecords == null || !oldRecords.Any())
                throw new ArgumentNullException(nameof(oldRecords), "You must supply at least one record to index");

            //Set the records to add or update
            var actions = oldRecords.Select(r => IndexAction.Delete(_autoMapper.Map<AzureOrganisationSearchModel>(r))).ToList();

            var batches = new ConcurrentBag<IndexBatch<AzureOrganisationSearchModel>>();

            while (actions.Any())
            {
                var batchSize = actions.Count > 1000 ? 1000 : actions.Count;
                var batch = IndexBatch.New(actions.Take(batchSize).ToList());
                batches.Add(batch);
                actions.RemoveRange(0, batchSize);
            }

            var deleteCount = 0;

            var exceptions = new ConcurrentBag<Exception>();
            var indexClient = await _indexClient.Value;

            await batches.WaitForAllAsync(
                async batch =>
                {
                    var retries = 0;
                    retry:
                    try
                    {
                        await indexClient.Documents.IndexAsync(batch);
                        Interlocked.Add(ref deleteCount, batch.Actions.Count());
                    }
                    catch (IndexBatchException e)
                    {
                        if (retries < 30)
                        {
                            retries++;
                            Thread.Sleep(1000);
                            goto retry;
                        }

                        exceptions.Add(e);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });
            if (exceptions.Count > 0) throw new AggregateException(exceptions);

            return deleteCount;
        }

        public async Task<OrganisationSearchModel> GetDocumentAsync(string key, string selectFields = null)
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            //Limit result fields
            var selectedFields = string.IsNullOrWhiteSpace(selectFields) ? null : selectFields.SplitI().ToList();

            var indexClient = await _indexClient.Value;

            var result = await indexClient.Documents.GetAsync<OrganisationSearchModel>(key, selectedFields);

            return result;
        }

        public async Task<IList<OrganisationSearchModel>> ListDocumentsAsync(string selectFields = null, string filter=null)
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            long totalPages = 0;
            var currentPage = 1;
            var resultsList = new List<OrganisationSearchModel>();
            do
            {
                var searchResults = await SearchDocumentsAsync(null,currentPage,selectFields: selectFields, filter:filter);
                totalPages = searchResults.PageCount;
                resultsList.AddRange(searchResults.Results);
                currentPage++;
            } while (currentPage < totalPages);

            return resultsList;
        }

        public async Task<long> GetDocumentCountAsync()
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            var serviceClient = await _serviceClient.Value;

            if (!await serviceClient.Indexes.ExistsAsync(IndexName)) return 0;

            var searchResults = await SearchDocumentsAsync(null, 1);
            return searchResults.ActualRecordTotal;
        }

        public async Task<PagedResult<OrganisationSearchModel>> SearchDocumentsAsync(string searchText,
            int currentPage,
            int pageSize = 20,
            string searchFields = null,
            string selectFields = null,
            string orderBy = null,
            Dictionary<string, Dictionary<object, long>> facets = null,
            string filter = null,
            string highlights = null,
            SearchModes searchMode = SearchModes.Any)
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            var indexClient = await _indexClient.Value;

            // Execute search based on query string
            var sp = new SearchParameters
            {
                SearchMode = searchMode.Equals("any") ? SearchMode.Any : SearchMode.All,
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
            var
                results = await indexClient.Documents.SearchAsync<OrganisationSearchModel>(searchText, sp);

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
                    {"SearchParameters", HttpUtility.UrlDecode(sp.ToString())}
                };

                _telemetryClient?.TrackEvent("msu_Search", telemetryProperties);

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
            var searchResults = new PagedResult<OrganisationSearchModel>
            {
                Results = results.Results.Select(r => r.Document).ToList(),
                CurrentPage = currentPage,
                PageSize = pageSize,
                ActualRecordTotal = totalRecords,
                VirtualRecordTotal = totalRecords
            };

            return searchResults;
        }

    }
}