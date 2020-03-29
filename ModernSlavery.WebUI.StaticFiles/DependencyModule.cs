using System;
using Autofac;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.WebUI.StaticFiles
{
    [AutoRegister]
    public class DependencyModule : IDependencyModule
    {
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