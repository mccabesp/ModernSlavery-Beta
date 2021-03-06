﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using AutoMapper;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Rest.Azure;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Core.Options;
using Index = Microsoft.Azure.Search.Models.Index;

namespace ModernSlavery.Infrastructure.Search
{
    public class AzureOrganisationSearchRepository : ISearchRepository<OrganisationSearchModel>
    {
        private const string suggestorName = "sgOrgName";
        private const string synonymMapName = "desc-synonymmap";

        private readonly HttpClient _httpClient;
        private Lazy<Task<ISearchServiceClient>> _serviceClient;
        private Lazy<Task<ISearchIndexClient>> _indexClient;

        public readonly IAuditLogger SearchLog;
        private readonly SharedOptions _sharedOptions;
        private readonly SearchOptions _searchOptions;

        public AzureOrganisationSearchRepository(
            SharedOptions sharedOptions,
            SearchOptions searchOptions,
            HttpClient httpClient,
            [KeyFilter(Filenames.SearchLog)] IAuditLogger searchLog,
            IMapper autoMapper)
        {
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _searchOptions = searchOptions ?? throw new ArgumentNullException(nameof(searchOptions));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            Disabled = searchOptions.Disabled;
            if (Disabled)
            {
                Console.WriteLine($"{nameof(AzureOrganisationSearchRepository)} is disabled");
                return;
            }
            SearchLog = searchLog;

            IndexName = searchOptions.OrganisationIndexName.ToLower();
            if (string.IsNullOrWhiteSpace(searchOptions.ServiceName)) throw new ArgumentNullException(nameof(searchOptions.ServiceName));

#if DEBUG || DEBUGLOCAL
            /* Since we cant emulate Search Index we share the same search search on Azure DEV environment.
              Therefore we must use a different index name to the default when running locally in development mode
              So as not to interfer with the default indexes.
              Add your initials after the index name in your appsettings.secret.json file. eg., "SearchService": {"OrganisationIndexName": "OrganisationSearchModel-SMc"} */

            if (searchOptions.OrganisationIndexName.EqualsI(nameof(OrganisationSearchModel)) && _sharedOptions.IsDevelopment()) throw new ArgumentException($"Config setting 'SearchService:OrganisationIndexName' cannot be '{nameof(OrganisationSearchModel)}' when running locally in Development mode");
#endif

            if (string.IsNullOrWhiteSpace(searchOptions.AdminApiKey) && string.IsNullOrWhiteSpace(searchOptions.QueryApiKey))
                throw new ArgumentNullException($"You must provide '{nameof(searchOptions.AdminApiKey)}' or '{nameof(searchOptions.QueryApiKey)}'");

            if (!string.IsNullOrWhiteSpace(searchOptions.AdminApiKey) && !string.IsNullOrWhiteSpace(searchOptions.QueryApiKey))
                throw new ArgumentException($"Cannot specify both '{nameof(searchOptions.AdminApiKey)}' and '{nameof(searchOptions.QueryApiKey)}'");

            SetServiceClient();

            SetIndexClient();
        }

        void SetServiceClient()
        {
            _serviceClient = new Lazy<Task<ISearchServiceClient>>(
                async () =>
                {
                    //Get the service client
                    var serviceClient = new SearchServiceClient(new SearchCredentials(_searchOptions.AdminApiKey),_httpClient,true);
                    serviceClient.SearchServiceName = _searchOptions.ServiceName;

                    //Ensure the index exists
                    await CreateIndexIfNotExistsAsync(serviceClient, IndexName).ConfigureAwait(false);

                    return serviceClient;
                });
        }

        void SetIndexClient()
        {
            _indexClient = new Lazy<Task<ISearchIndexClient>>(
                async () =>
                {
                    //Get the index client
                    if (!string.IsNullOrWhiteSpace(_searchOptions.AdminApiKey))
                    {
                        var serviceClient = await _serviceClient.Value.ConfigureAwait(false);
                        return serviceClient.Indexes.GetClient(IndexName);
                    }

                    if (!string.IsNullOrWhiteSpace(_searchOptions.QueryApiKey))
                    {
                        var indexClient = new SearchIndexClient(new SearchCredentials(_searchOptions.QueryApiKey),_httpClient,true);
                        indexClient.IndexName = IndexName;

                        return indexClient;
                    }

                    throw new ArgumentNullException($"You must provide '{nameof(_searchOptions.AdminApiKey)}' or '{nameof(_searchOptions.QueryApiKey)}'");
                });

        }


