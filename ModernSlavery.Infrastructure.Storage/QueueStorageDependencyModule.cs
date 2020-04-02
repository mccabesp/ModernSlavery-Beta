using System;
using Autofac;
using ModernSlavery.Core;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Storage.MessageQueues;

namespace ModernSlavery.Infrastructure.Storage
{
    public class QueueStorageDependencyModule : IDependencyModule
    {
        private readonly StorageOptions _options;

        public QueueStorageDependencyModule(StorageOptions options)
        {
            _options = options;
        }

        public void Register(IDependencyBuilder builder)
        {
            // Register queues
            builder.Autofac.RegisterAzureQueue(_options.AzureConnectionString, QueueNames.SendEmail);
            builder.Autofac.RegisterAzureQueue(_options.AzureConnectionString, QueueNames.SendNotifyEmail);
            builder.Autofac.RegisterAzureQueue(_options.AzureConnectionString, QueueNames.ExecuteWebJob);
        }

        public void Configure(ILifetimeScope lifetimeScope)
        {
            //TODO: Add configuration here
        }
    }
}