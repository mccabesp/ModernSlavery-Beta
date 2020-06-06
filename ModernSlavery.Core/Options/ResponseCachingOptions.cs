using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ModernSlavery.Core.Attributes;

namespace ModernSlavery.Core.Options
{
    [Options("ResponseCaching")]
    public class ResponseCachingOptions : IOptions
    {
        public bool Enabled { get; set; } = true;

        public int StaticCacheSeconds { get; set; } = 86400;

        public Dictionary<string, CacheProfile> CacheProfiles { get; set; } =
            new Dictionary<string, CacheProfile>(StringComparer.OrdinalIgnoreCase);
    }
}