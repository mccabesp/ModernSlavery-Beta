using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace ModernSlavery.SharedKernel.Options
{
    public class ResponseCachingOptions
    {
        public int StaticCacheSeconds { get; set; } = 86400;
    }
}
