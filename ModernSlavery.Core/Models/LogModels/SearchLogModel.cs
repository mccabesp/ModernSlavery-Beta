using System;

namespace ModernSlavery.Core.Models.LogModels
{
    [Serializable]
    public class SearchLogModel
    {
        public DateTime TimeStamp { get; set; }
        public string SearchServiceName { get; set; }
        public string IndexName { get; set; }
        public string SearchId { get; set; }
        public string QueryTerms { get; set; }
        public string Filter { get; set; }
        public string OrderBy { get; set; }
        public long ResultCount { get; set; }
    }
}