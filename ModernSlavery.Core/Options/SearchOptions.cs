using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using System;
using System.Configuration;

namespace ModernSlavery.Core.Options
{
    [Options("SearchService")]
    public class SearchOptions : IOptions
    {
        public const int MaxBatchSize = 1000;

        public string ServiceName { get; set; }
        public string AdminApiKey { get; set; }
        public string QueryApiKey { get; set; }
        public string OrganisationIndexName { get; set; } = nameof(OrganisationSearchModel);
        public bool Disabled { get; set; }
        public bool CacheResults { get; set; }

        public int BatchSize { get; set; } = MaxBatchSize;

        public void Validate() 
        {
            if (BatchSize < 1 || MaxBatchSize > 1000) throw new ConfigurationException("SearchService:BatchSize must be between 1 and 1000");
        }

    }
}