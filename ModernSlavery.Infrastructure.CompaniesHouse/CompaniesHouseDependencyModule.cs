using System;
using System.Net.Http;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.CompaniesHouse
{
    public class CompaniesHouseDependencyModule: IDependencyModule
    {
        private readonly CompaniesHouseOptions _options;
        public CompaniesHouseDependencyModule(CompaniesHouseOptions options)
        {
            _options= options;
        }

        public void Bind(ContainerBuilder builder, IServiceCollection services)
        {
            //Add a dedicated httpclient for Companies house API with exponential retry policy
            services.AddHttpClient<ICompaniesHouseAPI, CompaniesHouseAPI>(nameof(ICompaniesHouseAPI), (httpClient) =>
                {
                    CompaniesHouseAPI.SetupHttpClient(httpClient, _options.ApiServer, _options.ApiKey);
                })
                .SetHandlerLifetime(TimeSpan.FromMinutes(10))
                .AddPolicyHandler(CompaniesHouseAPI.GetRetryPolicy());

            builder.RegisterType<CompaniesHouseAPI>()
                .As<ICompaniesHouseAPI>()
                .SingleInstance()
                .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(ICompaniesHouseAPI)));

        }
    }
}
