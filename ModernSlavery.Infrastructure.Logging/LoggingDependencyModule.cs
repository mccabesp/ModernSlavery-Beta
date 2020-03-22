using Autofac;
using Microsoft.Extensions.DependencyInjection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Core.SharedKernel.Interfaces;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Storage.MessageQueues;

namespace ModernSlavery.Infrastructure.Logging
{
    public class LoggingDependencyModule: IDependencyModule
    {
        private readonly StorageOptions _options;
        public LoggingDependencyModule(StorageOptions options)
        {
            _options = options;
        }

        public void Bind(ContainerBuilder builder, IServiceCollection services)
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
            builder.RegisterType<UserLogger>().As<IUserLogger>().SingleInstance();
            builder.RegisterType<RegistrationLogger>().As<IRegistrationLogger>().SingleInstance();

        }
    }
}
