using Autofac;
using Microsoft.Azure.Search;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.SharedKernel;
using ModernSlavery.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.Search
{
    public class SearchDependencyModule: IDependencyModule
    {
        private readonly SearchOptions _options;
        public SearchDependencyModule(SearchOptions options)
        {
            _options = options;
        }

        public void Bind(ContainerBuilder builder)
        {
            // Setup azure search
            builder.Register(c => new SearchServiceClient(_options.AzureServiceName, new SearchCredentials(_options.AzureApiAdminKey)))
                .As<ISearchServiceClient>()
                .SingleInstance();

            builder.RegisterType<AzureEmployerSearchRepository>()
                .As<ISearchRepository<EmployerSearchModel>>()
                .SingleInstance()
                .WithParameter("serviceName", _options.AzureServiceName)
                .WithParameter("indexName", _options.EmployerIndexName)
                .WithParameter("adminApiKey", _options.AzureApiAdminKey)
                .WithParameter("disabled", _options.Disabled);

            builder.RegisterType<AzureSicCodeSearchRepository>()
                .As<ISearchRepository<SicCodeSearchModel>>()
                .SingleInstance()
                .WithParameter("indexName", _options.SicCodeIndexName)
                .WithParameter("disabled", _options.Disabled);


        }
    }
}
