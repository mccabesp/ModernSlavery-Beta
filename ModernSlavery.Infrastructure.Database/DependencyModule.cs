using System;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Infrastructure.Database.Classes;

namespace ModernSlavery.Infrastructure.Database
{
    public class DatabaseDependencyModule : IDependencyModule
    {
        public bool AutoSetup { get; } = false;

        public void Register(DependencyBuilder builder)
        {
            builder.ContainerBuilder.RegisterType<DatabaseContext>().As<IDbContext>().InstancePerLifetimeScope();

            builder.ContainerBuilder.RegisterType<SqlRepository>().As<IDataRepository>().InstancePerLifetimeScope();
        }

        public void Configure(IContainer container)
        {
            //TODO: Add configuration here
        }
    }
}
