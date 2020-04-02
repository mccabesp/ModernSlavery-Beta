using Autofac;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.BusinessDomain.Shared
{
    public class DependencyModule : IDependencyModule
    {
        public DependencyModule()
        {
        }

        public void Register(IDependencyBuilder builder)
        {
            //Add registrations here
            builder.Autofac.RegisterType<SharedBusinessLogic>().As<ISharedBusinessLogic>().SingleInstance();
            builder.Autofac.RegisterType<UpdateFromCompaniesHouseService>()
                .As<UpdateFromCompaniesHouseService>().InstancePerLifetimeScope();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Add configuration here
        }
    }
}