        public bool Disabled { get; set; }
        public string IndexName { get; }

        public async Task CreateIndexIfNotExistsAsync(string indexName)
        {
            await CreateIndexIfNotExistsAsync(null, indexName).ConfigureAwait(false);
        }

        private async Task CreateIndexIfNotExistsAsync(ISearchServiceClient serviceClient, string indexName=null)
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            //Ensure we have a service client and index name
            serviceClient = serviceClient ?? await _serviceClient.Value.ConfigureAwait(false) ?? throw new ArgumentNullException(nameof(serviceClient));
            indexName = !string.IsNullOrWhiteSpace(indexName) ? indexName : !string.IsNullOrWhiteSpace(IndexName) ? IndexName : throw new ArgumentNullException(nameof(indexName));

            if (await serviceClient.Indexes.ExistsAsync(indexName).ConfigureAwait(false)) return;

            #region Create the field definitions
            var fields = new List<Field>(FieldBuilder.BuildForType<OrganisationSearchModel>());
            void Add(Field field)
            {
                var index = fields.FindIndex(f => f.Name == field.Name);
                if (index < 0)
                    fields.Add(field);
                else
                    fields[index] = field;
            }

            //Index fields
            Add(Field.New(nameof(OrganisationSearchModel.SearchDocumentKey),DataType.String,isKey:true));
            Add(Field.New(nameof(OrganisationSearchModel.Timestamp),DataType.String, isRetrievable:false));

            //Searchable fields
            Add(Field.NewSearchableString(nameof(OrganisationSearchModel.PartialNameForSuffixSearches), AnalyzerName.EnMicrosoft, isRetrievable:false));
            Add(Field.NewSearchableString(nameof(OrganisationSearchModel.PartialNameForCompleteTokenSearches), AnalyzerName.EnMicrosoft, isRetrievable:false));
            Add(Field.NewSearchableCollection(nameof(OrganisationSearchModel.Abbreviations), AnalyzerName.EnLucene, isRetrievable:false));
            Add(Field.NewSearchableString(nameof(OrganisationSearchModel.OrganisationName),AnalyzerName.EnLucene, isFilterable:true,isSortable:true));
            Add(Field.NewSearchableString(nameof(OrganisationSearchModel.CompanyNumber),AnalyzerName.EnMicrosoft));

            //Filterable fields
            Add(Field.New(nameof(OrganisationSearchModel.ParentOrganisationId), DataType.Int64, isFilterable:true));
            Add(Field.New(nameof(OrganisationSearchModel.ParentName), DataType.String, isFilterable:true));
            Add(Field.New(nameof(OrganisationSearchModel.StatementId), DataType.Int64, isFilterable:true));
            Add(Field.New(nameof(OrganisationSearchModel.SubmissionDeadlineYear), DataType.Int32, isFilterable: true, isSortable:true));
            Add(Field.New(nameof(OrganisationSearchModel.Modified), DataType.DateTimeOffset, isSortable:true));

            //Complex filterable fields
            var keyNameFilterFields = new List<Field>();
            keyNameFilterFields.Add(Field.New(nameof(OrganisationSearchModel.KeyName.Key), DataType.Int32, isFilterable: true));
            keyNameFilterFields.Add(Field.New(nameof(OrganisationSearchModel.KeyName.Name), DataType.String));
            Add(Field.NewComplex(nameof(OrganisationSearchModel.Turnover),false, keyNameFilterFields));
            Add(Field.NewComplex(nameof(OrganisationSearchModel.StatementYears), false, keyNameFilterFields));
            Add(Field.NewComplex(nameof(OrganisationSearchModel.Sectors), true, keyNameFilterFields));
            #endregion

