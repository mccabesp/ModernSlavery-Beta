using System;
using System.Collections.Generic;
using System.Net.Http;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Infrastructure.Messaging.GovNotify
{
    public class DependencyModule : IDependencyModule
    {

        private readonly ILogger _logger;
        private readonly GovNotifyOptions _govNotifyOptions;
        private readonly SharedOptions _sharedOptions;

        public DependencyModule(
            ILogger<DependencyModule> logger,
            GovNotifyOptions govNotifyOptions,
            SharedOptions sharedOptions)
        {
            _logger = logger;
            _govNotifyOptions = govNotifyOptions;
            _sharedOptions = sharedOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGovNotifyAPI, GovNotifyAPI>();
            services.AddHttpClient<IGovNotifyAPI,GovNotifyAPI>(httpClient => httpClient.SetupHttpClient(_govNotifyOptions.ApiServer))
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(_sharedOptions.ApplicationName == $"ModernSlavery.Hosts.Web" ? Resilience.GetLinearAsyncRetryPolicy(3) : Resilience.GetExponentialAsyncRetryPolicy(5));
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Configure dependencies here
        }

        public void RegisterModules(IList<Type> modules)
        {
            //TODO: Add any linked dependency modules here
        }
    }
}