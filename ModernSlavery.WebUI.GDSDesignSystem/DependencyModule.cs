using System;
using Autofac;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.WebUI.Account
{
    public class DependencyModule : IDependencyModule
    {
        public bool AutoSetup { get; } = false;

        public void Register(IDependencyBuilder builder)
        {
            //TODO: Register dependencies here
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Configure dependencies here
        }
    }
}