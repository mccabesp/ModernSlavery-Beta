using System;
using Autofac;
using Autofac.Features.AttributeFilters;
using Microsoft.Azure.Search;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Infrastructure.Search
{
    public class DependencyModule : IDependencyModule
    {
        private readonly SearchOptions _options;

        public DependencyModule(SearchOptions options)
        {
            _options = options;
        }

        public void Register(IDependencyBuilder builder)
        {
            // Setup azure search
            builder.Autofac.Register(c =>
                    new SearchServiceClient(_options.AzureServiceName,
                        new SearchCredentials(_options.AzureApiAdminKey)))
                .As<ISearchServiceClient>()
                .SingleInstance();

            builder.Autofac.RegisterType<AzureEmployerSearchRepository>()
                .As<ISearchRepository<EmployerSearchModel>>()
                .SingleInstance()
                .WithParameter("serviceName", _options.AzureServiceName)
                .WithParameter("indexName", _options.EmployerIndexName)
                .WithParameter("adminApiKey", _options.AzureApiAdminKey)
                .WithParameter("disabled", _options.Disabled).WithAttributeFiltering();

            builder.Autofac.RegisterType<AzureSicCodeSearchRepository>()
                .As<ISearchRepository<SicCodeSearchModel>>()
                .SingleInstance()
                .WithParameter("indexName", _options.SicCodeIndexName)
                .WithParameter("disabled", _options.Disabled).WithAttributeFiltering();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Add configuration here
        }
    }
}