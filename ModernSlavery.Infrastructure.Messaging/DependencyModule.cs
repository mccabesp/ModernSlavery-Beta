using System;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.Messaging
{
    public class DependencyModule: IDependencyModule
    {
        public void Register(IDependencyBuilder builder) 
        {
            builder.ContainerBuilder.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().SingleInstance();

        }
        public bool AutoSetup { get; } = false;

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Add configuration here
        }
    }
}
