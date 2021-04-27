using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;
using ModernSlavery.Infrastructure.Telemetry.AppInsights.Suppressions;
using static ModernSlavery.Infrastructure.Telemetry.AppInsights.Suppressions.TelemetrySuppressionRecord;

namespace ModernSlavery.Infrastructure.Telemetry.AppInsights
{
    /// <summary>
    /// Removes Http 404 (NotFound) errors received from file storage from telemetry to Application Insights
    /// </summary>
    public class AppInsightsTelemetryProcessor : ITelemetryProcessor
    {
        private readonly ApplicationInsightsOptions _applicationInsightsOptions;
        private readonly TelemetrySuppressionOptions _telemetrySuppressionOptions;
        private readonly ITelemetryProcessor _next;

        // next will point to the next TelemetryProcessor in the chain.
        public AppInsightsTelemetryProcessor(
            ITelemetryProcessor next, 
            StorageOptions storageOptions, 
            ApplicationInsightsOptions applicationInsightsOptions, 
            TelemetrySuppressionOptions telemetrySuppressionOptions)
        {
            _next = next;
            _applicationInsightsOptions = applicationInsightsOptions;
            _telemetrySuppressionOptions = telemetrySuppressionOptions;

            //Remove all the suppressions not belonging to this application role
            var removals=_telemetrySuppressionOptions.RemoveAll(r => !string.IsNullOrWhiteSpace(r.ApplicationRole) && !r.ApplicationRole.EqualsI(_applicationInsightsOptions.RoleName));
        }

        public void Process(ITelemetry telemetry)
        {
            //Send the telemetry
            telemetry.Context.Cloud.RoleName = _applicationInsightsOptions.RoleName;

            var suppressionAction = _telemetrySuppressionOptions.ExecuteSuppressions(telemetry);

            if (suppressionAction == FilterActionTypes.Delete) return;
            _next.Process(telemetry);
        }
    }
}