            var index = new Index { Name = indexName, Fields = fields };

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
            if (!await serviceClient.SynonymMaps.ExistsAsync(synonymMapName).ConfigureAwait(false))
                serviceClient.SynonymMaps.CreateOrUpdate(
                    new SynonymMap
                    {
                        Name = synonymMapName,
                        //Format = "solr", cannot set after upgrade from v5.03 to version 9.0.0
                        Synonyms = "coop, co-operative"
                    });

            await serviceClient.Indexes.CreateAsync(index).ConfigureAwait(false);
        }

        public async Task DeleteIndexIfExistsAsync(string indexName = null)
        {
            await DeleteIndexIfExistsAsync(null, indexName).ConfigureAwait(false);
        }

        private async Task DeleteIndexIfExistsAsync(ISearchServiceClient serviceClient, string indexName=null)
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            //Ensure we have a service client and index name
            serviceClient = serviceClient ?? await _serviceClient.Value.ConfigureAwait(false) ?? throw new ArgumentNullException(nameof(serviceClient));
            indexName = !string.IsNullOrWhiteSpace(indexName) ? indexName : !string.IsNullOrWhiteSpace(IndexName) ? IndexName : throw new ArgumentNullException(nameof(indexName));

            if (!await serviceClient.Indexes.ExistsAsync(indexName).ConfigureAwait(false)) return;

            await serviceClient.Indexes.DeleteAsync(indexName).ConfigureAwait(false);

            //Recreate the service client
            SetServiceClient();

            //Recreate the index client
            SetIndexClient();

