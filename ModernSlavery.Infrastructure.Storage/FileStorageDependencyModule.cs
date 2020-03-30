using System;
using Autofac;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Interfaces;
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

        public void Register(IDependencyBuilder builder)
        {
            // use the 'localStorageRoot' when hosting the storage in a local folder
            if (string.IsNullOrWhiteSpace(_options.LocalStorageRoot))
                builder.Autofac.Register(
                        c => new AzureFileRepository(_options,
                            new ExponentialRetry(TimeSpan.FromMilliseconds(500), 10)))
                    .As<IFileRepository>()
                    .SingleInstance();
            else
                builder.Autofac.Register(c => new SystemFileRepository(_options)).As<IFileRepository>()
                    .SingleInstance();
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Add configuration here
        }
    }
}