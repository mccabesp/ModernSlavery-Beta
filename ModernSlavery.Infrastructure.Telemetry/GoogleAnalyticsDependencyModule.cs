using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using Polly;
using Polly.Extensions.Http;

namespace ModernSlavery.Infrastructure.Telemetry
{
    public class GoogleAnalyticsDependencyModule : IDependencyModule
    {
        private readonly SharedOptions _sharedOptions;

        public GoogleAnalyticsDependencyModule(SharedOptions sharedOptions)
        {
            _sharedOptions = sharedOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Add a dedicated httpclient for Google Analytics tracking with exponential retry policy
            services.AddHttpClient<IWebTracker, GoogleAnalyticsTracker>(nameof(IWebTracker), client =>
            {
                client.BaseAddress = GoogleAnalyticsTracker.BaseUri;
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.ConnectionClose = false;
                ServicePointManager.FindServicePoint(client.BaseAddress).ConnectionLeaseTimeout = 60 * 1000;
            })
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(
                    //see https://developers.google.com/analytics/devguides/config/mgmt/v3/errors
                    HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(6,
                            retryAttempt => TimeSpan.FromMilliseconds(new Random().Next(1, 1000)) +
                                            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(_sharedOptions.GoogleAnalyticsAccountId))
            {
                if (_sharedOptions.IsProduction())
                    throw new ArgumentNullException(nameof(_sharedOptions.GoogleAnalyticsAccountId));

                builder.RegisterType<FakeWebTracker>().As<IWebTracker>().SingleInstance();
                return;
            }

            //Register WebTracker
            builder.RegisterType<GoogleAnalyticsTracker>()
                .As<IWebTracker>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(IWebTracker)))
                .WithParameter("trackingId", _sharedOptions.GoogleAnalyticsAccountId);
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