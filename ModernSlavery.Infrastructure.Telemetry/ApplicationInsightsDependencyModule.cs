﻿using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.Telemetry
{
    public class ApplicationInsightsDependencyModule : IDependencyModule
    {
        private readonly ApplicationInsightsOptions _applicationInsightsOptions;

        public ApplicationInsightsDependencyModule(ApplicationInsightsOptions applicationInsightsOptions)
        {
            _applicationInsightsOptions = applicationInsightsOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Add app insights tracking using ApplicationInsights:InstrumentationKey (from AppSettings.json or Azure KeyVault)
            services.AddApplicationInsightsTelemetry();

            //Add filter to removes Http 404 (NotFound) errors received from file storage from telemetry sent to Application Insights
            services.AddApplicationInsightsTelemetryProcessor<FileNotFoundTelemetryFilter>();
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