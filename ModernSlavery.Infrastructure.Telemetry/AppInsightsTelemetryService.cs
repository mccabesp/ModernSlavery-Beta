using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Telemetry
{
    public class AppInsightsTelemetryService
    {
        public AppInsightsTelemetryService(ApplicationInsightsOptions applicationInsightsOptions)
        {
            //Set Application Insights instrumentation key
            //if (!Debugger.IsAttached)
            if (!string.IsNullOrWhiteSpace(applicationInsightsOptions.InstrumentationKey))
            {
                TelemetryConfiguration.Active.InstrumentationKey = applicationInsightsOptions.InstrumentationKey;

                //Disable application insights tracing when debugger is attached
                //if (!Debugger.IsAttached
                TelemetryDebugWriter.IsTracingDisabled = false;

                var builder = TelemetryConfiguration.Active.TelemetryProcessorChainBuilder;
                builder.Use(
                    next => new DependencyTelemetryFilter(next, "file.core.windows.net", "HEAD /common/", "404"));
                builder.Build();
            }
        }
    }
}