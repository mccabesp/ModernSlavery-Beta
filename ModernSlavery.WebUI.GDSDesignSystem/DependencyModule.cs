using System;
using Autofac;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.WebUI.GDSDesignSystem
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
            //TODO: Register dependencies here
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Configure dependencies here
        }
    }
}