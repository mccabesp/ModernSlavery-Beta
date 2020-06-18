using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Storage.FileRepositories;

namespace ModernSlavery.Infrastructure.Storage
{
    public class FileStorageDependencyModule : IDependencyModule
    {
        private readonly StorageOptions _options;

        public FileStorageDependencyModule(StorageOptions options)
        {
            _options = options;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: Register service dependencies here
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            // use the 'localStorageRoot' when hosting the storage in a local folder
            if (string.IsNullOrWhiteSpace(_options.LocalStorageRoot))
                builder.Register(
                        c => new AzureFileRepository(_options,
                            new ExponentialRetry(TimeSpan.FromMilliseconds(500), 10)))
                    .As<IFileRepository>()
                    .SingleInstance();
            else
                builder.Register(c => new SystemFileRepository(_options)).As<IFileRepository>()
                    .SingleInstance();
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