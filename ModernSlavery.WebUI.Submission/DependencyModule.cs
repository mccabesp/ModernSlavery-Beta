using System;
using Autofac;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.WebUI.Submission.Classes;

namespace ModernSlavery.WebUI.Submission
{
    [AutoRegister]
    public class DependencyModule : IDependencyModule
    {
        public void Register(IDependencyBuilder builder)
        {
            //Register dependencies here
            builder.Autofac.RegisterType<SubmissionPresenter>().As<ISubmissionPresenter>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<ScopePresenter>().As<IScopePresenter>().InstancePerLifetimeScope();
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //Configure dependencies here
        }
    }
}