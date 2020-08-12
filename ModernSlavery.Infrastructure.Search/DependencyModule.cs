using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Azure.Search;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Options;

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
            //TODO: Register service dependencies here
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Setup azure search
            builder.Register(c =>
                    new SearchServiceClient(_options.ServiceName,
                        new SearchCredentials(_options.AdminApiKey)))
                .As<ISearchServiceClient>()
                .SingleInstance();

            builder.RegisterType<AzureOrganisationSearchRepository>()
                .As<ISearchRepository<OrganisationSearchModel>>()
                .SingleInstance().WithAttributeFiltering();
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