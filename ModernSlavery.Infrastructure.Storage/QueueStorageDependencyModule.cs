using System;
using Autofac;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;
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

        public bool AutoSetup { get; } = false;

        public void Register(IDependencyBuilder builder)
        {
            // Register queues
            builder.Autofac.RegisterAzureQueue(_options.AzureConnectionString, QueueNames.SendEmail);
            builder.Autofac.RegisterAzureQueue(_options.AzureConnectionString, QueueNames.SendNotifyEmail);
            builder.Autofac.RegisterAzureQueue(_options.AzureConnectionString, QueueNames.ExecuteWebJob);
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Add configuration here
        }
    }
}