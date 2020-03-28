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
        private readonly StorageOptions _storageOptions;

        public DependencyModule(StorageOptions options)
        {
            _storageOptions = options;
        }

        public bool AutoSetup { get; } = false;

        public void Register(IDependencyBuilder builder)
        {
            // Register queues (without key filtering)
            builder.Autofac
                .Register(c => new LogEventQueue(_storageOptions.AzureConnectionString, c.Resolve<IFileRepository>()))
                .SingleInstance();
            builder.Autofac
                .Register(c => new LogRecordQueue(_storageOptions.AzureConnectionString, c.Resolve<IFileRepository>()))
                .SingleInstance();

            // Register record loggers
            builder.Autofac.RegisterLogRecord(Filenames.BadSicLog);
            builder.Autofac.RegisterLogRecord(Filenames.ManualChangeLog);
            builder.Autofac.RegisterLogRecord(Filenames.RegistrationLog);
            builder.Autofac.RegisterLogRecord(Filenames.SubmissionLog);
            builder.Autofac.RegisterLogRecord(Filenames.SearchLog);

            // Register log records (without key filtering)
            builder.Autofac.RegisterType<UserAuditLogger>().As<IUserLogger>().SingleInstance();
            builder.Autofac.RegisterType<RegistrationAuditLogger>().As<IRegistrationLogger>().SingleInstance();
        }

        public void Configure(IServiceProvider serviceProvider, IContainer container)
        {
            //TODO: Add configuration here
        }
    }
}