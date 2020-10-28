using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Shared.Classes;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Shared.Models;

namespace ModernSlavery.WebUI.Shared
{
    public class DependencyModule: IDependencyModule
    {
        private readonly ILogger _logger;
        private readonly SharedOptions _sharedOptions;
        public DependencyModule(
            ILogger<DependencyModule> logger,
            SharedOptions sharedOptions
        )
        {
            _logger = logger;
            _sharedOptions = sharedOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Add the custom url helper
            services.AddSingleton<IUrlHelperFactory,CustomUrlHelperFactory>();

            //Register service dependencies here
            services.AddSingleton<SectorTypeIndex>();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //Register HttpCache and HttpSession
            builder.RegisterType<HttpSession>().As<IHttpSession>().InstancePerLifetimeScope();
            builder.RegisterType<HttpCache>().As<IHttpCache>().SingleInstance();

            //Register the web service container
            builder.RegisterType<WebService>().As<IWebService>().InstancePerLifetimeScope();

            //Register factories
            builder.RegisterType<ErrorViewModelFactory>().As<IErrorViewModelFactory>()
                .SingleInstance();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //Configure dependencies here
            PasswordAttribute.SharedOptions = _sharedOptions;
            PinAttribute.SharedOptions = _sharedOptions;
            SpamProtectionAttribute.SharedOptions = _sharedOptions;
        }

        public void RegisterModules(IList<Type> modules)
        {
            //Register references dependency modules
            modules.AddDependency<ModernSlavery.BusinessDomain.Shared.DependencyModule>();
            modules.AddDependency<ModernSlavery.WebUI.GDSDesignSystem.DependencyModule>();

        }
    }
}
