using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.WebUI.Identity
{
    public class DependencyModule : IDependencyModule
    {        
        private readonly ILogger _logger;

        public DependencyModule(ILogger<DependencyModule> logger)
        {
            _logger = logger;
        }

        public void ConfigureServices(IServiceCollection services)
        {
           
        }


        public void ConfigureContainer(ContainerBuilder builder)
        {
           
        }


        public void Configure(ILifetimeScope lifetimeScope)
        {
            
        }

        public void RegisterModules(IList<Type> modules)
        {
            

        }
    }
}