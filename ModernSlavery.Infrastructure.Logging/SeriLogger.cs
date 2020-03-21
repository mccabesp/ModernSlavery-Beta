using ModernSlavery.Core.Interfaces;
using Newtonsoft.Json;
using Serilog;

namespace ModernSlavery.Infrastructure.Logging
{
    public class SeriLogger : IEventLogger
    {
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
                JsonConvert.SerializeObject(values);
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