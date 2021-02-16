using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Telemetry.AppInsights
{
    public class AppInsightsTelemetryInitializer : ITelemetryInitializer
    {
        private readonly ApplicationInsightsOptions _applicationInsightsOptions;
        /// <summary>
        /// The HTTP method and path prefix (e.g., GET /Common/App_Data/)
        /// </summary>
        private readonly string _operationName;
        public AppInsightsTelemetryInitializer(ApplicationInsightsOptions applicationInsightsOptions, StorageOptions storageOptions)
        {
            _applicationInsightsOptions = applicationInsightsOptions;
            _operationName = $"HEAD /{storageOptions.AzureShareName}/";
        }

        /// <summary>
        /// The not found result codes to regard as successful
        /// </summary>
        private const string ResultCode = "404";

        /// <summary>
        /// The suffix of the host name of the dependency(e.g., file.core.windows.net)
        /// </summary>
        private const string _hostName = "file.core.windows.net";

        public void Initialize(ITelemetry telemetry)
        {
            // set custom role name here
            telemetry.Context.Cloud.RoleName = _applicationInsightsOptions.RoleName;

            // If we set the Success property, the SDK won't change it:
            if (telemetry is DependencyTelemetry dependencyTelemetry && IsFile404(dependencyTelemetry))
                dependencyTelemetry.Success = true;
        }

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
