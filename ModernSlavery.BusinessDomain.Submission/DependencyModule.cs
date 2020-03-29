using System;
using Autofac;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.BusinessDomain.Submission
{
    [AutoRegister]
    public class DependencyModule : IDependencyModule
    {
        public DependencyModule()
        {
            //TODO: Add IOptions parameters
        }

        public void Register(IDependencyBuilder builder)
        {
            //Add registrations here
            builder.Autofac.RegisterType<ScopeBusinessLogic>().As<IScopeBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<DraftFileBusinessLogic>().As<IDraftFileBusinessLogic>()
                .SingleInstance();
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Add configuration here
        }
    }
}