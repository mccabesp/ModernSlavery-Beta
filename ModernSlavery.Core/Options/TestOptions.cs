using ModernSlavery.Core.Attributes;

namespace ModernSlavery.Core.Options
{
    [Options("TestOptions")]
    public class TestOptions : IOptions
    {
        public bool ForceApplicationInsightsTracking { get; set; }
        public bool ForceGoogleAnalyticsTracking { get; set; }
    }
}