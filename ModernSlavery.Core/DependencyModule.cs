using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using System;
using System.Collections.Generic;

namespace ModernSlavery.Core
{
    public class DependencyModule : IDependencyModule
    {
        private readonly ILogger _logger;
        private readonly SharedOptions _sharedOptions;

        public DependencyModule(
            ILogger<DependencyModule> logger, 
            SharedOptions sharedOptions)
        {
            _logger = logger;
            _sharedOptions = sharedOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Register service dependencies here
            services.AddSingleton<DependencyContractResolver>();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<ReportingDeadlineHelper>().As<IReportingDeadlineHelper>().SingleInstance();

            builder.RegisterType<GovUkCountryProvider>().As<IGovUkCountryProvider>().SingleInstance();

            //Register some singletons
            builder.RegisterType<InternalObfuscator>().As<IObfuscator>().SingleInstance()
                .WithParameter("seed", _sharedOptions.ObfuscationSeed);

            builder.RegisterType<SourceComparer>().As<ISourceComparer>().SingleInstance();

            builder.RegisterType<UrlChecker>().As<IUrlChecker>().InstancePerLifetimeScope();
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