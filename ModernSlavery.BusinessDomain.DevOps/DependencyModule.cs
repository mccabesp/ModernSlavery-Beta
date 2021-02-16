using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessDomain.DevOps.Testing;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Azure;
using ModernSlavery.Infrastructure.Azure.AppInsights;
using ModernSlavery.Infrastructure.Azure.Cache;
using ModernSlavery.Infrastructure.Storage;

namespace ModernSlavery.BusinessDomain.DevOps
{
    public class DependencyModule : IDependencyModule
    {
        public DependencyModule()
        {
            //TODO: Add IOptions parameters
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: Register service dependencies here
            services.AddSingleton<AzureManager>();
            services.AddSingleton<AppInsightsManager>();
            services.AddSingleton<IAzureStorageManager,AzureStorageManager>();
            services.AddSingleton<DistributedCacheManager>();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //TODO: Configure autofac dependencies here
            builder.RegisterType<TestBusinessLogic>().As<ITestBusinessLogic>().InstancePerLifetimeScope().WithAttributeFiltering();
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