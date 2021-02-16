using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Options;
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

        public void ConfigureServices(IServiceCollection services)
        {
            //TODO: Register service dependencies here
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Register queues (without key filtering)
            builder.Register(c => new LogRecordQueue(_storageOptions.AzureConnectionString, c.Resolve<IFileRepository>()))
                .SingleInstance();

            // Register record loggers
            builder.RegisterLogRecord(Filenames.BadSicLog);
            builder.RegisterLogRecord(Filenames.ManualChangeLog);
            builder.RegisterLogRecord(Filenames.RegistrationLog);
            builder.RegisterLogRecord(Filenames.SubmissionLog);
            builder.RegisterLogRecord(Filenames.SearchLog);

            // Register log records (without key filtering)
            builder.RegisterType<UserAuditLogger>().As<IUserLogger>().SingleInstance();
            builder.RegisterType<RegistrationAuditLogger>().As<IRegistrationLogger>().SingleInstance();

            builder.RegisterType<SeriEventLogger>().As<IEventLogger>().SingleInstance();
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