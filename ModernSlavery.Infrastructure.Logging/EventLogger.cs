﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;

namespace ModernSlavery.Infrastructure.Logging
{
    public class EventLogger : ILogger
    {
        private readonly string _category;
        private readonly Func<string, string, LogLevel, bool> _filter;
        private readonly LogLevel _minLevel;
        internal readonly EventLoggerProvider _provider;

        public EventLogger(EventLoggerProvider provider,
            string category,
            LogLevel minLevel,
            Func<string, string, LogLevel, bool> filter)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentNullException(nameof(category));

            _category = category;
            _minLevel = minLevel;
            _filter = filter;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _minLevel && (_filter == null || _filter(_provider.Alias, _category, logLevel));
        }

        public async void Log<TState>(LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            //Check logging for this level is enabled
            if (!IsEnabled(logLevel)) return;
            
            var entry = new LogEntryModel {Message = formatter(state, exception)};

            if (exception != null)
            {
                entry.Message = Lists.ToDelimitedString(Environment.NewLine, null, exception.Message, entry.Message);
                entry.Stacktrace = string.IsNullOrWhiteSpace(exception.StackTrace)
                    ? Environment.StackTrace
                    : exception.StackTrace;
                var innerException = exception.GetInnermostException();
                if (innerException != null)
                {
                    entry.Stacktrace = Lists.ToDelimitedString(Environment.NewLine, null, innerException.StackTrace,
                        entry.Stacktrace);
                    entry.Details = Lists.ToDelimitedString(
                        Environment.NewLine,
                        null,
                        innerException.Message,
                        formatter(state, innerException));
                }
            }

            if (!string.IsNullOrWhiteSpace(_category)) entry.Source = _category;

            await _provider.WriteAsync(logLevel, entry);
        }
    }
}