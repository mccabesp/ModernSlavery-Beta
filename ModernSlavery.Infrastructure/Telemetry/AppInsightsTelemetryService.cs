using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

namespace ModernSlavery.Infrastructure.Telemetry
{
    public class AppInsightsTelemetryService
    {
        private TelemetryClient _AppInsightsClient;

        public AppInsightsTelemetryService(string instrumentationKey)
        {
            //Set Application Insights instrumentation key
            //if (!Debugger.IsAttached)
            if (!string.IsNullOrWhiteSpace(instrumentationKey))
            {
                TelemetryConfiguration.Active.InstrumentationKey = instrumentationKey;

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