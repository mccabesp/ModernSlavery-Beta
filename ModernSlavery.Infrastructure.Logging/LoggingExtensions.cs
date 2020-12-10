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
    }
}