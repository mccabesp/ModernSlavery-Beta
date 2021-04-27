using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;
using ModernSlavery.Core.Extensions;
using System.Net.Http;

namespace ModernSlavery.Infrastructure.Search
{
    public class DependencyModule : IDependencyModule
    {
        private readonly SearchOptions _options;

        public DependencyModule(SearchOptions options)
        {
            _options = options;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Add a dedicated httpclient for Companies house API with retry policy
            services.AddHttpClient<ISearchRepository<OrganisationSearchModel>, AzureOrganisationSearchRepository>(
                    nameof(AzureOrganisationSearchRepository),
                    httpClient => httpClient.SetupHttpClient($"https://{_options.ServiceName}.search.windows.net/"))
                .SetHandlerLifetime(TimeSpan.FromMinutes(10));
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Setup azure search
            builder.RegisterType<AzureOrganisationSearchRepository>()
                .As<ISearchRepository<OrganisationSearchModel>>()
                    .WithParameter(
                    (p, ctx) => p.ParameterType == typeof(HttpClient),
                    (p, ctx) => ctx.Resolve<IHttpClientFactory>().CreateClient(nameof(AzureOrganisationSearchRepository)))
                .SingleInstance()
                .WithAttributeFiltering();
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