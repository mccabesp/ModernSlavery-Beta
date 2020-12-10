using System;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Infrastructure.Telemetry
{
    /// <summary>
    /// Removes Http 404 (NotFound) errors received from file storage from telemetry to Application Insights
    /// </summary>
    public class FileNotFoundTelemetryFilter : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        // next will point to the next TelemetryProcessor in the chain.
        public FileNotFoundTelemetryFilter(ITelemetryProcessor next)
        {
            this.Next = next;
        }

        /// <summary>
        /// The suffix of the host name of the dependency(e.g., file.core.windows.net)
        /// </summary>
        private const string HostName = "file.core.windows.net";

        /// <summary>
        /// The HTTP method and path prefix (e.g., GET /Common/App_Data/)
        /// </summary>
        private const string OperationName = "HEAD /common/";

        /// <summary>
        /// The list of result codes to regard as successful
        /// </summary>
        private readonly string[] ResultCodes = new string[] { "404" };

        public void Process(ITelemetry item)
        {
            // To filter out an item, return without calling the next processor.
            if (!OKtoSend(item)) { return; }

            this.Next.Process(item);
        }

        // Example: replace with your own criteria.
        private bool OKtoSend(ITelemetry item)
        {
            var dependency = item as DependencyTelemetry;

            if (dependency != null && !string.IsNullOrWhiteSpace(dependency?.Context?.Operation?.Name)
                && dependency.Target.EndsWithI(HostName)
                && dependency.Name.StartsWithI(OperationName)
                && ResultCodes.Any(rc => rc.EqualsI(dependency.ResultCode)))
            {
                dependency.Success = true;
                return false;
            }

            return true;
        }
    }
}