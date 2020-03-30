using System;
using Autofac;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.WebUI.Submission.Classes;

namespace ModernSlavery.WebUI.Submission
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
            //Register dependencies here
            builder.Autofac.RegisterType<SubmissionPresenter>().As<ISubmissionPresenter>()
                .InstancePerLifetimeScope();
            builder.Autofac.RegisterType<ScopePresenter>().As<IScopePresenter>().InstancePerLifetimeScope();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //Configure dependencies here
        }
    }
}