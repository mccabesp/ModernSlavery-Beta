using System;
using Autofac;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.Infrastructure.Messaging
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

        public void Register(IDependencyBuilder builder)
        {
            builder.Autofac.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().SingleInstance();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Add configuration here
        }
    }
}