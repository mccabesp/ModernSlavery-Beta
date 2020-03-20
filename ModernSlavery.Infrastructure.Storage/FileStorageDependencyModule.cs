using System;
using Autofac;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Storage.Classes;
using ModernSlavery.SharedKernel.Interfaces;

namespace ModernSlavery.Infrastructure.Messaging
{
    public class FileStorageDependencyModule: IDependencyModule
    {
        private readonly StorageOptions _options;
        public FileStorageDependencyModule(StorageOptions options)
        {
            _options = options;
        }

        public void Bind(ContainerBuilder builder) 
        {
            // use the 'localStorageRoot' when hosting the storage in a local folder
            if (string.IsNullOrWhiteSpace(_options.LocalStorageRoot))
            {
                builder.Register(
                        c => new AzureFileRepository(_options,
                            new ExponentialRetry(TimeSpan.FromMilliseconds(500), 10)))
                    .As<IFileRepository>()
                    .SingleInstance();
            }
            else
            {
                builder.Register(c => new SystemFileRepository(_options)).As<IFileRepository>().SingleInstance();
            }
        }
    }
}
