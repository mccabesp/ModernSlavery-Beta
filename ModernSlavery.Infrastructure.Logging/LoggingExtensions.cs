using System;
using Autofac;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Infrastructure.Storage.MessageQueues;
using Microsoft.Extensions.Configuration;

namespace ModernSlavery.Infrastructure.Logging
{
    public static class LoggingExtensions
    {
        public static void RegisterLogRecord(this ContainerBuilder builder, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            builder.RegisterType<AuditLogger>()
                .As<IAuditLogger>()
                .Keyed<IAuditLogger>(fileName)
                .SingleInstance()
                .WithParameter("fileName", fileName);
        }

        /// <summary>
        ///     Adds the LogEvent queue as logging provider to the application
        /// </summary>
        /// <param name="factory"></param>
        public static ILoggerFactory UseLogEventQueueLogger(this ILifetimeScope lifetimeScope)
        {
            var loggerFactory = lifetimeScope.Resolve<ILoggerFactory>();

            // Resolve filter options
            var filterOptions = lifetimeScope.Resolve<IOptions<LoggerFilterOptions>>();

            // Resolve the keyed queue from autofac
            var logEventQueue = lifetimeScope.Resolve<LogEventQueue>();

            var configuration = lifetimeScope.Resolve<IConfiguration>();

            // Create the logging provider
            loggerFactory.AddProvider(new EventLoggerProvider(logEventQueue, filterOptions.Value, configuration.GetApplicationName()));

            return loggerFactory;
        }
    }
}