using System;
using System.Collections.Generic;
using System.Net.Http;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Options;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Infrastructure.CompaniesHouse
{
    public class DependencyModule : IDependencyModule
    {
        private readonly CompaniesHouseOptions _companiesHouseOptions;
        private readonly PostcodeCheckerOptions _postcodeCheckerOptions;

        private readonly ILogger _logger;
        public DependencyModule(
            ILogger<DependencyModule> logger,
            CompaniesHouseOptions companiesHouseOptions,
            PostcodeCheckerOptions postcodeCheckerOptions
        )
        {
            _logger = logger;
            _companiesHouseOptions = companiesHouseOptions;
            _postcodeCheckerOptions = postcodeCheckerOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Add a dedicated httpclient for Companies house API with retry policy
            services.AddHttpClient<ICompaniesHouseAPI, CompaniesHouseAPI>(nameof(ICompaniesHouseAPI),httpClient => httpClient.SetupConnectionLease(_companiesHouseOptions.ApiServer))
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(_companiesHouseOptions.RetryPolicy==Core.RetryPolicyTypes.Exponential ? CompaniesHouseAPI.GetExponentialRetryPolicy() : CompaniesHouseAPI.GetLinearRetryPolicy());

            //Add a dedicated httpclient for post code checker with retry policy
            services.AddHttpClient<IPostcodeChecker, PostcodeChecker>(nameof(IPostcodeChecker),httpClient => httpClient.SetupConnectionLease(_postcodeCheckerOptions.ApiServer))
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(_postcodeCheckerOptions.RetryPolicy == Core.RetryPolicyTypes.Exponential ? PostcodeChecker.GetExponentialRetryPolicy() : PostcodeChecker.GetLinearRetryPolicy());

        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<CompaniesHouseAPI>()
                .As<ICompaniesHouseAPI>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(ICompaniesHouseAPI)));

            builder.RegisterType<PostcodeChecker>()
            .As<IPostcodeChecker>()
            .SingleInstance()
            .WithParameter(
                (p, ctx) => p.ParameterType == typeof(HttpClient),
                (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(IPostcodeChecker)));
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