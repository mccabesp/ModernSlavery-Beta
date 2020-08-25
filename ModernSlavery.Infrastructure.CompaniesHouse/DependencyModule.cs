using System;
using System.Collections.Generic;
using System.Net.Http;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Options;

namespace ModernSlavery.Infrastructure.CompaniesHouse
{
    public class DependencyModule : IDependencyModule
    {
        private readonly CompaniesHouseOptions _companiesHouseOptions;

        private readonly ILogger _logger;
        public DependencyModule(
            ILogger<DependencyModule> logger,
            CompaniesHouseOptions companiesHouseOptions
        )
        {
            _logger = logger;
            _companiesHouseOptions = companiesHouseOptions;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Add a dedicated httpclient for Companies house API with exponential retry policy
            services.AddHttpClient<ICompaniesHouseAPI, CompaniesHouseAPI>(nameof(ICompaniesHouseAPI),
                    httpClient =>
                    {
                        CompaniesHouseAPI.SetupHttpClient(httpClient, _companiesHouseOptions.ApiServer, _companiesHouseOptions.ApiKey);
                    })
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(CompaniesHouseAPI.GetRetryPolicy());

        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterType<CompaniesHouseAPI>()
                .As<ICompaniesHouseAPI>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(ICompaniesHouseAPI)));

            builder.RegisterType<PostcodeChecker>().As<IPostcodeChecker>().SingleInstance();
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