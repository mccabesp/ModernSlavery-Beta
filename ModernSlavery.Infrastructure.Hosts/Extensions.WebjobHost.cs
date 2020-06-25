using Autofac;
using Autofac.Core.Lifetime;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Configuration;
using System.Collections.Generic;

namespace ModernSlavery.Infrastructure.Hosts
{
    public static class WebjobHost
    {
        public static IHostBuilder ConfigureWebjobHostBuilder<TStartupModule>(string applicationName = null, Dictionary<string, string> additionalSettings = null, params string[] commandlineArgs) where TStartupModule : class, IDependencyModule
        {
            var genericHost = Extensions.CreateGenericHost<TStartupModule>(applicationName, additionalSettings, commandlineArgs);
            //Register the callback to configure the web jobs
            genericHost.HostBuilder.ConfigureWebJobs(webJobsBuilder =>
            {
                genericHost.DependencyBuilder.Container_OnBuild += (lifetimeScope) => ConfigureHost(lifetimeScope,genericHost.DependencyBuilder,webJobsBuilder);
            });
            return genericHost.HostBuilder;
        }

        private static void ConfigureHost(ILifetimeScope lifeTimeScope, DependencyBuilder dependencyBuilder, IWebJobsBuilder webJobsBuilder)
        {
            //Only add the appbuilder temporarily
            using ILifetimeScope innerScope = lifeTimeScope.BeginLifetimeScope(b =>b.RegisterInstance(webJobsBuilder).SingleInstance().ExternallyOwned());

            dependencyBuilder.ConfigureHost(innerScope);
        }
    }
}