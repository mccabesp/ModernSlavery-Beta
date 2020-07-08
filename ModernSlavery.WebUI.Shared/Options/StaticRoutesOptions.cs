using System;
using System.Collections.Generic;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.WebUI.Shared.Options
{
    [Options("StaticRoutes", true)]
    public class StaticRoutesOptions : Dictionary<string, string>, IOptions
    {
        public StaticRoutesOptions() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}