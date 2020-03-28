using System;
using Autofac;
using Microsoft.Azure.Search;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.Search
{
    public class DependencyModule : IDependencyModule
    {
        private readonly SearchOptions _options;

        public DependencyModule(SearchOptions options)
        {
            _options = options;
        }

        public bool AutoSetup { get; } = false;

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
                .WithParameter("disabled", _options.Disabled);

            builder.Autofac.RegisterType<AzureSicCodeSearchRepository>()
                .As<ISearchRepository<SicCodeSearchModel>>()
                .SingleInstance()
                .WithParameter("indexName", _options.SicCodeIndexName)
                .WithParameter("disabled", _options.Disabled);
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Add configuration here
        }
    }
}