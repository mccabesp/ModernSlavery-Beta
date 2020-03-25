using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Storage.MessageQueues;

namespace ModernSlavery.Infrastructure.Logging
{
    public static class LoggingExtensions
    {
        public static void RegisterLogRecord(this ContainerBuilder builder, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            var applicationName = AppDomain.CurrentDomain.FriendlyName;

            builder.RegisterType<AuditLogger>()
                .Keyed<IAuditLogger>(fileName)
                .SingleInstance()
                .WithParameter("applicationName", AppDomain.CurrentDomain.FriendlyName)
                .WithParameter("fileName", fileName);
        }

        /// <summary>
        ///     Adds the LogEvent queue as logging provider to the application
        /// </summary>
        /// <param name="factory"></param>
        public static ILoggerFactory UseLogEventQueueLogger(this ILoggerFactory factory,
            IServiceProvider serviceProvider)
        {
            // Resolve filter options
            var filterOptions = serviceProvider.GetService<LoggerFilterOptions>();

            // Resolve the keyed queue from autofac
            var lifetimeScope = serviceProvider.GetService<ILifetimeScope>();
            var logEventQueue = lifetimeScope.Resolve<LogEventQueue>();

            // Create the logging provider
            factory.AddProvider(new EventLoggerProvider(logEventQueue, AppDomain.CurrentDomain.FriendlyName,
                filterOptions));

            return factory;
        }

        /// <summary>
        ///     Adds an azure queue logger to a LoggingBuilder
        /// </summary>
        /// <param name="builder"></param>
        public static ILoggingBuilder AddAzureQueueLogger(this ILoggingBuilder builder)
        {
            // Build the current service provider
            var serviceProvider = builder.Services.BuildServiceProvider();

            // Resolve filter options
            var filterOptions = serviceProvider.GetService<LoggerFilterOptions>();

            // Resolve the keyed queue from autofac
            var lifetimeScope = serviceProvider.GetService<IContainer>();
            var logEventQueue = lifetimeScope.Resolve<LogEventQueue>();

            // Register the logging provider
            return builder.AddProvider(new EventLoggerProvider(logEventQueue, AppDomain.CurrentDomain.FriendlyName,
                filterOptions));
        }
    }
}
