using Microsoft.ApplicationInsights.Extensibility;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;

namespace ModernSlavery.Infrastructure.Logging
{
    public class SeriEventLogger : IEventLogger
    {
        private readonly Logger log;
        public SeriEventLogger(SharedOptions sharedOptions, TelemetryConfiguration telemetryConfiguration)
        {
            if (sharedOptions.IsDevelopment())
                log = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            else
                log = new LoggerConfiguration().WriteTo.ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces).CreateLogger();

            Log.Logger = log;
        }

        public void Debug(string message, object values = null)
        {
            Log.Debug(GetLogMessage(message, values), values);
        }

        public void Information(string message, object values = null)
        {
            Log.Information(GetLogMessage(message, values), values);
        }

        public void Warning(string message, object values = null)
        {
            Log.Warning(GetLogMessage(message, values), values);
        }

        public void Error(string message, object values = null)
        {
            Log.Error(GetLogMessage(message, values), values);
        }

        public void Fatal(string message, object values = null)
        {
            Log.Fatal(GetLogMessage(message, values), values);
        }

        private string GetLogMessage(string message, object values = null)
        {
            try
            {
                // The logger doesn't use JsonConvert but it is doing something similar
                // We try to convert values to JSON to see if it will throw an exception
                Core.Extensions.Json.SerializeObject(values);
                return values == null ? message : message + ". Log: {@Values}";
            }
            catch (JsonSerializationException ex)
            {
                Error("SERILOG ERROR: Can't serialize values");
                throw;
            }
        }
    }
}