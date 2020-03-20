using System;
using System.Linq;
using Autofac;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Storage.Classes;
using ModernSlavery.SharedKernel;
using ModernSlavery.SharedKernel.Interfaces;
using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.Infrastructure.Storage
{
    public class LogStorageDependencyModule: IDependencyModule
    {
        private readonly StorageOptions _options;
        public LogStorageDependencyModule(StorageOptions options)
        {
            _options = options;
        }

        public void Bind(ContainerBuilder builder)
        {
            // Register queues (without key filtering)
            builder.Register(c => new LogEventQueue(_options.AzureConnectionString, c.Resolve<IFileRepository>())).SingleInstance();
            builder.Register(c => new LogRecordQueue(_options.AzureConnectionString, c.Resolve<IFileRepository>())).SingleInstance();

            // Register record loggers
            builder.RegisterLogRecord(Filenames.BadSicLog);
            builder.RegisterLogRecord(Filenames.ManualChangeLog);
            builder.RegisterLogRecord(Filenames.RegistrationLog);
            builder.RegisterLogRecord(Filenames.SubmissionLog);
            builder.RegisterLogRecord(Filenames.SearchLog);

            // Register log records (without key filtering)
            builder.RegisterType<UserLogRecord>().As<IUserLogRecord>().SingleInstance();
            builder.RegisterType<RegistrationLogRecord>().As<IRegistrationLogRecord>().SingleInstance();

        }
    }
}
