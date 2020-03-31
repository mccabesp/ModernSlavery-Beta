using System;
using Autofac;
using Autofac.Features.AttributeFilters;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.BusinessDomain.Submission
{
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
            builder.Autofac.RegisterType<SubmissionService>().As<ISubmissionService>()
                .InstancePerLifetimeScope().WithAttributeFiltering();
            builder.Autofac.RegisterType<SubmissionBusinessLogic>().As<ISubmissionBusinessLogic>()
                .InstancePerLifetimeScope().WithAttributeFiltering();
            builder.Autofac.RegisterType<DraftFileBusinessLogic>().As<IDraftFileBusinessLogic>()
                .SingleInstance();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Add configuration here
        }
    }
}