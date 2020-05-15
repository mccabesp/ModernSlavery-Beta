using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.WebUI.Registration.Classes;
using ModernSlavery.WebUI.Registration.Presenters;

namespace ModernSlavery.WebUI.Registration
{
    public class DependencyModule : IDependencyModule
    {
        private readonly ILogger _logger;
        public DependencyModule(
            ILogger<DependencyModule> logger
            //TODO Add any required IOptions here
        )
        {
            _logger = logger;
            //TODO set any required local IOptions here
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: Register service dependencies here
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //Register public and private repositories
            builder.RegisterType<PublicSectorRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Public")
                .InstancePerLifetimeScope();

            builder.RegisterType<PrivateSectorRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Private")
                .InstancePerLifetimeScope();

            builder.Autofac.RegisterType<RegistrationPresenter>()
                .As<IRegistrationPresenter>()
                .InstancePerLifetimeScope();

            //Register all controllers - this is required to ensure KeyFilter is resolved in constructors
            builder.RegisterAssemblyTypes(typeof(DependencyModule).Assembly)
                .Where(t => t.IsAssignableTo<Controller>())
                .InstancePerLifetimeScope()
                .WithAttributeFiltering();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Configure dependencies here
        }

        public void RegisterModules(IList<Type> modules)
        {
            //Register references dependency modules
            modules.Add(typeof(ModernSlavery.BusinessDomain.Registration.DependencyModule));
            modules.Add(typeof(ModernSlavery.BusinessDomain.Shared.DependencyModule));
            modules.Add(typeof(ModernSlavery.WebUI.Shared.DependencyModule));

        }
    }
}