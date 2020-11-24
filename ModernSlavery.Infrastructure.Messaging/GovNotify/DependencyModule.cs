using System;
using System.Collections.Generic;
using System.Net.Http;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            var linearRetry = _sharedOptions.ApplicationName == $"ModernSlavery.Hosts.Web";

            services.AddHttpClient<GovNotifyAPI>(nameof(GovNotifyAPI),
                httpClient =>
                {
                    GovNotifyAPI.SetupHttpClient(httpClient, _govNotifyOptions.ApiServer);
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(GovNotifyAPI.GetRetryPolicy(linearRetry));
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<GovNotifyAPI>().As<IGovNotifyAPI>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(GovNotifyAPI)));
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