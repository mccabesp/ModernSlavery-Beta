using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Options;
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

        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: Register service dependencies here
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Register queues
            builder.RegisterAzureQueue(_options.AzureConnectionString, QueueNames.SendEmail);
            builder.RegisterAzureQueue(_options.AzureConnectionString, QueueNames.SendNotifyEmail);
            builder.RegisterAzureQueue(_options.AzureConnectionString, QueueNames.ExecuteWebJob);
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