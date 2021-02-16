using System;
using System.Collections.Generic;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.WebUI.StaticFiles
{
    [Options("StaticHeaders")]
    public class StaticHeaderOptions : Dictionary<string, string>, IOptions
    {
        public StaticHeaderOptions() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}