using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Core.SharedKernel.Options;
using ModernSlavery.Infrastructure.Storage.MessageQueues;

namespace ModernSlavery.Infrastructure.Logging
{
    public class RecordLogger : IRecordLogger
    {
        private readonly string fileName;
        protected readonly SharedOptions SharedOptions;

        private readonly IQueue queue;

        public RecordLogger(SharedOptions sharedOptions, LogRecordQueue queue, string applicationName,
            string fileName)
        {
            SharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            if (string.IsNullOrWhiteSpace(applicationName)) throw new ArgumentNullException(nameof(applicationName));

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