using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Search
{
    [Options("Search")]
    public class SearchOptions : IOptions
    {
        public string ServiceName { get; set; }
        public string ApiAdminKey { get; set; }
        public string ApiQueryKey { get; set; }
        public string EmployerIndexName { get; set; } = nameof(EmployerSearchModel);
        public string SicCodeIndexName { get; set; } = nameof(SicCodeSearchModel);
        public bool Disabled { get; set; }
        public bool CacheResults { get; set; }
    }
}