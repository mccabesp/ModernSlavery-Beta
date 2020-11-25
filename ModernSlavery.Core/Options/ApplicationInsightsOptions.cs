using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Options
{
    [Options("ApplicationInsights")]
    public class ApplicationInsightsOptions : IOptions
    {
        public string InstrumentationKey { get; set; }

        public string RoleName { get; set; }

        
    }
}