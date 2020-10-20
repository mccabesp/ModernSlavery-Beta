﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Infrastructure.Storage.MessageQueues;

namespace ModernSlavery.Infrastructure.Logging
{
    public class AuditLogger : IAuditLogger
    {
        private readonly string fileName;

        private readonly IQueue queue;
        protected readonly SharedOptions SharedOptions;

        public AuditLogger(SharedOptions sharedOptions, LogRecordQueue queue, string fileName, string applicationName=null)
        {
            SharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            if (string.IsNullOrWhiteSpace(applicationName)) applicationName=sharedOptions.ApplicationName;
            if (string.IsNullOrWhiteSpace(applicationName)) applicationName= AppDomain.CurrentDomain.FriendlyName;

            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));
            ApplicationName = applicationName;
            this.fileName = fileName;
        }

        public string ApplicationName { get; }

        public async Task WriteAsync(IEnumerable<object> records)
        {
            foreach (var record in records) await WriteAsync(record);
        }

        public async Task WriteAsync(object record)
        {
            var wrapper = new LogRecordWrapperModel
                {ApplicationName = ApplicationName, FileName = fileName, Record = record};

            await queue.AddMessageAsync(wrapper);
        }
    }
}