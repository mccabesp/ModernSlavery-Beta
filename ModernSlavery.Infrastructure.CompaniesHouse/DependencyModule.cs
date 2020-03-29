using System;
using System.Net.Http;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Attributes;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.CompaniesHouse
{
    [AutoRegister]
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

        public void Register(IDependencyBuilder builder)
        {
            //Add a dedicated httpclient for Companies house API with exponential retry policy
            builder.Services.AddHttpClient<ICompaniesHouseAPI, CompaniesHouseAPI>(nameof(ICompaniesHouseAPI),
                    httpClient =>
                    {
                        CompaniesHouseAPI.SetupHttpClient(httpClient, _companiesHouseOptions.ApiServer, _companiesHouseOptions.ApiKey);
                    })
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(CompaniesHouseAPI.GetRetryPolicy());

            builder.Autofac.RegisterType<CompaniesHouseAPI>()
                .As<ICompaniesHouseAPI>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(ICompaniesHouseAPI)));
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Add configuration here
        }
    }
}