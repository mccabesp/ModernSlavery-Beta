using System;
using Autofac;
using ModernSlavery.BusinessLogic;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.Messaging
{
    public class MessagingDependencyModule: IDependencyModule
    {
        public void Bind(ContainerBuilder builder) 
        {
            builder.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>().SingleInstance();

        }
    }
}
