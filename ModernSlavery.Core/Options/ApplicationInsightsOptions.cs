using ModernSlavery.Core.Attributes;

namespace ModernSlavery.Core.Options
{
    [Options("ApplicationInsights")]
    public class ApplicationInsightsOptions : IOptions
    {
        public string InstrumentationKey { get; set; }
        public bool EnableAdaptiveSampling { get; set; } = true;

        public string RoleName { get; set; }

        
    }
}