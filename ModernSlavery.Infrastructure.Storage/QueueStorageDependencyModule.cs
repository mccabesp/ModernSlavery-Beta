using System;
using System.Linq;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Infrastructure.Storage.MessageQueues;

namespace ModernSlavery.Infrastructure.Storage
{
    public class QueueStorageDependencyModule: IDependencyModule
    {
        private readonly StorageOptions _options;
        public QueueStorageDependencyModule(StorageOptions options)
        {
            _options = options;
        }
        public bool AutoSetup { get; } = false;

        public void Register(DependencyBuilder builder)
        {
            // Register queues
            builder.ContainerBuilder.RegisterAzureQueue(_options.AzureConnectionString, QueueNames.SendEmail);
            builder.ContainerBuilder.RegisterAzureQueue(_options.AzureConnectionString, QueueNames.SendNotifyEmail);
            builder.ContainerBuilder.RegisterAzureQueue(_options.AzureConnectionString, QueueNames.ExecuteWebJob);
        }
        public void Configure(IContainer container)
        {
            //TODO: Add configuration here
        }
    }
}
