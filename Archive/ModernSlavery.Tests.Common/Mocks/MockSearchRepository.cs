﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.Tests.Common.Mocks
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

    public class MockSearchRepository : ISearchRepository<EmployerSearchModel>
    {
        private List<EmployerSearchModel> _documents = new List<EmployerSearchModel>();

        public MockSearchRepository(List<EmployerSearchModel> documents = null)
        {
            if (documents != null) _documents = documents;
        }

        public bool Disabled { get; set; }
        public string IndexName { get; set; }


        public async Task CreateIndexIfNotExistsAsync(string indexName)
        {
        }

        public async Task<EmployerSearchModel> GetAsync(string key, string selectFields = null)
        {
            return _documents.FirstOrDefault(d => d.OrganisationId == key);
        }

        public async Task<long> GetDocumentCountAsync()
        {
            return _documents.Count;
        }

        public async Task AddOrUpdateIndexDataAsync(IEnumerable<EmployerSearchModel> records)
        {
            _documents.AddOrUpdate(records.ToList());
        }

        public async Task<int> RemoveFromIndexAsync(IEnumerable<EmployerSearchModel> records)
        {
            var c = 0;
            foreach (var record in records)
            {
                var i = _documents.FindIndex(d => d.OrganisationId == record.OrganisationId);
                if (i > -1)
                {
                    _documents.RemoveAt(i);
                    c++;
                }
            }

            return c;
        }

        public async Task<PagedResult<EmployerSearchModel>> SearchAsync(string searchText,
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
            var result = new PagedResult<EmployerSearchModel>();
            result.Results = new List<EmployerSearchModel>(_documents);
            //result.ActualRecordTotal = _documents.Count;
            //result.VirtualRecordTotal = _documents.Count;

            var totalRecords = _documents.Count;

            //Return the results
            var searchResults = new PagedResult<EmployerSearchModel>
            {
                Results = result.Results,
                CurrentPage = currentPage,
                PageSize = pageSize,
                ActualRecordTotal = totalRecords,
                VirtualRecordTotal = totalRecords
            };

            return searchResults;
        }

        public Task<IEnumerable<KeyValuePair<string, EmployerSearchModel>>> SuggestAsync(string searchText,
            string searchFields = null,
            string selectFields = null,
            bool fuzzy = true,
            int maxRecords = 10)
        {
            return Task.FromResult<IEnumerable<KeyValuePair<string, EmployerSearchModel>>>(null);
        }

        public async Task RefreshIndexDataAsync(IEnumerable<EmployerSearchModel> listOfRecordsToAddOrUpdate)
        {
            _documents = listOfRecordsToAddOrUpdate.ToList();
        }

        public async Task<IList<EmployerSearchModel>> ListAsync(string selectFields = null)
        {
            return _documents;
        }
    }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}