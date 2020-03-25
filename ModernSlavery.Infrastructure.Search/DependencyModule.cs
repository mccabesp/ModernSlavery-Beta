using System;
using Autofac;
using Microsoft.Azure.Search;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.Search
{
    public class DependencyModule: IDependencyModule
    {
        private readonly SearchOptions _options;
        public DependencyModule(SearchOptions options)
        {
            _options = options;
        }
        
        public bool AutoSetup { get; } = false;

        public void Register(DependencyBuilder builder)
        {
            // Setup azure search
            builder.ContainerBuilder.Register(c => new SearchServiceClient(_options.AzureServiceName, new SearchCredentials(_options.AzureApiAdminKey)))
                .As<ISearchServiceClient>()
                .SingleInstance();

            builder.ContainerBuilder.RegisterType<AzureEmployerSearchRepository>()
                .As<ISearchRepository<EmployerSearchModel>>()
                .SingleInstance()
                .WithParameter("serviceName", _options.AzureServiceName)
                .WithParameter("indexName", _options.EmployerIndexName)
                .WithParameter("adminApiKey", _options.AzureApiAdminKey)
                .WithParameter("disabled", _options.Disabled);

            builder.ContainerBuilder.RegisterType<AzureSicCodeSearchRepository>()
                .As<ISearchRepository<SicCodeSearchModel>>()
                .SingleInstance()
                .WithParameter("indexName", _options.SicCodeIndexName)
                .WithParameter("disabled", _options.Disabled);


        }

        public void Configure(IContainer container)
        {
            //TODO: Add configuration here
        }
    }
}
