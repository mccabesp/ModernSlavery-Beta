using System;
using Autofac;
using ModernSlavery.SharedKernel.Interfaces;

namespace ModernSlavery.SharedKernel.Extensions
{
    public static partial class Extensions
    {
        public static void BindResolvedDependencyModule<TModule>(this ContainerBuilder builder) where TModule : class, IDependencyModule
        {
            BindResolvedDependencyModule(builder, typeof(TModule));
        }

        public static void BindResolvedDependencyModule(this ContainerBuilder builder, Type serviceType) 
        {
            builder.Register((context, parameters) =>
            {
                var scope = context.Resolve<ILifetimeScope>();

                using (var innerScope = scope.BeginLifetimeScope(b => b.RegisterType(serviceType).ExternallyOwned()))
                {
                    var module = (IDependencyModule)innerScope.Resolve(serviceType,parameters);
                    module.Bind(builder);
                    return default(IDependencyModule);
                }
            });
        }

    }
}
