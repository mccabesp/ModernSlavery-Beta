using System;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.BusinessLogic;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.Messaging
{
    public class MessagingDependencyModule: IDependencyModule
    {
        public void Bind(ContainerBuilder builder, IServiceCollection services) 
        {
            builder.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().SingleInstance();

        }
    }
}
