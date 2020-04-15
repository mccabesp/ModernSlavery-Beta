using System;
using System.Collections.Generic;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.WebUI.Shared.Options
{
    [Options("UrlRoutes", true)]
    public class UrlRoutesOptions : Dictionary<string, string>, IOptions
    {
        public UrlRoutesOptions() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}