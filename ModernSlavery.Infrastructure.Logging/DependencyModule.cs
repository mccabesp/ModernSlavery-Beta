using System;
using Autofac;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Storage.MessageQueues;

namespace ModernSlavery.Infrastructure.Logging
{
    public class DependencyModule : IDependencyModule
    {
        private readonly StorageOptions _options;

        public DependencyModule(StorageOptions options)
        {
            _options = options;
        }

        public bool AutoSetup { get; } = false;

        public void Register(IDependencyBuilder builder)
        {
            // Register queues (without key filtering)
            builder.ContainerBuilder
                .Register(c => new LogEventQueue(_options.AzureConnectionString, c.Resolve<IFileRepository>()))
                .SingleInstance();
            builder.ContainerBuilder
                .Register(c => new LogRecordQueue(_options.AzureConnectionString, c.Resolve<IFileRepository>()))
                .SingleInstance();

            // Register record loggers
            builder.ContainerBuilder.RegisterLogRecord(Filenames.BadSicLog);
            builder.ContainerBuilder.RegisterLogRecord(Filenames.ManualChangeLog);
            builder.ContainerBuilder.RegisterLogRecord(Filenames.RegistrationLog);
            builder.ContainerBuilder.RegisterLogRecord(Filenames.SubmissionLog);
            builder.ContainerBuilder.RegisterLogRecord(Filenames.SearchLog);

            // Register log records (without key filtering)
            builder.ContainerBuilder.RegisterType<UserAuditLogger>().As<IUserLogger>().SingleInstance();
            builder.ContainerBuilder.RegisterType<RegistrationAuditLogger>().As<IRegistrationLogger>().SingleInstance();
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Add configuration here
        }
    }
}