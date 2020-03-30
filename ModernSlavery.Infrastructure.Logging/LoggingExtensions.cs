using System;
using System.Diagnostics;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        public static ILoggerFactory UseLogEventQueueLogger(this ILifetimeScope lifetimeScope)
        {
            var loggerFactory = lifetimeScope.Resolve<ILoggerFactory>();

            // Resolve filter options
            var filterOptions = lifetimeScope.Resolve<IOptions<LoggerFilterOptions>>();

            // Resolve the keyed queue from autofac
            var logEventQueue = lifetimeScope.Resolve<LogEventQueue>();

            // Create the logging provider
            loggerFactory.AddProvider(new EventLoggerProvider(logEventQueue, AppDomain.CurrentDomain.FriendlyName,filterOptions.Value));

            return loggerFactory;
        }

        /// <summary>
        ///     Adds an azure queue logger to a LoggingBuilder
        /// </summary>
        /// <param name="builder"></param>
        public static ILoggingBuilder AddAzureQueueLogger(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton(ctx =>
                {
                    // Resolve filter options
                    var filterOptions = ctx.GetRequiredService<LoggerFilterOptions>();

                    // Resolve the keyed queue from autofac
                    var logEventQueue = ctx.GetRequiredService<LogEventQueue>();

                    return new EventLoggerProvider(logEventQueue, AppDomain.CurrentDomain.FriendlyName, filterOptions);
                });

            return builder;
        }
    }
}