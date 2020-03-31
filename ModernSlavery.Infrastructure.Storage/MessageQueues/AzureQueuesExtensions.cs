using System;
using Autofac;
using ModernSlavery.Core.Interfaces;

namespace ModernSlavery.Infrastructure.Storage.MessageQueues
{
    public static class AzureQueuesExtensions
    {
        public static void RegisterAzureQueue(this ContainerBuilder builder,
            string azureConnectionString,
            string queueName,
            bool supportLargeMessages = true)
        {
            if (string.IsNullOrWhiteSpace(azureConnectionString))
                throw new ArgumentNullException(nameof(azureConnectionString));

            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentNullException(nameof(queueName));

            if (supportLargeMessages)
                builder.RegisterType<AzureQueue>()
                    .Keyed<IQueue>(queueName)
                    .SingleInstance()
                    .WithParameter("connectionString", azureConnectionString)
                    .WithParameter("queueName", queueName);

            else
                builder.RegisterType<AzureQueue>()
                    .Keyed<IQueue>(queueName)
                    .SingleInstance()
                    .WithParameter("connectionString", azureConnectionString)
                    .WithParameter("queueName", queueName)
                    .WithParameter("fileRepository", null);

        }
    }
}