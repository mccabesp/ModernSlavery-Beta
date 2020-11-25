using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Telemetry.AppInsights
{
    public class AppInsightsRoleNameTelemetryInitializer : ITelemetryInitializer
    {
        private readonly ApplicationInsightsOptions _applicationInsightsOptions;
        public AppInsightsRoleNameTelemetryInitializer(ApplicationInsightsOptions applicationInsightsOptions)
        {
            _applicationInsightsOptions = applicationInsightsOptions;
        }

        public void Initialize(ITelemetry telemetry)
        {
            // set custom role name here
            telemetry.Context.Cloud.RoleName = _applicationInsightsOptions.RoleName;
        }
    }
}
