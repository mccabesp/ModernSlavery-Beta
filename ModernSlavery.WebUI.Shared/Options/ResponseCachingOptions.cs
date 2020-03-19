using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Options
{
    [Options("ResponseCaching")]
    public class ResponseCachingOptions : IOptions
    {
        public int StaticCacheSeconds { get; set; } = 86400;
        public Dictionary<string, CacheProfile> CacheProfiles { get; set; } = new Dictionary<string, CacheProfile>(StringComparer.OrdinalIgnoreCase);
    }
}
