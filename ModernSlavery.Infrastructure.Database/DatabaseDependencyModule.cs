using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Infrastructure.Database.Classes;

namespace ModernSlavery.Infrastructure.Database
{
    public class DatabaseDependencyModule : IDependencyModule
    {
        public void Bind(ContainerBuilder builder, IServiceCollection services)
        {
            builder.RegisterType<DatabaseContext>().As<IDbContext>().InstancePerLifetimeScope();

            builder.RegisterType<SqlRepository>().As<IDataRepository>().InstancePerLifetimeScope();
        }
    }
}
