using System;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Infrastructure.Storage.FileRepositories;

namespace ModernSlavery.Infrastructure.Storage
{
    public class FileStorageDependencyModule: IDependencyModule
    {
        private readonly StorageOptions _options;
        public FileStorageDependencyModule(StorageOptions options)
        {
            _options = options;
        }

        public void Bind(ContainerBuilder builder, IServiceCollection services)
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
