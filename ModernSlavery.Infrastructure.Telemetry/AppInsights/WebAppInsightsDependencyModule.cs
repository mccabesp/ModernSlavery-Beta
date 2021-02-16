using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Telemetry.AppInsights
{
    public class WebAppInsightsDependencyModule : IDependencyModule
    {
        private readonly ApplicationInsightsOptions _applicationInsightsOptions;

        public WebAppInsightsDependencyModule(ApplicationInsightsOptions applicationInsightsOptions)
        {
            _applicationInsightsOptions = applicationInsightsOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            if (!string.IsNullOrWhiteSpace(_applicationInsightsOptions.InstrumentationKey))
            {
                //Add app insights initialiser to set role name and set file 404 error codes to successful
                //Should be superceded by AppInsightsTelemetryProcessor but doesnt work for webjobs so using this here
                services.AddSingleton<ITelemetryInitializer, AppInsightsTelemetryInitializer>();

                var aiOptions = new ApplicationInsightsServiceOptions {
                    //Disable adaptive sampling to use only ingestion sampling (see https://docs.microsoft.com/en-us/azure/azure-monitor/app/sampling)

                    EnableAdaptiveSampling = false,

                    //Add app insights tracking using ApplicationInsights:InstrumentationKey (from AppSettings.json or Azure KeyVault)
                    InstrumentationKey = _applicationInsightsOptions.InstrumentationKey
                };

                services.AddApplicationInsightsTelemetry(aiOptions);

                //Set telemetry role and remove telemetry sent to Application Insights
                services.AddApplicationInsightsTelemetryProcessor<AppInsightsTelemetryProcessor>();
            }
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {

        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Configure dependencies here
        }

        public void RegisterModules(IList<Type> modules)
        {
            //TODO: Add any linked dependency modules here
        }
    }
}