            //Recreate the index
            await CreateIndexIfNotExistsAsync(IndexName).ConfigureAwait(false);
        }

        public async Task AddOrUpdateDocumentsAsync(IEnumerable<OrganisationSearchModel> newRecords)
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            if (newRecords == null || !newRecords.Any())
                throw new ArgumentNullException(nameof(newRecords), "You must supply at least one record to index");

            //Ensure the records are ordered by name
            newRecords = newRecords.OrderBy(o => o.OrganisationName);

            //Set the records to add or update
            var actions = newRecords.Select(r => IndexAction.Upload(r))
                .ToList();

            var batches = new ConcurrentBag<IndexBatch<OrganisationSearchModel>>();
            while (actions.Any())
            {
                var batchSize = actions.Count > _searchOptions.BatchSize ? _searchOptions.BatchSize : actions.Count;
                var batch = IndexBatch.New(actions.Take(batchSize).ToList());
                batches.Add(batch);
                actions.RemoveRange(0, batchSize);
            }

            var indexClient = await _indexClient.Value.ConfigureAwait(false);

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
            var actions = oldRecords.Select(r => IndexAction.Delete(r)).ToList();

            var batches = new ConcurrentBag<IndexBatch<OrganisationSearchModel>>();

            while (actions.Any())
            {
                var batchSize = actions.Count > 1000 ? 1000 : actions.Count;
                var batch = IndexBatch.New(actions.Take(batchSize).ToList());
                batches.Add(batch);
                actions.RemoveRange(0, batchSize);
            }

            var deleteCount = 0;

            var exceptions = new ConcurrentBag<Exception>();
            var indexClient = await _indexClient.Value.ConfigureAwait(false);

            await batches.WaitForAllAsync(
                async batch =>
                {
                    var retries = 0;
                    retry:
                    try
                    {
                        await indexClient.Documents.IndexAsync(batch).ConfigureAwait(false);
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
                }).ConfigureAwait(false);
            if (!exceptions.IsEmpty) throw new AggregateException(exceptions);

            return deleteCount;
        }

        public async Task<OrganisationSearchModel> GetDocumentAsync(string key, string selectFields = null)
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            //Limit result fields
            var selectedFields = string.IsNullOrWhiteSpace(selectFields) ? null : selectFields.SplitI().ToList();

            var indexClient = await _indexClient.Value.ConfigureAwait(false);

            try
            {
                var result = await indexClient.Documents.GetAsync<OrganisationSearchModel>(key, selectedFields).ConfigureAwait(false);

                return result;
            }
            catch (CloudException cex)when (cex?.Response?.StatusCode==System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<IList<OrganisationSearchModel>> ListDocumentsAsync(string selectFields = null, string filter=null)
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            long totalPages = 0;
            var currentPage = 1;
            var resultsList = new List<OrganisationSearchModel>();
            var orderBy = $"{nameof(OrganisationSearchModel.OrganisationName)}, {nameof(OrganisationSearchModel.SubmissionDeadlineYear)} desc";
            do
            {
                var searchResults = await SearchDocumentsAsync(null,currentPage,selectFields: selectFields, filter:filter,orderBy: orderBy).ConfigureAwait(false);
                totalPages = searchResults.ActualPageCount;
                resultsList.AddRange(searchResults.Results);
                currentPage++;
            } while (currentPage <= totalPages);

            return resultsList;
        }

        public async Task<long> GetDocumentCountAsync()
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            var serviceClient = await _serviceClient.Value.ConfigureAwait(false);

            if (!await serviceClient.Indexes.ExistsAsync(IndexName).ConfigureAwait(false)) return 0;

            var searchResults = await SearchDocumentsAsync(null, 1).ConfigureAwait(false);
            return searchResults.ActualRecordTotal;
        }

        public async Task<PagedSearchResult<OrganisationSearchModel>> SearchDocumentsAsync(string searchText,
            int currentPage,
            int pageSize = 20,
            string searchFields = null,
            string selectFields = null,
            string facetFields = null,
            string orderBy = null,
            string filter = null,
            string highlights = null,
            SearchModes searchMode = SearchModes.Any)
        {
            if (Disabled) throw new Exception($"{nameof(AzureOrganisationSearchRepository)} is disabled");

            var indexClient = await _indexClient.Value.ConfigureAwait(false);

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
            var facets = facetFields?.SplitI().ToList();
            if (facets != null && facets.Count > 0) sp.Facets = facets;

            //Execute the search
            var headers = new Dictionary<string, List<string>>() { { "x-ms-azs-return-searchid", new List<string>() { "true" } } };
            var results = await indexClient.Documents.SearchWithHttpMessagesAsync<OrganisationSearchModel>(searchText, sp,customHeaders:headers).ConfigureAwait(false);

            //Return the total records
            var totalRecords = results.Body.Count.Value;

            /* There are too many empty searches being executed (about 1200). This needs further investigation to see if/how they can be reduced */
            if (!string.IsNullOrEmpty(searchText) && currentPage==1)
            {
                //Get the search id for logging purposes
                var searchId = results.Response.Headers.TryGetValues("x-ms-azs-searchid", out IEnumerable<string> headerValues) ? headerValues.FirstOrDefault() : string.Empty;

                var searchLogModel = new SearchLogModel
                {
                    TimeStamp=VirtualDateTime.Now,
                    SearchServiceName=_searchOptions.ServiceName,
                    IndexName=IndexName,
                    SearchId=searchId,
                    QueryTerms=searchText,
                    Filter=sp.Filter,
                    OrderBy=sp.OrderBy.ToDelimitedString(),
                    ResultCount=totalRecords
                };

                await SearchLog.WriteAsync(searchLogModel).ConfigureAwait(false);
            }

            
            //Create the results
            var searchResults = new PagedSearchResult<OrganisationSearchModel>
            {
                Results = results.Body.Results.Select(r => r.Document).ToList(),
                CurrentPage = currentPage,
                PageSize = pageSize,
                ActualRecordTotal = totalRecords,
                VirtualRecordTotal = totalRecords
            };

            //Add the facet results
            if (results.Body.Facets != null && results.Body.Facets.Count>0)
            {
                searchResults.facets = new Dictionary<string, Dictionary<object, long>>();
                foreach (var facetGroupKey in results.Body.Facets.Keys)
                {
                    searchResults.facets[facetGroupKey] = new Dictionary<object, long>();

                    foreach (var facetResult in results.Body.Facets[facetGroupKey])
                        searchResults.facets[facetGroupKey][facetResult.Value] = facetResult.Count.Value;
                }
            }

            //Return the results
            return searchResults;
        }
    }
}