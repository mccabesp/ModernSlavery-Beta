using System;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.SharedKernel.Interfaces;

namespace ModernSlavery.SharedKernel.Extensions
{
    public static partial class Extensions
    {
        public static void RegisterDependencyModule<TModule>(this ContainerBuilder builder) where TModule : class, IDependencyModule
        {
            RegisterDependencyModule(builder, typeof(TModule));
        }

        public static void RegisterDependencyModule(this ContainerBuilder builder, Type serviceType) 
        {
            builder.Register((context, parameters) =>
            {
                var scope = context.Resolve<ILifetimeScope>();
                var services = context.Resolve<IServiceCollection>();

                using (var innerScope = scope.BeginLifetimeScope(b => b.RegisterType(serviceType).ExternallyOwned()))
                {
                    var module = (IDependencyModule)innerScope.Resolve(serviceType,parameters);
                    module.Bind(builder, services);
                    return module;
                }
            });
        }
    }
}
