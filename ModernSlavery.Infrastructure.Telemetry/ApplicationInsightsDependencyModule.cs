using System;
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
            _applicationInsightsOptions = _applicationInsightsOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Add app insights tracking
            services.AddApplicationInsightsTelemetry(_applicationInsightsOptions.InstrumentationKey);
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