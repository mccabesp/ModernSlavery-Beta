using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Extensions;

namespace ModernSlavery.Infrastructure.Logging
{
    [ProviderAlias("QueuedLog")]
    public class EventLoggerProvider : ILoggerProvider
    {
        internal readonly string ApplicationName;
        private readonly IList<LoggerFilterRule> FilterRules;
        private readonly LogLevel MinLevel = LogLevel.Information;

        private readonly IQueue queue;

        public string Alias =
            typeof(EventLoggerProvider).GetAttributeValue((ProviderAliasAttribute attrib) => attrib.Alias);

        public EventLoggerProvider(IQueue queue, string applicationName, LoggerFilterOptions filterOptions)
        {
            if (string.IsNullOrWhiteSpace(applicationName)) throw new ArgumentNullException(nameof(applicationName));

            ApplicationName = applicationName;
            this.queue = queue ?? throw new ArgumentNullException(nameof(queue));

            if (filterOptions != null)
            {
                MinLevel = filterOptions.MinLevel;
                FilterRules = filterOptions.Rules.Where(r => r.ProviderName.EqualsI(Alias)).ToList();
                if (FilterRules == null || FilterRules.Count == 0)
                    FilterRules = filterOptions.Rules
                        .Where(r => string.IsNullOrWhiteSpace(r.ProviderName) || r.ProviderName.EqualsI("default"))
                        .ToList();
            }
        }

        public ILogger CreateLogger(string category)
        {
            var matchedCategory = FilterRules.FirstOrDefault(x => category.StartsWithI(x.CategoryName));
            if (matchedCategory == null)
                matchedCategory =
                    FilterRules.FirstOrDefault(x =>
                        string.IsNullOrWhiteSpace(x.CategoryName) || x.CategoryName.EqualsI("default"));

            var minLevel = MinLevel;
            Func<string, string, LogLevel, bool> filter = null;
            if (matchedCategory != null)
            {
                if (matchedCategory.LogLevel.HasValue) minLevel = matchedCategory.LogLevel.Value;

                filter = matchedCategory.Filter;
            }

            return new EventLogger(this, category, minLevel, filter);
        }

        public void Dispose()
        {
        }

        public async Task WriteAsync(LogLevel logLevel, LogEntryModel logEntry)
        {
            var wrapper = new LogEventWrapperModel
                {ApplicationName = ApplicationName, LogLevel = logLevel, LogEntry = logEntry};

            await queue.AddMessageAsync(wrapper);
        }
    }
}