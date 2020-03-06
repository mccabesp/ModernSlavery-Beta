using System;
using System.Collections.Generic;
using System.Text;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Extensions.AspNetCore;

namespace ModernSlavery.Infrastructure.Search
{
    public class AzureSearchService
    {
        public AzureSearchService(ISearchRepository<EmployerSearchModel> searchRepository, ISearchRepository<SicCodeSearchModel> sicCodeSearchRepository)
        {
            SearchRepository = searchRepository;
            SicCodeSearchRepository = sicCodeSearchRepository;
        }

        public ISearchRepository<EmployerSearchModel> SearchRepository;
        public ISearchRepository<SicCodeSearchModel> SicCodeSearchRepository;

        public static string SearchIndexName => Config.GetAppSetting("SearchService:IndexName", nameof(EmployerSearchModel).ToLower());

        public static string SicCodesIndexName => Config.GetAppSetting("SearchService:SicCodesIndexName", nameof(SicCodeSearchModel).ToLower());

    }
}
