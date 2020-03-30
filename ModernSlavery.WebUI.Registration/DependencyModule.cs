using System;
using Autofac;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.WebUI.Registration.Classes;

namespace ModernSlavery.WebUI.Registration
{
    [AutoRegister]
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

        public void Register(IDependencyBuilder builder)
        {
            //Register public and private repositories
            builder.Autofac.RegisterType<PublicSectorRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Public")
                .InstancePerLifetimeScope();

            builder.Autofac.RegisterType<PrivateSectorRepository>()
                .As<IPagedRepository<EmployerRecord>>()
                .Keyed<IPagedRepository<EmployerRecord>>("Private")
                .InstancePerLifetimeScope();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //Configure dependencies here
        }
    }
}