using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Core.Options
{
    [Options("SearchService")]
    public class SearchOptions : IOptions
    {
        public string ServiceName { get; set; }
        public string AdminApiKey { get; set; }
        public string QueryApiKey { get; set; }
        public string OrganisationIndexName { get; set; } = nameof(OrganisationSearchModel);
        public bool Disabled { get; set; }
        public bool CacheResults { get; set; }
    }
}