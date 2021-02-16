using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Telemetry.AppInsights
{
    /// <summary>
    /// Removes Http 404 (NotFound) errors received from file storage from telemetry to Application Insights
    /// </summary>
    public class AppInsightsTelemetryProcessor : ITelemetryProcessor
    {
        private ApplicationInsightsOptions _applicationInsightsOptions;
        private readonly ITelemetryProcessor _next;
        private readonly string _operationName;

        // next will point to the next TelemetryProcessor in the chain.
        public AppInsightsTelemetryProcessor(ITelemetryProcessor next, StorageOptions storageOptions, ApplicationInsightsOptions applicationInsightsOptions)
        {
            _next = next;
            _operationName = $"HEAD /{storageOptions.AzureShareName}/";
            _applicationInsightsOptions = applicationInsightsOptions;
        }


        /// <summary>
        /// The not found result codes to regard as successful
        /// </summary>
        private const string ResultCode = "404";

        /// <summary>
        /// The suffix of the host name of the dependency(e.g., file.core.windows.net)
        /// </summary>
        private const string _hostName = "file.core.windows.net";

        public void Process(ITelemetry telemetry)
        {
            var dependency = telemetry as DependencyTelemetry;
            if (dependency != null)
            {
                //Dont log storage 404 errors
                if (IsFile404(dependency)) return;

                //Ignore successful dependencies 
                if (_applicationInsightsOptions.EnableAdaptiveSampling && dependency.Success==true) return;
            }

            //Send the telemetry
            telemetry.Context.Cloud.RoleName = _applicationInsightsOptions.RoleName;
            _next.Process(telemetry);
        }

        // Check dependency is for file storage 
        private bool IsFile404(DependencyTelemetry dependencyTelemetry)
        {
            if (ResultCode == dependencyTelemetry.ResultCode
                && dependencyTelemetry.Target.EndsWithI(_hostName)
                && dependencyTelemetry.Name.StartsWithI(_operationName))
                return true;

            return false;
        }
    }
}
