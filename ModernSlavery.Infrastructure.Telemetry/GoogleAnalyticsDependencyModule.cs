using System;
using System.Net;
using System.Net.Http;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Core.SharedKernel.Options;
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

        public void Register(IDependencyBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(_sharedOptions.GoogleAnalyticsAccountId))
            {
                if (_sharedOptions.IsProduction())
                    throw new ArgumentNullException(nameof(_sharedOptions.GoogleAnalyticsAccountId));

                builder.Autofac.RegisterType<FakeWebTracker>().As<IWebTracker>().SingleInstance();
                return;
            }

            //Add a dedicated httpclient for Google Analytics tracking with exponential retry policy
            builder.Services.AddHttpClient<IWebTracker, GoogleAnalyticsTracker>(nameof(IWebTracker), client =>
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

            //Register WebTracker
            builder.Autofac.RegisterType<GoogleAnalyticsTracker>()
                .As<IWebTracker>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(IWebTracker)))
                .WithParameter("trackingId", _sharedOptions.GoogleAnalyticsAccountId);
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Add configuration here
        }
    }
}