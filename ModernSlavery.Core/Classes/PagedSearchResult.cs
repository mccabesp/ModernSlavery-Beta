using System;
using System.Collections.Generic;

namespace ModernSlavery.Core.Classes
{
    [Serializable]
    public class PagedSearchResult<T>: PagedResult<T>
    {
        public Dictionary<string, Dictionary<object, long>> facets { get; set; }
    }
}