using System;
using Autofac;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Infrastructure.Database.Classes;

namespace ModernSlavery.Infrastructure.Database
{
    [AutoRegister]
    public class DatabaseDependencyModule : IDependencyModule
    {
        public void Register(IDependencyBuilder builder)
        {
            builder.Autofac.RegisterType<DatabaseContext>().As<IDbContext>().InstancePerLifetimeScope();

            builder.Autofac.RegisterType<SqlRepository>().As<IDataRepository>().InstancePerLifetimeScope();
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Add configuration here
        }
    }
}