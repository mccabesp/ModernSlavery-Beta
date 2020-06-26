using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Search
{
    [Options("SearchService")]
    public class SearchOptions : IOptions
    {
        public string ServiceName { get; set; }
        public string AdminApiKey { get; set; }
        public string QueryApiKey { get; set; }
        public string EmployerIndexName { get; set; } = nameof(EmployerSearchModel);
        public string SicCodeIndexName { get; set; } = nameof(SicCodeSearchModel);
        public bool Disabled { get; set; }
        public bool CacheResults { get; set; }
    }
}