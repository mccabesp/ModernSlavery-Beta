using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.Configuration
{
    public static class Extensions
    {
        public static IServiceProvider SetupDependencies<TModule>(this IServiceCollection services, IConfiguration configuration) where TModule: class, IDependencyModule
        {
            //Load all the IOptions in the domain
            var optionsBinder = new OptionsBinder(services, configuration);
            optionsBinder.BindAssemblies("ModernSlavery");

            //Register the dependencies
            using (var dependencyBuilder = new DependencyBuilder(services))
            {
                //Register all the automatic dependencies
                dependencyBuilder.RegisterDomainAssemblyModules("ModernSlavery");

                //Register the root dependency module and descendents
                dependencyBuilder.RegisterModule<TModule>();

                //Build all the registered dependencies
                var serviceProvider = dependencyBuilder.Build();

                //Configure the root module and descendents
                dependencyBuilder.ConfigureModule<TModule>(); ;

                //Return the service provider
                return serviceProvider;
            }
        }
        public static IServiceProvider SetupDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            //Load all the IOptions in the domain
            var optionsBinder = new OptionsBinder(services, configuration);
            optionsBinder.BindAssemblies("ModernSlavery");

            //Register the dependencies
            using (var dependencyBuilder = new DependencyBuilder(services))
            {
                //Register all the automatic dependencies
                dependencyBuilder.RegisterDomainAssemblyModules("ModernSlavery");

                //Build all the registered and configure all the autosetup dependencies 
                var serviceProvider = dependencyBuilder.Build();

                //Return the service provider
                return serviceProvider;
            }
        }
    }
}
