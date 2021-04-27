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
            services.AddSingleton<ICompaniesHouseAPI, CompaniesHouseAPI>();
            services.AddHttpClient<ICompaniesHouseAPI, CompaniesHouseAPI>(httpClient => httpClient.SetupHttpClient(_companiesHouseOptions.ApiServer))
                .SetHandlerLifetime(TimeSpan.FromHours(1))
                .AddPolicyHandler(_companiesHouseOptions.RetryPolicy==Core.RetryPolicyTypes.Exponential ? Resilience.GetExponentialAsyncRetryPolicy() : Resilience.GetLinearAsyncRetryPolicy());

            //Add a dedicated httpclient for post code checker with retry policy
            services.AddSingleton<IPostcodeChecker, PostcodeChecker>();
            services.AddHttpClient<IPostcodeChecker, PostcodeChecker>(httpClient => httpClient.SetupHttpClient(_postcodeCheckerOptions.ApiServer))
                .SetHandlerLifetime(TimeSpan.FromHours(1))
                .AddPolicyHandler(_postcodeCheckerOptions.RetryPolicy == Core.RetryPolicyTypes.Exponential ? Resilience.GetExponentialAsyncRetryPolicy() : Resilience.GetLinearAsyncRetryPolicy());
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