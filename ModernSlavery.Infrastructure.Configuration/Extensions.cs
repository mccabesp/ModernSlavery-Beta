using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.Configuration
{
    public static class Extensions
    {
        public static IServiceProvider ConfigureDependencies<TModule>(this IServiceCollection services, IConfiguration configuration) where TModule: class, IDependencyModule
        {
            //Load all the IOptions in the domain
            var optionsBinder = new OptionsBinder(services, configuration);
            optionsBinder.BindAssemblies("ModernSlavery");

            //Register the web host dependencies
            var dependencyBuilder = new DependencyBuilder(services);
            dependencyBuilder.Bind<TModule>();
            return dependencyBuilder.Build();
        }
    }
}
